using UnityEngine;
using UnityEngine.EventSystems;
using Doozy.Engine.UI;

public class ViewQuit : MonoBehaviour
{
    [SerializeField]
    GameObject defaultButton = null;
    UIButton defaultButtonUI = null;

    [SerializeField]
    EventSystem eventSystem = null;

    [SerializeField]
    AK.Wwise.Event showSound = null;
    [SerializeField]
    AK.Wwise.Event hideSound = null;

    private void Awake()
    {
        defaultButtonUI = defaultButton.GetComponent<UIButton>();
    }

    public void OnShow()
    {
        eventSystem.SetSelectedGameObject(defaultButton);
        defaultButtonUI.SelectButton();
        showSound.Post(gameObject);
    }

    public void OnHide()
    {
        hideSound.Post(gameObject);
    }
}
