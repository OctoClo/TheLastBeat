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
    [SerializeField]
    float knockbackStrength;
    [SerializeField]
    float knockbackDuration;
    float knockbackTimer;
    Material material;

    [Header("References")]
    [SerializeField] [Required]
    Transform player;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();

        lives = maxLives;
        knockbackTimer = 0;
        material = GetComponent<MeshRenderer>().material;
    }

    private void Update()
    {
        transform.LookAt(player);
        knockbackTimer -= Time.deltaTime;

        if (knockbackTimer <= 0)
            material.color = Color.red;
    }

    void FixedUpdate()
    {
        if (knockbackTimer <= 0)
        {
            Vector3 movement = (player.position - transform.position);

            if (movement.sqrMagnitude > 10)
            {
                movement.Normalize();
                rb.velocity = movement * speed;
            }
        }
    }

    public void GetAttacked()
    {
        if (knockbackTimer <= 0)
        {
            lives--;
            if (lives == 0)
            {
                Destroy(gameObject);
                return;
            }

            material.color = Color.blue;
            knockbackTimer = knockbackDuration;
            rb.velocity = -transform.forward * knockbackStrength;
        }
    }

    public void SetSelected(bool selected)
    {
        if (selected)
            material.color = Color.green;
        else
            material.color = Color.red;
    }
}
