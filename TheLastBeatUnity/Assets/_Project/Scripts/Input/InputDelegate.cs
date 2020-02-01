using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Rewired;

public class InputDelegate : MonoBehaviour
{
    [SerializeField]
    Inputable inputable = null;

    public static Rewired.Player player;

    private void Start()
    {
        player = ReInput.players.GetPlayer(0);
    }

    private void Update()
    {
        if (!inputable.BlockInput)
        {
            inputable.ProcessInput(player);
        }
    }
}
