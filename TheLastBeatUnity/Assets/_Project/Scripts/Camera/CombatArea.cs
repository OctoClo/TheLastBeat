using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;
using System.Linq;

public class CombatArea : MonoBehaviour
{
    [SerializeField]
    CameraManager camManager;

    [SerializeField]
    CinemachineTargetGroup groupTarget;

    [SerializeField]
    AnimationCurve weightModifier;

    [SerializeField]
    float ignoreIfFurtherThan = 10;

    float radius;

    // Start is called before the first frame update
    void Start()
    {
        radius = GetComponent<SphereCollider>().radius;
    }

    // Update is called once per frame
    void Update()
    {
        foreach(Transform tar in groupTarget.m_Targets.Select(x => x.target))
        {
            if (tar.CompareTag("Enemy"))
            {
                groupTarget.RemoveMember(tar);
                groupTarget.AddMember(tar, ComputeRatio(tar), 5);
            }
        }
    }

    float ComputeRatio(Transform trsf)
    {
        float ratio = Vector3.Distance(trsf.position, transform.parent.position) / radius;
        ratio = Mathf.Clamp(1 - ratio, 0, 1);
        return weightModifier.Evaluate(ratio);
    }

    void OnTriggerEnter(Collider coll)
    {
        if (coll.CompareTag("Enemy"))
        {
            groupTarget.AddMember(coll.transform, 0, 5);
        }
    }

    void OnTriggerExit(Collider coll)
    {
        if (coll.CompareTag("Enemy"))
        {
            groupTarget.RemoveMember(coll.transform);
        }
    }
}
