using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAnimationModifier : MonoBehaviour
{

    [SerializeField]
    Player plyr;

    [SerializeField]
    Transform leftFoot;

    [SerializeField]
    Transform rightFoot;

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
