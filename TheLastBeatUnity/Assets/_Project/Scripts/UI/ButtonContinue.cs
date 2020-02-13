using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using UnityEngine.SceneManagement;

public class ButtonContinue : ButtonSelection
{
    [SerializeField]
    AK.Wwise.State musicStateContinue = null;

    [SerializeField]
    Image fadeImage = null;
    [SerializeField]
    float fadeDuration = 2;

    protected override void TriggerButton()
    {
        base.TriggerButton();
        musicStateContinue.SetValue();
        DOTween.Sequence().Append(fadeImage.DOFade(1, fadeDuration)).AppendCallback(() => SceneManager.LoadScene("LD_MVP", LoadSceneMode.Single));
    }
}
