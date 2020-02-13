using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using Doozy.Engine.UI;

public class ViewQuit : MonoBehaviour
{
    [SerializeField]
    GameObject defaultButton = null;
    UIButton defaultButtonUI = null;

    [SerializeField]
    GameObject returnButton = null;
    UIButton returnButtonUI = null;

    [SerializeField]
    EventSystem eventSystem = null;

    private void Awake()
    {
        defaultButtonUI = defaultButton.GetComponent<UIButton>();
        returnButtonUI = returnButton.GetComponent<UIButton>();
    }

    public void OnShow()
    {
        eventSystem.SetSelectedGameObject(defaultButton);
        defaultButtonUI.SelectButton();
    }

    public void OnHide()
    {
        eventSystem.SetSelectedGameObject(returnButton);
        returnButtonUI.DeselectButton();
        returnButtonUI.SelectButton();
    }
}
