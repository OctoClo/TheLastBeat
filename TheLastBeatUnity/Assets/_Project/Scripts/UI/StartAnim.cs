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
    float nemesisApparitionDuration = 2f;
    [TabGroup("Animation")] [SerializeField]
    float nemesisFadeDuration = 0.5f;

    [TabGroup("References")] [SerializeField]
    GameObject logo = null;
    Image[] logoImages = null;
    [TabGroup("References")] [SerializeField]
    Image pressAnyButtonWithoutRhythm = null;
    [TabGroup("References")] [SerializeField]
    Image pressAnyButtonWithRhythm = null;
    Image background = null;
    [TabGroup("References")] [SerializeField]
    GameObject nemesis = null;
    SpriteRenderer nemesisSprite = null;
    [TabGroup("References")] [Header("Audio")] [SerializeField]
    AK.Wwise.Event logoMusic = null;
    [TabGroup("References")] [SerializeField]
    AK.Wwise.State musicStatePressAnyButton1 = null;
    [TabGroup("References")] [SerializeField]
    AK.Wwise.State musicStatePressAnyButton2 = null;

    bool waitingForFirstInput = false;
    bool waitingForSecondInput = false;

    Light nemesisLight = null;
    float nemesisLightIntensity = 0;
    Color transparentWhite = new Color(1, 1, 1, 0);
    Rock[] rocks = null;

    private void Start()
    {
        background = GetComponent<Image>();
        player = ReInput.players.GetPlayer(0);
        logoImages = logo.GetComponentsInChildren<Image>();
        rocks = GameObject.FindObjectsOfType<Rock>();
        nemesisSprite = nemesis.GetComponentInChildren<SpriteRenderer>();
        nemesisLight = nemesis.GetComponentInChildren<Light>();
        nemesisLightIntensity = nemesisLight.intensity;

        DOTween.Sequence()
            .AppendInterval(waitBeforeShowLogo)
            .InsertCallback(waitBeforeShowLogo, () =>
            {
                logoMusic.Post(gameObject);
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
        Debug.Log("music state 1");
        musicStatePressAnyButton1.SetValue();

        DOTween.Sequence()
            .Append(pressAnyButtonWithoutRhythm.DOFade(0, logoFadeDuration))
            .InsertCallback(0.5f, () => ShowNemesis(false))
            .AppendInterval(nemesisApparitionDuration + nemesisFadeDuration)
            .AppendCallback(() => 
            {
                DOTween.ToAlpha(() => nemesisSprite.color, x => nemesisSprite.color = x, 0, nemesisFadeDuration);
                DOTween.To(() => nemesisLight.intensity, x => nemesisLight.intensity = x, 0, nemesisFadeDuration);
            })
            .InsertCallback(logoFadeDuration + 0.5f + nemesisApparitionDuration + nemesisFadeDuration * 2, () =>
            {
                nemesis.SetActive(false);
                waitingForSecondInput = true;
                pressAnyButtonWithRhythm.DOFade(1, logoFadeDuration);
            });
    }

    private void LaunchMenu()
    {
        waitingForSecondInput = false;
        Debug.Log("music state 2");
        musicStatePressAnyButton2.SetValue();

        DOTween.Sequence()
            .Append(pressAnyButtonWithRhythm.DOFade(0, logoFadeDuration))
            .AppendCallback(() => 
            {
                ShowNemesis(true);
                foreach (Rock rock in rocks)
                    rock.ChangeState(ERockState.PULSE_ON_BEAT);
            })
            .AppendCallback(() => GameEventMessage.SendEvent("LaunchMenu"));
    }

    void ShowNemesis(bool showParticles)
    {
        nemesisLight.intensity = 0;
        nemesisSprite.color = transparentWhite;
        nemesis.transform.GetChild(nemesis.transform.childCount - 1).gameObject.SetActive(showParticles);
        nemesis.SetActive(true);
        DOTween.ToAlpha(() => nemesisSprite.color, x => nemesisSprite.color = x, 1, nemesisFadeDuration);
        DOTween.To(() => nemesisLight.intensity, x => nemesisLight.intensity = x, nemesisLightIntensity, nemesisFadeDuration);
    }
}
