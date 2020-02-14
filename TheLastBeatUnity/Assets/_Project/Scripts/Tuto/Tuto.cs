using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using UnityEngine.UI;
using UnityEngine.Video;

public class Tuto : Inputable
{
    [Header("General")]
    [SerializeField]
    Image backgroundImage = null;

    [SerializeField]
    Image aContinue = null;

    [SerializeField]
    Color backgroundColor = Color.black;

    [Header("First step")]
    [SerializeField]
    Image textImage = null;

    [SerializeField]
    Image iconRewind = null;

    [Header("Second step")]
    [SerializeField]
    Image imageObtentionRewind = null;

    [Header("Third step")]
    [SerializeField]
    Image videoFrame = null;

    [SerializeField]
    VideoClip secondClip = null;

    [SerializeField]
    RawImage rawImage = null;
    VideoPlayer videoPlayer;

    [Header("Fourth step")]
    [SerializeField]
    Sprite newSpriteVideo = null;

    [Header("Sound")] [SerializeField]
    AK.Wwise.State setPause = null;

    [Header("Sound")] [SerializeField]
    AK.Wwise.State unsetPause = null;

    [Header("Sound")] [SerializeField]
    AK.Wwise.Event enterUI = null;

    Sequence blockSeq;
    int progression = 0;
    public int Progression
    {
        get
        {
            return progression;
        }
        set
        {
            progression = value;
            InterpretNewProgression(progression);
        }
    }

    private void Start()
    {
        videoPlayer = GetComponentInChildren<VideoPlayer>();
        textImage.rectTransform.localScale = Vector3.zero;
    }

    public override void OnInputEnter()
    {
        setPause.SetValue();
        Progression++;
    }

    public override void OnUnblockedInput()
    {
        aContinue.DOColor(Color.white, 0.3f).SetUpdate(true);
    }

    public override void OnBlockedInput()
    {
        aContinue.DOColor(new Color(1,1,1,0), 0.3f).SetUpdate(true);
    }

    void InterpretNewProgression(int newValue)
    {
        switch (newValue)
        {
            case 1:
                TextAppear();
                break;

            case 2:
                ShowVideo();
                TemporaryBlock(2.0f);
                break;

            case 3:
                videoFrame.sprite = newSpriteVideo;
                videoPlayer.Stop();
                videoPlayer.clip = secondClip;
                StartCoroutine(PlayVideo());
                TemporaryBlock(2.0f);
                break;

            case 4:
                End();
                OnBlockedInput();
                break;
        }
    }

    void TextAppear()
    {
        SceneHelper.Instance.TryKillFreeze();
        Time.timeScale = 0;
        IndependantSequence()

            //Strange text
            .AppendCallback(() => SetBlockInput(true))
            .Append(backgroundImage.DOColor(backgroundColor, 1))
            .Append(textImage.rectTransform.DOScale(Vector3.one, 1))
            .Insert(1, textImage.DOColor(Color.white, 1))
            .AppendInterval(2.0f)

            //Rewind obtained
            .Append(textImage.DOColor(new Color(1, 1, 1, 0), 1))
            .Insert(4 , imageObtentionRewind.DOColor(Color.white, 1.0f))
            .Insert(4 , iconRewind.DOColor(Color.white , 1).OnComplete(() => SceneHelper.Instance.MainPlayer.AddRewindRush()))
            .AppendCallback(() => SetBlockInput(false));
            
    }

    Sequence IndependantSequence()
    {
        return DOTween.Sequence().SetUpdate(true);
    }

    void ShowVideo()
    {
        IndependantSequence()
            .Insert(0, iconRewind.DOColor(new Color(1, 1, 1, 0), 1.0f))
            .AppendCallback(() => StartCoroutine(PlayVideo()))
            .Append(imageObtentionRewind.DOColor(new Color(1, 1, 1, 0), 1.0f))
            .Append(videoFrame.DOColor(Color.white, 1.0f))
            .Insert(2.0f, rawImage.DOColor(Color.white, 1.0f));
    }

    void End()
    {
        unsetPause.SetValue();
        IndependantSequence()
            .Append(videoFrame.DOColor(new Color(1, 1, 1, 0), 1.0f))
            .Insert(0, rawImage.DOColor(new Color(1, 1, 1, 0), 1.0f))
            .Insert(0, backgroundImage.DOColor(Color.clear, 1.0f))
            .AppendCallback(() => Time.timeScale = 1)
            .AppendCallback(() => videoPlayer.Stop())
            .AppendCallback(() => InputDelegate.Instance.ResetInput());
    }

    void TemporaryBlock(float time)
    {
        if (blockSeq != null)
            blockSeq.Kill();

        blockSeq = IndependantSequence()
            .AppendCallback(() => SetBlockInput(true))
            .AppendInterval(time)
            .AppendCallback(() => SetBlockInput(false));
    }

    public override void ProcessInput(Rewired.Player player)
    {
        if (player.GetButtonDown("Ok"))
        {
            enterUI.Post(gameObject);
            Progression++;
        }
    }

    IEnumerator PlayVideo()
    {
        videoPlayer.Prepare();
        WaitForSeconds waitForSeconds = new WaitForSeconds(0.01f);
        while (!videoPlayer.isPrepared)
        {
            yield return waitForSeconds;
            break;
        }
        rawImage.texture = videoPlayer.texture;
        videoPlayer.Play();
    }
}
