using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Rewired;
using Doozy.Engine;

public class PauseEvent : GameEvent { public bool pause; }

public class GameManager : MonoBehaviour
{
    bool pause;

    private void Start()
    {
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