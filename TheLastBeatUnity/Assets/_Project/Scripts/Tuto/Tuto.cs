using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using UnityEngine.UI;

public class Tuto : Inputable
{
    [SerializeField]
    Image firstImage = null;

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

    public override void OnInputEnter()
    {
        Progression++;
    }

    void InterpretNewProgression(int newValue)
    {
        Debug.Log(newValue);
        if (newValue == 2)
        {
            DOTween.Sequence()
                .AppendCallback(() =>
                {
                    firstImage.color = Color.clear;
                    firstImage.rectTransform.localPosition = Vector3.down * 10.0f;
                })
                .Append(firstImage.rectTransform.DOMove(Vector3.up * 3, 1.0f).SetRelative(true))
                .Insert(0 , firstImage.DOColor(Color.white, 1.0f));
        }
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
            TemporaryBlock(2.0f);
        }
    }
}
