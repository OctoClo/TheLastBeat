using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class SceneSetup : MonoBehaviour
{
    [RuntimeInitializeOnLoadMethod]
    public void Init()
    {
        DOTween.Init();
    }
}
