using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

public class InCombat : CameraState
{
    [SerializeField]
    Collider confinObject = null;

    public Collider ConfinObject => confinObject;

    float confinerWidth = 10;
    public float ConfinerWidth
    {
        get
        {
            return confinerWidth;
        }
        set
        {
            confinerWidth = Mathf.Abs(confinerWidth);
            Vector3 size = ConfinObject.GetComponent<BoxCollider>().size;
            ConfinObject.GetComponent<BoxCollider>().size = new Vector3(confinerWidth, size.y, size.z);
        }
    }

    public override void StateEnter()
    {
        confinObject.enabled = true;
        confinObject.transform.position = machine.virtualCam.Follow.position;
        CinemachineConfiner cf = CameraMachine.GetLiveCamera().GetComponent<CinemachineConfiner>();
        if (cf != null)
        {
            cf.m_BoundingVolume = confinObject;
        }
    }

    public override void StateExit()
    {
        confinObject.enabled = false;
    }
}
