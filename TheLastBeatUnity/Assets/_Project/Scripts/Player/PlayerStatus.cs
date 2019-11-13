using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

public class PlayerStatus : MonoBehaviour
{
    [TabGroup("Status")]
    public bool stunned = false;

    [TabGroup("Status")]
    public bool dashing = false;

    [TabGroup("Stun")] [SerializeField]
    float stunDuration = 0.5f;
    float stunTimer = 0;

    Material material = null;

    private void Start()
    {
        material = GetComponent<MeshRenderer>().material;
    }

    private void Update()
    {
        if (stunned)
        {
            stunTimer -= Time.deltaTime * Time.timeScale;

            if (stunTimer <= 0)
            {
                stunned = false;
                material.color = Color.white;
            }
        }
    }
}
