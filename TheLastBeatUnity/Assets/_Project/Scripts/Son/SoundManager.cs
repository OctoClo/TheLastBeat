using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundManager : MonoBehaviour
{
    [SerializeField]
    AK.Wwise.Event ambStart;

    [SerializeField]
    AK.Wwise.Event musStart;

    [SerializeField]
    AK.Wwise.State musStateStart;

    void Start()
    {
        musStateStart.SetValue();
        ambStart.Post(gameObject);
        musStart.Post(gameObject);
    }

}
