using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Doozy.Engine;
using Rewired;

public class CreditsBack : MonoBehaviour
{
    public static Rewired.Player player;

    private void Awake()
    {
        player = ReInput.players.GetPlayer(0);
    }

    private void Update()
    {
        if (player.GetAnyButtonDown())
        {
            Debug.Log("Sending back game event");
            GameEventMessage.SendEvent("Back");
        }
    }
}
