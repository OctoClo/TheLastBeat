using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AlwaysFaceCamera : MonoBehaviour
{
    void Update()
    {
        transform.rotation = Quaternion.identity;
    }
}
