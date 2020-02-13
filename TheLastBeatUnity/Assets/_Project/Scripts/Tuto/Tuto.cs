using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using UnityEngine.UI;

public class Tuto : Inputable
{
    [Header("General")]
    [SerializeField]
    Image backgroundImage = null;

    [SerializeField]
    Color backgroundColor = Color.black;

    [Header("First step")]
    [SerializeField]
    Image textImage = null;

    [Header("Second step")]
    [SerializeField]
    Image imageObtentionRewind = null;

    [Header("Third step")]
    [SerializeField]
    Image videoFrame = null;

    [Header("Fourth step")]
    [SerializeField]
    Sprite newSpriteVideo = null;

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
        textImage.rectTransform.localScale = Vector3.zero;
    }

    public override void OnInputEnter()
    {
        Progression++;
    }

    void InterpretNewProgression(int newValue)
    {
        if (newValue == 1)
        {
            TextAppear();
        }

        switch (newValue)
        {
            case 1:
                TextAppear();
                break;

            case 2:
                ShowVideo();
                break;

            case 3:
                videoFrame.sprite = newSpriteVideo;
                break;

            case 4:
                End();
                break;
        }
    }

    void TextAppear()
    {
        Time.timeScale = 0;
        IndependantSequence()
            .AppendCallback(() => blockInput = true)
            .Append(backgroundImage.DOColor(backgroundColor, 1))
            .Append(textImage.rectTransform.DOScale(Vector3.one, 1))
            .Insert(1, textImage.DOColor(Color.white, 1))
            .AppendInterval(2.0f)
            .Append(textImage.DOColor(new Color(1, 1, 1, 0), 1))
            .Append(imageObtentionRewind.DOColor(Color.white, 1.0f))
            .Insert(5, imageObtentionRewind.rectTransform.DOMove(Vector3.up * 10, 1).SetRelative(true))
            .AppendCallback(() => blockInput = false);
            
    }

    Sequence IndependantSequence()
    {
        return DOTween.Sequence().SetUpdate(true);
    }

    void ShowVideo()
    {
        IndependantSequence()
            .Append(imageObtentionRewind.DOColor(new Color(1, 1, 1, 0), 1.0f))
            .Append(videoFrame.DOColor(Color.white, 1.0f))
            .SetUpdate(true);
    }

    void End()
    {
        IndependantSequence()
            .Append(videoFrame.DOColor(new Color(1, 1, 1, 0), 1.0f))
            .Insert(0 , backgroundImage.DOColor(Color.clear, 1.0f))
            .AppendCallback(() => Time.timeScale = 1);
    }

    void TemporaryBlock(float time)
    {
        if (blockSeq != null)
            blockSeq.Kill();

        blockSeq = DOTween.Sequence()
            .AppendCallback(() => blockInput = true)
            .AppendInterval(time)
            .AppendCallback(() => blockInput = false);
    }

    public override void ProcessInput(Rewired.Player player)
    {
        if (player.GetButtonDown("Ok"))
        {
            Progression++;
        }
    }
}
