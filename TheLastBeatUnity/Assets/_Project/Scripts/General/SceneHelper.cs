using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using UnityEngine.UI;
using UnityEngine.Events;

public class SceneHelper : MonoBehaviour
{
    public static SceneHelper Instance { get; private set; }

    [SerializeField]
    Image img;

    Sequence seq;

    private void Start()
    {
        if (Instance == null)
            Instance = this;
    }

    public void StartFade(UnityAction lambda, float duration , Color color)
    {
        seq = DOTween.Sequence();
        seq.Append(img.DOColor(color, duration));
        seq.AppendCallback(() => lambda());
        seq.Play();
    }

    private void OnDestroy()
    {
        DOTween.KillAll();
    }

    public Vector3 RotatePointAroundPivot(Vector3 point, Vector3 pivot, Vector3 angles)
    {
        return Quaternion.Euler(angles) * (point - pivot) + pivot;
    }
}
