using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

public class CameraArea : MonoBehaviour
{
    [SerializeField]
    string triggerEnter;

    [SerializeField]
    string triggerAtTag;

    [SerializeField]
    string triggerExit;

    void OnTriggerEnter(Collider collid)
    {
        if (collid.CompareTag(triggerAtTag))
        {
            CameraManager.Instance.CameraStateChange(triggerEnter);
        }
    }

    void OnTriggerExit(Collider collid)
    {
        if (collid.CompareTag(triggerAtTag))
        {
            CameraManager.Instance.CameraStateChange(triggerExit);
        }
    }
}
