using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using UnityEngine.SceneManagement;

public class ButtonContinue : ButtonSelection
{
    [SerializeField]
    AK.Wwise.State musicStateContinue = null;

    [SerializeField]
    AK.Wwise.Event showSound = null;

    [SerializeField]
    Image fadeImage = null;
    [SerializeField]
    float fadeDuration = 2;

    protected override void TriggerButton()
    {
        showSound.Post(gameObject);
        musicStateContinue.SetValue();
        DOTween.Sequence().Append(fadeImage.DOFade(1, fadeDuration)).AppendCallback(() => SceneManager.LoadScene("LD_MVP", LoadSceneMode.Single));
    }
}
