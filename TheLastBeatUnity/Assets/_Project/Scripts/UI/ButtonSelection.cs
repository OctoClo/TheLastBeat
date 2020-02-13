using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Rewired;
using Doozy.Engine;

public class ButtonSelection : MonoBehaviour
{
    [SerializeField]
    string doozyEvent = "";

    public static Rewired.Player player;
    bool selected = false;
    bool eventSent = false;

    [SerializeField] [Range(0, 1)]
    float deselectOpacity = 0.8f;

    [SerializeField]
    bool continueButton = false;

    [SerializeField]
    AK.Wwise.State musicStateContinue = null;

    Image image = null;
    Color transparentWhite = Color.white;

    private void Awake()
    {
        image = GetComponent<Image>();
        transparentWhite = new Color(1, 1, 1, deselectOpacity);
        player = ReInput.players.GetPlayer(0);
    }

    private void OnDisable()
    {
        eventSent = false;
    }

    public void OnSelect()
    {
        image.color = Color.white;
        selected = true;
    }

    public void OnDeselect()
    {
        image.color = transparentWhite;
        selected = false;
        eventSent = false;
    }

    private void Update()
    {
        if (player.GetButtonDown("UISubmit") && selected && !eventSent)
        {
            Debug.Log("Sending game event " + doozyEvent);
            GameEventMessage.SendEvent(doozyEvent);
            eventSent = true;
            if (continueButton)
                musicStateContinue.SetValue();
        }
    }
}
