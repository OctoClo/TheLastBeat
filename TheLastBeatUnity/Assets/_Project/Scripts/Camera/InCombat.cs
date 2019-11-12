using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

public class InCombat : CameraState
{
    Vector3 confinOrigin;
    GameObject confiner;

    public void SetConfin(Vector3 pointConfin, GameObject gob)
    {
        confiner = gob;
        gob.transform.position = pointConfin;
    }

    public override void StateEnter()
    {
        confiner.SetActive(true);
    }

    public override void StateExit()
    {
        confiner.SetActive(false);
    }

    public override void StateUpdate()
    {
    }
}
