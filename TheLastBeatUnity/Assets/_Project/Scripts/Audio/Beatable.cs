﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public abstract class Beatable : MonoBehaviour
{
    [SerializeField]
    protected float sequenceDuration;
    public float SequenceDuration => sequenceDuration;

    bool pause;
    [SerializeField]
    protected SoundManager.TypeBeat recordAs = SoundManager.TypeBeat.BEAT;

    public abstract void Beat();

    protected virtual void Start()
    {
        SoundManager.Instance.Add(this, recordAs);
    }

    private void OnEnable()
    {
        EventManager.Instance.AddListener<PauseEvent>(OnPauseEvent);
    }

    private void OnDisable()
    {
        EventManager.Instance.RemoveListener<PauseEvent>(OnPauseEvent);
    }

    private void OnPauseEvent(PauseEvent e)
    {
        pause = e.pause;
    }
}
