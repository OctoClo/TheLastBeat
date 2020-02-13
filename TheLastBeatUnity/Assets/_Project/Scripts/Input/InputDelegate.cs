using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Rewired;

public class InputDelegate : MonoBehaviour
{
    [SerializeField]
    Inputable inputable = null;

    [SerializeField]
    Inputable obtainRewind = null;

    public Inputable Inputable
    {
        get
        {
            return inputable;
        }
        set
        {
            if (inputable != null)
                inputable.OnInputExit();

            inputable = value;
            if (value != null)
            {
                inputable.OnInputEnter();
            }
        }
    }
    Inputable firstInput = null;

    public static Rewired.Player player;
    public static InputDelegate Instance;

    private void Start()
    {
        firstInput = inputable;
        player = ReInput.players.GetPlayer(0);
        inputable.OnInputEnter();
        Instance = this;
    }

    public void ObtainAbility()
    {
        Inputable = obtainRewind;
    }

    public void ResetInput()
    {
        Inputable = firstInput;
    }

    public void ObtainRewind()
    {
        Inputable = obtainRewind;
    }

    private void Update()
    {
        if (inputable && !inputable.BlockInput)
        {
            inputable.ProcessInput(player);
        }
    }
}
