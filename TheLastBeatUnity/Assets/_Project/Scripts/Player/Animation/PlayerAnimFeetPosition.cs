using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAnimFeetPosition : MonoBehaviour
{
    [SerializeField]
    Player plyr = null;

    [SerializeField]
    Transform leftFoot = null;

    [SerializeField]
    AK.Wwise.Event leftFootSound = null;

    [SerializeField]
    Transform rightFoot = null;

    [SerializeField]
    AK.Wwise.Event rightFootSound = null;

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
                leftFootSound.Post(plyr.gameObject);
                break;

            case FootStatus.RIGHTFOOT:
                plyr.SetFoot(rightFoot);
                rightFootSound.Post(plyr.gameObject);
                break;

            default:
                plyr.SetFoot(null);
                break;
        }
    }
}
