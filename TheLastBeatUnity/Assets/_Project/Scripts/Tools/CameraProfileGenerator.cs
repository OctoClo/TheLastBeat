using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

#if UNITY_EDITOR
using UnityEditor;
#endif

using Sirenix.OdinInspector;
using Cinemachine;

public class CameraProfileGenerator : MonoBehaviour
{
    [TabGroup("Profile")] [SerializeField]
    float fov = 0;

    [TabGroup("Profile")] [SerializeField]
    float angle = 0;

    [TabGroup("Profile")] [SerializeField]
    float distance = 0;

    [TabGroup("Profile")] [SerializeField]
    AnimationCurve transitionInCurve = null;

    [TabGroup("Profile")] [SerializeField] [FolderPath(RequireExistingPath = true, ParentFolder = "Assets")]
    string outputAssetFolder = "";

    [TabGroup("Profile")] [SerializeField]
    string outputAssetName = "";

    [TabGroup("References")] [SerializeField] [Required]
    CinemachineVirtualCamera virtualCam;

    [TabGroup("References")] [SerializeField] [Required]
    CameraEffect camEffect;

    [TabGroup("References")] [SerializeField] [Required]
    CameraPosition camPosition;

#if UNITY_EDITOR
    [Button][TabGroup("Profile")]
    public void GenerateAsset()
    {
        CameraProfile cp = ScriptableObject.CreateInstance<CameraProfile>();
        cp.FOV = fov;
        cp.Angle = angle;
        cp.DistanceToViewer = distance;
        cp.transitionIn = transitionInCurve;

        AssetDatabase.CreateAsset(cp, "Assets/" + outputAssetFolder + "/" + outputAssetName + ".asset");
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
