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
        transform.GetChild(0).gameObject.SetActive(lockTarget != null);
        if (lockTarget)
        {
            transform.GetChild(0).position = cam.WorldToScreenPoint(lockTarget.position);
        }
    }

    public void SetLockPoint(Transform trsf)
    {
        lockTarget = trsf;
        if (lockTarget)
            transform.GetChild(0).position = cam.WorldToScreenPoint(lockTarget.position);
    }
}
