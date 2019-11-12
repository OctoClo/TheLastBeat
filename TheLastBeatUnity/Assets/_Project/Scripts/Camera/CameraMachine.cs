using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;
using Sirenix.OdinInspector;
using UnityEditor;
using DG.Tweening;

public class CameraMachine : MonoBehaviour
{
    CameraState currentState;

    [SerializeField]
    CameraState firstState;

    [SerializeField]
    float fov;

    [SerializeField]
    float angle;

    [SerializeField]
    float distance;

    [Button]
    public void Test()
    {
        CameraEffect camEffect = GetComponent<CameraEffect>();
        CameraPosition camPosition = GetComponent<CameraPosition>();

        if (camEffect && camPosition)
        {
            camPosition.Angle = angle;
            camEffect.SetZoomFOV(fov);
            virtualCam.GetCinemachineComponent<CinemachineFramingTransposer>().m_CameraDistance = distance;
        }
    }

    [SerializeField]
    [FolderPath(RequireExistingPath = true, ParentFolder = "Assets")]
    string outputAsset;

    [SerializeField]
    string outputFile;

    [Button]
    public void GenerateAsset()
    {
        CameraProfile cp = ScriptableObject.CreateInstance<CameraProfile>();
        cp.FOV = fov;
        cp.Angle = angle;
        cp.DistanceToViewer = distance;

        AssetDatabase.CreateAsset(cp, "Assets/" + outputAsset + "/" + outputFile + ".asset");
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
    }


    public CinemachineVirtualCamera virtualCam => GetComponent<CinemachineVirtualCamera>();

    private void Start()
    {
        SetState(firstState, 2);
    }

    void Update()
    {
        if (currentState != null)
        {
            currentState.StateUpdate();
        }
    }

    //During the transition the camera has no state, careful
    public void StartTransition(CameraState newState, float timeTransition)
    {
        CameraEffect camEffect = GetComponent<CameraEffect>();
        CameraPosition camPosition = GetComponent<CameraPosition>();

        Sequence seq = DOTween.Sequence();
        seq.AppendCallback(() => SetState(null, -1));
        seq.AppendCallback(() => camPosition.Interpolate(newState.Profile.Angle, timeTransition));
        seq.AppendCallback(() => SetState(newState, -1));

        Sequence seq2 = DOTween.Sequence();
        seq2.AppendCallback(() => camEffect.StartZoom(newState.Profile.FOV - virtualCam.m_Lens.FieldOfView, timeTransition, CameraEffect.ZoomType.FOV, CameraEffect.ValueType.Absolute));

        Cinemachine.CinemachineFramingTransposer transposer = GetComponent<Cinemachine.CinemachineFramingTransposer>();
        Sequence seq3 = DOTween.Sequence();
        seq3.Append(DOTween.To(() => transposer.m_CameraDistance, x => transposer.m_CameraDistance = x, newState.Profile.DistanceToViewer, timeTransition));

        seq.Play();
        seq2.Play();
        seq3.Play();
    }

    //Negative / zero = ignore transition
    public void SetState(CameraState state, float durationTransition = -1)
    {
        if (durationTransition > 0 && state.Profile != null)
        {
            StartTransition(state, durationTransition);
        }

        if (currentState != null)
        {
            currentState.StateExit();
            currentState.SetMachine(null);
        }
            
        currentState = state;
        currentState.SetMachine(this);
        currentState.StateEnter();
    }


}
