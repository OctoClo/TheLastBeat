using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LockPoint : MonoBehaviour
{
    Transform lockTarget;
    Camera cam;

    // Start is called before the first frame update
    void Start()
    {
        cam = Camera.main;
    }

    // Update is called once per frame
    void Update()
    {
        if (lockTarget)
        {
            transform.GetChild(0).position = cam.WorldToScreenPoint(lockTarget.position);
        }
        else
        {
            transform.GetChild(0).position = -Vector3.one * 100;
        }
    }

    public void SetLockPoint(Transform trsf)
    {
        lockTarget = trsf;
        transform.GetChild(0).position = cam.WorldToScreenPoint(lockTarget.position);
    }
}
