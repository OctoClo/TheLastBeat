using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using UnityEditor;

public class SceneSetup : MonoBehaviour
{
    [RuntimeInitializeOnLoadMethod]
    public static void Init()
    {
        DOTween.Init();
    }

    [MenuItem("Tools/Bata/Centrer Camera")]
    public static void CenterCam()
    {
        Camera target = SceneView.lastActiveSceneView.camera;
        Transform tmp = target.transform;
        tmp.position = Camera.main.transform.position;
        tmp.rotation = Quaternion.LookRotation(Camera.main.transform.forward);
        SceneView.lastActiveSceneView.AlignViewToObject(tmp);
    }
}
