﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
using TMPro;
using DG.Tweening;

public class EnemyDeadEvent : GameEvent { public Enemy enemy; }

public class Enemy : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField]
    float speed;
    Rigidbody rb;

    [Header("Life")]
    [SerializeField]
    int maxLives = 0;
    int lives;
    [SerializeField]
    TextMeshProUGUI lifeText;

    [Header("Stun")]
    bool stunned;
    [SerializeField]
    float stunDuration;
    float stunTimer;
    [SerializeField] [Range(0.0f, 1.0f)]
    float[] chancesToGetStunned;
    [SerializeField]
    int stunCounter;

    [Header("References")]
    [SerializeField] [Required]
    Transform player;

    bool isTarget;
    FocusZone focusZone;

    Material material;

    private void Start()
    {
        TimeManager.Instance.AddEnemy(this);

        rb = GetComponent<Rigidbody>();

        lives = maxLives;
        lifeText.text = lives.ToString();
        stunTimer = 0;

        material = GetComponent<MeshRenderer>().material;

        stunCounter = 0;
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

    private void OnDestroy()
    {
        
    }
}
