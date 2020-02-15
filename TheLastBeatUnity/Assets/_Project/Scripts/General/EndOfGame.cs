﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using Sirenix.OdinInspector;
using UnityEngine.UI;
using Rewired;
using System.Linq;

public class EndOfGame : MonoBehaviour
{
    [TabGroup("Sequence")] [SerializeField]
    float HUDFadeDuration = 3;
    [TabGroup("Sequence")] [SerializeField]
    float waitBeforeTpPlayer = 2;
    [TabGroup("Sequence")] [SerializeField]
    Transform playerLastPosition = null;

    [TabGroup("Sequence")] [SerializeField]
    float followTrackDuration = 2;
    [TabGroup("Sequence")] [SerializeField]
    float waitBeforeZoom = 2;
    [TabGroup("Sequence")] [SerializeField]
    float zoomIntensity = 2;

    [TabGroup("Sequence")] [SerializeField]
    float waitBeforeChangeColor = 2;
    [TabGroup("Sequence")] [SerializeField]
    float screenShakeIntensity = 1;

    [TabGroup("Sequence")] [SerializeField]
    float waitBeforeFade = 3;
    [TabGroup("Sequence")] [SerializeField]
    float fadeDuration = 2;

    [TabGroup("References")] [SerializeField]
    GameObject HUD = null;
    Image[] HUDimages = null;
    [TabGroup("References")] [SerializeField]
    CameraManager camManager = null;
    [TabGroup("References")] [SerializeField]
    GameObject specialMonolith = null;
    [TabGroup("References")] [SerializeField]
    CameraEffect zoomCamEffect = null;
    SpecialMonolithPulse[] pulses;
    GameObject particles = null;
    [TabGroup("References")] [SerializeField]
    Image endImage = null;
    [TabGroup("References")] [SerializeField]
    GameObject zoneName = null;

    [TabGroup("References")] [Header("Audio")] [SerializeField]
    AK.Wwise.Event stopMusic = null;
    [TabGroup("References")] [SerializeField]
    AK.Wwise.Event startAmbRumble = null;
    [TabGroup("References")] [SerializeField]
    AK.Wwise.Event startBossRumble = null;
    [TabGroup("References")] [SerializeField]
    AK.Wwise.Event playBossScream = null;
    [TabGroup("References")] [SerializeField]
    SoundManager soundManager = null;
    [TabGroup("References")] [SerializeField]
    Transform newListener = null;
    [TabGroup("References")] [SerializeField]
    Transform oldListener = null;

    public static Rewired.Player playerRe;
    Player player = null;
    bool readyToQuit = false;

    private void Awake()
    {
        playerRe = ReInput.players.GetPlayer(0);
    }

    private void Start()
    {
        pulses = specialMonolith.GetComponentsInChildren<SpecialMonolithPulse>();
        particles = specialMonolith.transform.GetChild(specialMonolith.transform.childCount - 1).gameObject;
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            player = other.GetComponent<Player>();
            LaunchEnd();
            Destroy(GetComponentInChildren<BoxCollider>());
        }
    }

    private void LaunchEnd()
    {
        player.LaunchEnd();
        SceneHelper.Instance.EndOfGame = true;
        DOTween.Sequence()
            .AppendCallback(() =>
            {
                HUDimages = HUD.GetComponentsInChildren<Image>();
                HUDimages = HUDimages.Where(x => !x.CompareTag("Ephemeral")).ToArray();
                camManager.LaunchTrackCamera(1.0f / followTrackDuration);
                oldListener.SetPositionAndRotation(newListener.position, newListener.rotation);
                oldListener.SetParent(newListener);
                foreach (Image image in HUDimages)
                    image.DOFade(0, HUDFadeDuration);

                foreach(Enemy enn in GameObject.FindObjectsOfType<Enemy>())
                {
                    enn.Timescale = 0;
                }
                CameraManager.Instance.CameraStateChange("OutOfCombat");
            })
            .InsertCallback(waitBeforeTpPlayer, () =>
            {
                player.TpToLastPosition(playerLastPosition.position);
                stopMusic.Post(soundManager.gameObject);
            })
            .InsertCallback(followTrackDuration + waitBeforeZoom, () =>
            {
                zoneName.SetActive(false);
                camManager.LaunchZoomedCamera();
                Transform follow = zoomCamEffect.VirtualCam.GetCinemachineComponent<Cinemachine.CinemachineFramingTransposer>().FollowTarget;
                follow.DOMove(Vector3.forward * zoomIntensity, waitBeforeChangeColor + waitBeforeFade).SetRelative(true);
                startAmbRumble.Post(gameObject);
            })
            .InsertCallback(followTrackDuration + waitBeforeZoom + waitBeforeChangeColor, () =>
            {
                zoomCamEffect.StartScreenShake(waitBeforeFade + fadeDuration, screenShakeIntensity);
                particles.SetActive(true);
                foreach (SpecialMonolithPulse pulse in pulses)
                    pulse.ChangeColor();
                startBossRumble.Post(specialMonolith);
            })
            .InsertCallback(followTrackDuration + waitBeforeZoom + waitBeforeChangeColor + waitBeforeFade, () =>
            {
                SceneHelper.Instance.StartFade(() => BlackScreen(), fadeDuration, Color.black);
                playBossScream.Post(gameObject);
            });
    }

    private void BlackScreen()
    {
        endImage.DOFade(1, 2);
        readyToQuit = true;
    }

    private void Update()
    {
        if (readyToQuit && playerRe.GetAnyButtonDown())
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }
    }
}
