using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    [SerializeField]
    float speed;

    [SerializeField]
    Transform player;

    Rigidbody rb;

    int lives = 3;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    void Update()
    {
        transform.LookAt(player);
        if ((transform.position - player.position).sqrMagnitude > 10)
            transform.position += transform.forward * Time.deltaTime * speed;
    }

    public void GetAttacked()
    {
        Debug.Log("Getting attacked");
        transform.position -= transform.forward * 2;
        lives--;
        if (lives == 0)
            Destroy(gameObject, 1);
    }
}
