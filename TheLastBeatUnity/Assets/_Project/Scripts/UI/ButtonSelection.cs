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
    AK.Wwise.Event buttonSound = null;

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
        buttonSound.Post(gameObject);
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
            TriggerButton();
    }

    protected virtual void TriggerButton()
    {
        GameEventMessage.SendEvent(doozyEvent);
        eventSent = true;
    }
}
