using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

public class InCombat : CameraState
{
    Vector3 confinOrigin;
    public void SetConfin(Vector3 pointConfin, GameObject gob)
    {
        gob.SetActive(true);
        gob.transform.position = pointConfin;
    }

    public override void StateEnter()
    {
    }

    public override void StateExit()
    {
        machine.GetComponent<CinemachineConfiner>().m_BoundingVolume.gameObject.SetActive(false);
    }

    public override void StateUpdate()
    {
    }
}
