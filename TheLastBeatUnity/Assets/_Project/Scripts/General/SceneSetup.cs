using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using UnityEditor;
using AK.Wwise;

public class SceneSetup : MonoBehaviour
{
    [RuntimeInitializeOnLoadMethod]
    public static void Init()
    {
        DOTween.Init();
    }
}
