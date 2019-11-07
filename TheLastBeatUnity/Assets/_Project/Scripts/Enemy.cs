using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

public class Enemy : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField]
    float speed;
    Rigidbody rb;

    [Header("Life")]
    [Range(1, 5)]
    [SerializeField]
    int maxLives;
    int lives;

    float showHurtTimer;
    float showHurtDuration;

    bool hasResetMaterial;
    Material material;

    bool isTarget;
    FocusZone focusZone;

    [Header("References")]
    [SerializeField] [Required]
    Transform player;

    private void Start()
    {
        TimeManager.Instance.AddEnemy(this);

        rb = GetComponent<Rigidbody>();

        lives = maxLives;
        showHurtTimer = 0;
        showHurtDuration = 0.1f;

        material = GetComponent<MeshRenderer>().material;
    }

    private void Update()
    {
        transform.LookAt(player);

        showHurtTimer -= Time.deltaTime;

        if (showHurtTimer <= 0 && !hasResetMaterial)
        {
            material.color = isTarget ? Color.green : Color.red;
            hasResetMaterial = true;
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
            Destroy(gameObject);
            return;
        }

        material.color = Color.blue;
        showHurtTimer = showHurtDuration;
        hasResetMaterial = false;
    }

    public void SetSelected(bool selected, FocusZone newFocusZone)
    {
        isTarget = selected;
        focusZone = newFocusZone;
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

    private void OnDestroy()
    {
        if (isTarget)
            focusZone.TargetDestroyed();
    }
}
