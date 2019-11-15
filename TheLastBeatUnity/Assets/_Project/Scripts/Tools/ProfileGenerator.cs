using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

#if UNITY_EDITOR
using UnityEditor;
#endif

using Sirenix.OdinInspector;
using Cinemachine;

public class ProfileGenerator : MonoBehaviour
{
    [SerializeField]
    [TabGroup("Profile")]
    float fov = 0;

    [SerializeField]
    [TabGroup("Profile")]
    float angle = 0;

    [SerializeField]
    [TabGroup("Profile")]
    AnimationCurve curveTransition = null;

    [SerializeField][TabGroup("Profile")]
    float distance = 0;

    [SerializeField]
    [FolderPath(RequireExistingPath = true, ParentFolder = "Assets")]
    [TabGroup("Profile")]
    string outputAsset = "";

    [TabGroup("Profile")]
    string outputFile = "";

    [SerializeField]
    CinemachineVirtualCamera virtualCam;

    [SerializeField]
    CameraEffect camEffect;

    [SerializeField]
    CameraPosition camPosition;

#if UNITY_EDITOR
    [Button][TabGroup("Profile")]
    public void GenerateAsset()
    {
        CameraProfile cp = ScriptableObject.CreateInstance<CameraProfile>();
        cp.FOV = fov;
        cp.Angle = angle;
        cp.DistanceToViewer = distance;
        cp.transitionIn = curveTransition;

        AssetDatabase.CreateAsset(cp, "Assets/" + outputAsset + "/" + outputFile + ".asset");
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
    }
#endif

    [Button]
    [TabGroup("Profile")]
    public void Test()
    {
        if (camEffect && camPosition)
        {
            camPosition.Angle = angle;
            camEffect.SetZoomFOV(fov);
            virtualCam.GetCinemachineComponent<CinemachineFramingTransposer>().m_CameraDistance = distance;
        }
    }
}
