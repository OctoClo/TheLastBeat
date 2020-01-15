using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyWeaponHitbox : MonoBehaviour
{
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

    public bool playerInHitbox;
    Collider collid;

    private void Start()
    {
        PlayerInHitbox = false;
        collid = GetComponent<Collider>();
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
