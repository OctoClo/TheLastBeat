using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyWeaponHitbox : MonoBehaviour
{
    public bool playerInHitbox;
    public bool PlayerInHitbox
    {
        get
        {
            return playerInHitbox;
        }
        private set
        {
            playerInHitbox = value;
        }
    }

    private void Start()
    {
        PlayerInHitbox = false;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            PlayerInHitbox = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            PlayerInHitbox = false;
        }
    }
}
