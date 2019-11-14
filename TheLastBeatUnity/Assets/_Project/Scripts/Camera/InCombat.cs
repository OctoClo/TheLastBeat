using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

public class InCombat : CameraState
{
    [SerializeField]
    Collider confinObject;
    public Collider ConfinObject => confinObject;

    float width = 10;
    public float Width
    {
        get
        {
            return width;
        }
        set
        {
            width = Mathf.Abs(width);
            Vector3 size = ConfinObject.GetComponent<BoxCollider>().size;
            ConfinObject.GetComponent<BoxCollider>().size = new Vector3(width, size.y, size.z);
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

    public override void StateUpdate()
    {
    }
}
