using UnityEngine;
using Sirenix.OdinInspector;
using DG.Tweening;
using UnityEngine.UI;
using Doozy.Engine;
using Rewired;

public class StartAnim : MonoBehaviour
{
    public static Rewired.Player player;

    [TabGroup("Animation")] [SerializeField]
    float waitBeforeShowLogo = 2f;
    [TabGroup("Animation")] [SerializeField]
    float logoFadeDuration = 2f;
    [TabGroup("Animation")] [SerializeField]
    float logoShowDuration = 2f;
    [TabGroup("Animation")] [SerializeField]
    float waitBeforeFadeBackground = 2f;
    [TabGroup("Animation")] [SerializeField]
    float backgroundFadeDuration = 2f;
    [TabGroup("Animation")] [SerializeField]
    float lightDuration = 2f;

    [TabGroup("References")] [SerializeField]
    GameObject logo = null;
    Image[] logoImages = null;
    [TabGroup("References")] [SerializeField]
    Image pressAnyButton = null;
    Image background = null;
    [TabGroup("References")] [SerializeField]
    GameObject nemesisLight = null;

    bool waitingForFirstInput = false;
    bool waitingForSecondInput = false;

    private void Start()
    {
        background = GetComponent<Image>();
        player = ReInput.players.GetPlayer(0);
        logoImages = logo.GetComponentsInChildren<Image>();
        // Monolith no pulse

        DOTween.Sequence()
            .AppendInterval(waitBeforeShowLogo)
            .InsertCallback(waitBeforeShowLogo, () =>
            {
                foreach (Image image in logoImages)
                    image.DOFade(1, logoFadeDuration);
            })
            .AppendInterval(logoShowDuration)
            .InsertCallback(waitBeforeShowLogo + logoFadeDuration + logoShowDuration, () =>
            {
                foreach (Image image in logoImages)
                    image.DOFade(0, logoFadeDuration);
            })
            .AppendInterval(waitBeforeFadeBackground)
            .Append(background.DOFade(0, backgroundFadeDuration))
            .AppendInterval(1)
            .AppendCallback(() => waitingForFirstInput = true)
            .Append(pressAnyButton.DOFade(1, logoFadeDuration));
    }

    private void Update()
    {
        if (player.GetAnyButtonDown())
        {
            if (waitingForFirstInput)
                FragileLight();
            if (waitingForSecondInput)
                LaunchMenu();
        }
    }

    private void FragileLight()
    {
        waitingForFirstInput = false;
        DOTween.Sequence()
            .Append(pressAnyButton.DOFade(0, logoFadeDuration))
            .InsertCallback(0.5f, () => nemesisLight.SetActive(true))
            .AppendInterval(lightDuration)
            .AppendCallback(() => nemesisLight.SetActive(false))
            .AppendCallback(() => waitingForSecondInput = true)
            .Append(pressAnyButton.DOFade(1, logoFadeDuration));
    }

    private void LaunchMenu()
    {
        waitingForSecondInput = false;
        DOTween.Sequence()
            .Append(pressAnyButton.DOFade(0, logoFadeDuration))
            .AppendCallback(() => nemesisLight.SetActive(true))
            // Monolith pulse
            // Display nemesis
            .AppendCallback(() => GameEventMessage.SendEvent("LaunchMenu"));
    }
}
