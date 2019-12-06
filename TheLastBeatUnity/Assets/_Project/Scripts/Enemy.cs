using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
using TMPro;
using DG.Tweening;

public class EnemyDeadEvent : GameEvent { public Enemy enemy; }

public class Enemy : MonoBehaviour
{
    [Header("Movement")] [SerializeField]
    float speed = 5;
    Rigidbody rb = null;

    [Header("Life")]
    [SerializeField]
    int maxLives = 10;
    int lives = 10;
    [SerializeField]
    TextMeshProUGUI lifeText = null;

    [Header("Stun")]
    bool stunned = false;
    [SerializeField]
    float stunDuration = 1;
    float stunTimer = 0;
    [SerializeField] [Range(0.0f, 1.0f)]
    float[] chancesToGetStunned = new float[5];
    [SerializeField]
    int stunCounter = 0;
    

    [Header("References")]
    [SerializeField] [Required]
    Transform player = null;

    [SerializeField]
    float maxRotationPerFrame = 10;

    bool isTarget = false;
    FocusZone focusZone = null;
    Material material = null;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        material = GetComponent<MeshRenderer>().material;

        lives = maxLives;
        lifeText.text = lives.ToString();
    }

    private void Update()
    {
        if (stunTimer > 0)
        {
            stunTimer -= Time.deltaTime;

            if (stunTimer <= 0 && stunned)
            {
                material.color = isTarget ? Color.green : Color.red;
                stunned = false;
            }
        }

        Vector3 toPlayer = player.position - transform.position;
        toPlayer = new Vector3(toPlayer.x, 0, toPlayer.z);
        transform.rotation = Quaternion.RotateTowards(transform.rotation, Quaternion.LookRotation(toPlayer), maxRotationPerFrame);
    }

    void FixedUpdate()
    {
        Vector3 movement = (player.position - transform.position);

        if (movement.sqrMagnitude > 10)
        {
            movement.Normalize();
            rb.velocity = movement * speed;
        }
    }

    public void GetAttacked(bool onRythm)
    {
        lives--;
        if (lives == 0)
        {
            EventManager.Instance.Raise(new EnemyDeadEvent { enemy = this });
            Destroy(gameObject);
            return;
        }

        if (onRythm)
        {
            StartCoroutine(BlinkForOnRythmAttack());
        }

        lifeText.text = lives.ToString();
        if (stunCounter < chancesToGetStunned.Length)
        {
            float stunPercentage = Random.value;
            if (stunPercentage < chancesToGetStunned[stunCounter])
            {
                stunned = true;
                stunCounter++;
                stunTimer = stunDuration;
            }
        }
    }

    IEnumerator BlinkForOnRythmAttack()
    {
        material.color = Color.blue;
        yield return new WaitForSecondsRealtime(0.3f);
        UpdateColor();
    }

    public void SetFocusZone(FocusZone newFocusZone)
    {
        focusZone = newFocusZone;
    }

    public void SetSelected(bool selected)
    {
        isTarget = selected;
        UpdateColor();
    }

    void UpdateColor()
    {
        if (isTarget)
            material.color = Color.green;
        else
            material.color = Color.red;
    }
}
