﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyDetectionZone : MonoBehaviour
{
    public bool PlayerInZone { get; private set; }
    
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            PlayerInZone = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            PlayerInZone = false;
        }
    }
}
