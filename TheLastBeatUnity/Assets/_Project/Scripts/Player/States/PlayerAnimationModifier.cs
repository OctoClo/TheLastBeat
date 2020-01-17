using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAnimationModifier : MonoBehaviour
{

    [SerializeField]
    Player plyr = null;

    [SerializeField]
    Transform leftFoot = null;

    [SerializeField]
    Transform rightFoot = null;

    public enum FootStatus
    {
        RIGHTFOOT,
        LEFTFOOT,
        NONE
    }

    void UpdateFoot(FootStatus fs)
    {
        switch (fs)
        {
            case FootStatus.LEFTFOOT:
                plyr.SetFoot(leftFoot);
                break;

            case FootStatus.RIGHTFOOT:
                plyr.SetFoot(rightFoot);
                break;

            default:
                plyr.SetFoot(null);
                break;
        }
    }
}
