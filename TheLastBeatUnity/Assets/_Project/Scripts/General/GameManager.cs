using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Rewired;
using Doozy.Engine;
using DG.Tweening;

public class PauseEvent : GameEvent { public bool pause = false; }

public class GameManager : MonoBehaviour
{
    bool pause;

    private void Awake()
    {
        DOTween.SetTweensCapacity(2000, 300);
        pause = false;
    }

    void Update()
    {
        if (ReInput.players.GetPlayer(0).GetButtonDown("Pause"))
        {
            pause = !pause;
            GameEventMessage.SendEvent("PauseMenu");
            EventManager.Instance.Raise(new PauseEvent() { pause = pause });
        }
    }
}