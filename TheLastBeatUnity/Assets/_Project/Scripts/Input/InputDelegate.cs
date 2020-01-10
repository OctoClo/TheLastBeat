using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Rewired;

public class InputDelegate : MonoBehaviour
{
    [SerializeField]
    Inputable inputable = null;

    Rewired.Player player;

    public enum RythmLayout
    {
        NOPUNISH,
        PUNISH
    }

    public static RythmLayout rythm;
    [SerializeField]
    RythmLayout layout;

    private void Start()
    {
        player = ReInput.players.GetPlayer(0);
        rythm = layout;
    }

    private void Update()
    {
        if (!inputable.BlockInput)
        {
            inputable.ProcessInput(player);
        }
    }
}
