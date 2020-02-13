using UnityEngine;
using UnityEngine.EventSystems;
using Doozy.Engine.UI;

public class MainMenuSelectButton : MonoBehaviour
{
    [SerializeField]
    GameObject continueButton = null;
    UIButton continueButtonUI = null;
    [SerializeField]
    EventSystem eventSystem = null;

    private void Awake()
    {
        continueButtonUI = continueButton.GetComponent<UIButton>();
    }

    public void SelectButton()
    {
        eventSystem.SetSelectedGameObject(continueButton);
        continueButtonUI.DeselectButton();
        continueButtonUI.SelectButton();
    }
}
