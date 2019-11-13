using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

public class InCombat : CameraState
{
    [SerializeField]
    GameObject confinObject;
    public GameObject ConfinObject => confinObject;

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
        confinObject.SetActive(true);
        confinObject.transform.position = machine.virtualCam.Follow.position;
    }

    public override void StateExit()
    {
        confinObject.SetActive(false);
    }

    public override void StateUpdate()
    {
    }
}
