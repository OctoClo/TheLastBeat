using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using UnityEngine.UI;

public class ZoneNameAppear : MonoBehaviour
{
    [SerializeField]
    GameObject zoneName = null;

    [SerializeField]
    float animDuration = 1;

    public void Appear()
    {
        Image[] images = zoneName.GetComponentsInChildren<Image>();

        Sequence seq = DOTween.Sequence().Pause();
        foreach (Image image in images)
            seq.Insert(0, image.DOFade(1, animDuration));
        seq.Play();
    }
}
