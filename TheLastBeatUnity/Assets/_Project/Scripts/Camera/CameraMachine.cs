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

    [SerializeField]
    GameObject gob;

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

    [SerializeField] [FolderPath(RequireExistingPath = true, ParentFolder = "Assets")]
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
        StartTransition(firstState, firstState.Profile, 0.1f);
    }

    void Update()
    {
        if (currentState != null)
        {
            currentState.StateUpdate();
        }
    }

    //During the transition the camera has no state, careful
    public void StartTransition(CameraState newState, CameraProfile cp, float timeTransition, bool resetOffset = false)
    {
        CameraEffect camEffect = GetComponent<CameraEffect>();
        CameraPosition camPosition = GetComponent<CameraPosition>();
        Cinemachine.CinemachineFramingTransposer transposer = virtualCam.GetCinemachineComponent<CinemachineFramingTransposer>();

        float fov = virtualCam.m_Lens.FieldOfView;
        float angle = camPosition.Angle;
        float distance = transposer.m_CameraDistance;

        //Run parallel sequence
        //Set angle
        Sequence seq = DOTween.Sequence();
        seq.AppendCallback(() =>
        {
            SetState(null);
        });
        seq.AppendCallback(() =>
        {
            camPosition.Interpolate(cp.Angle, timeTransition, cp.curve);
        });
        seq.AppendInterval(timeTransition);
        seq.AppendCallback(() => {
            SetState(newState);
        });

        //Set FOV
        Sequence seq2 = DOTween.Sequence();
        seq2.AppendCallback(() => camEffect.StartZoom(cp.FOV - virtualCam.m_Lens.FieldOfView, timeTransition, CameraEffect.ZoomType.FOV, CameraEffect.ValueType.Absolute));
     
        //Set distance
        Sequence seq3 = DOTween.Sequence();
        seq3.Append(DOTween.To(() => transposer.m_CameraDistance, x => transposer.m_CameraDistance = x, cp.DistanceToViewer, timeTransition));

        if (resetOffset)
        {
            CinemachineCameraOffset offset = GetComponent<CinemachineCameraOffset>();
            Sequence seq4 = DOTween.Sequence();
            seq4.Append(DOTween.To(() => offset.m_Offset, x => offset.m_Offset = x, Vector3.zero, timeTransition));
        }

        seq.Play();
        seq2.Play();
        seq3.Play();
    }

    //Negative / zero = ignore transition
    public void SetState(CameraState state)
    {
        if (currentState != null)
        {
            currentState.StateExit();
            currentState.SetMachine(null);
        }
            
        currentState = state;

        if (currentState)
        {
            currentState.SetMachine(this);
            currentState.StateEnter();
        }
        
    }


}
