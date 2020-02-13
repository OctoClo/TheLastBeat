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
    Image pressAnyButtonWithoutRhythm = null;
    [TabGroup("References")] [SerializeField]
    Image pressAnyButtonWithRhythm = null;
    Image background = null;
    [TabGroup("References")] [SerializeField]
    GameObject nemesisLight = null;
    [TabGroup("References")] [SerializeField]
    AK.Wwise.State musicStatePressAnyButton1 = null;
    [TabGroup("References")] [SerializeField]
    AK.Wwise.State musicStatePressAnyButton2 = null;

    bool waitingForFirstInput = false;
    bool waitingForSecondInput = false;

    Rock[] rocks = null;

    private void Start()
    {
        background = GetComponent<Image>();
        player = ReInput.players.GetPlayer(0);
        logoImages = logo.GetComponentsInChildren<Image>();
        rocks = GameObject.FindObjectsOfType<Rock>();

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
            .AppendCallback(() =>
            {
                waitingForFirstInput = true;
                Debug.Log("music state 1");
                musicStatePressAnyButton1.SetValue();
            })
            .Append(pressAnyButtonWithoutRhythm.DOFade(1, logoFadeDuration));
    }

    private void Update()
    {
        if (player.GetAnyButtonDown())
        {
            if (waitingForFirstInput)
                FragileLight();
            if (waitingForSecondInput && SoundManagerMenu.Instance.IsInRythm(TimeManager.Instance.SampleCurrentTime()))
                LaunchMenu();
        }
    }

    private void FragileLight()
    {
        waitingForFirstInput = false;
        musicStatePressAnyButton2.SetValue();
        Debug.Log("music state 2");
        DOTween.Sequence()
            .Append(pressAnyButtonWithoutRhythm.DOFade(0, logoFadeDuration))
            .InsertCallback(0.5f, () => nemesisLight.SetActive(true))
            .AppendInterval(lightDuration)
            .AppendCallback(() => nemesisLight.SetActive(false))
            .AppendCallback(() => waitingForSecondInput = true)
            .Append(pressAnyButtonWithRhythm.DOFade(1, logoFadeDuration));
    }

    private void LaunchMenu()
    {
        waitingForSecondInput = false;
        DOTween.Sequence()
            .Append(pressAnyButtonWithRhythm.DOFade(0, logoFadeDuration))
            .AppendCallback(() => 
            {
                nemesisLight.SetActive(true);
                foreach (Rock rock in rocks)
                    rock.ChangeState(ERockState.PULSE_ON_BEAT);
            })
            // Display nemesis
            .AppendCallback(() => GameEventMessage.SendEvent("LaunchMenu"));
    }
}
