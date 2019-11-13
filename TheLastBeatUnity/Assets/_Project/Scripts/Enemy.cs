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

    bool isTarget = false;
    FocusZone focusZone = null;

    Material material = null;

    private void Start()
    {
        TimeManager.Instance.AddEnemy(this);

        rb = GetComponent<Rigidbody>();
        material = GetComponent<MeshRenderer>().material;

        lives = maxLives;
        lifeText.text = lives.ToString();
    }

    private void Update()
    {
        transform.DOLookAt(player.position, 1, AxisConstraint.Y);

        stunTimer -= Time.deltaTime;

        if (stunTimer <= 0 && stunned)
        {
            material.color = isTarget ? Color.green : Color.red;
            stunned = false;
        }
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

    public void GetAttacked()
    {
        lives--;
        if (lives == 0)
        {
            EventManager.Instance.Raise(new EnemyDeadEvent { enemy = this });
            Destroy(gameObject);
            return;
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
                material.color = Color.blue;
            }
        }
    }

    public void SetFocusZone(FocusZone newFocusZone)
    {
        focusZone = newFocusZone;
    }

    public void SetSelected(bool selected)
    {
        if (selected)
            material.color = Color.green;
        else
            material.color = Color.red;
    }

    public void Slow()
    {
        speed /= 10.0f;
    }

    public void ResetSpeed()
    {
        speed *= 10.0f;
    }
}
