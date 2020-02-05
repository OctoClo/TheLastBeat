using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PostWwiseEvent : MonoBehaviour
{
    [SerializeField]
    AK.Wwise.Event attackEvent = null;
    [SerializeField]
    AK.Wwise.Event callAttackEvent = null;

    public void PostAttack()
    {
        attackEvent.Post(gameObject);
    }


    public void PostCall()
    {
        callAttackEvent.Post(gameObject);
    }
}
