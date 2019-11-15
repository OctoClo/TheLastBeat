using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;
using Sirenix.OdinInspector;
using UnityEditor;
using DG.Tweening;
using System.Linq;

public class CameraMachine : MonoBehaviour
{
    CameraState currentState;

    [SerializeField][TabGroup("Machine")]
    CameraState firstState = null;

    public static CameraMachine GetLiveCamera()
    {
        CinemachineVirtualCamera cam = GameObject.FindObjectOfType<CinemachineBrain>().ActiveVirtualCamera as CinemachineVirtualCamera;
        return cam.GetComponent<CameraMachine>();
    }

    struct Sequences
    {
        public Sequence sequenceAngle;
        public Sequence sequenceFOV;
        public Sequence sequenceDistance;
        public Sequence sequenceOffset;
    }
    Sequences runningSequences;

    public CinemachineVirtualCamera virtualCam => GetComponent<CinemachineVirtualCamera>();

    IEnumerator TestCoroutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(Random.Range(1.5f, 2.5f));
            if (Random.value < 0.5f)
            {
                CameraState cs = GetComponent<CameraOutOfCombat>();
                StartTransition(cs, cs.Profile, 2);
            }
            else
            {
                CameraState cs = GetComponent<CameraInCombat>();
                StartTransition(cs, cs.Profile, 2);
            }
        }
    }

    private void Start()
    {
        StartTransition(firstState, firstState.Profile, 0.1f);
    }

    public void StartZoom(float newValue , CameraEffect.ZoomType zt, float duration)
    {
        if (zt == CameraEffect.ZoomType.FOV)
        {
            DOTween.To(() => virtualCam.m_Lens.FieldOfView, x => virtualCam.m_Lens.FieldOfView = x, newValue, duration);
        }
        else
        {
            CinemachineFramingTransposer transposer = virtualCam.GetCinemachineComponent<CinemachineFramingTransposer>();
            DOTween.To(() => transposer.m_CameraDistance , x => transposer.m_CameraDistance = x, newValue, duration);
        }
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

        //You can't have two sequence of camera transition at the same time
        if (runningSequences.sequenceAngle != null)
        {
            runningSequences.sequenceAngle.Kill();
        }

        if (runningSequences.sequenceFOV != null)
        {
            runningSequences.sequenceFOV.Kill();
        }

        if (runningSequences.sequenceDistance != null)
        {
            runningSequences.sequenceDistance.Kill();
        }

        runningSequences.sequenceAngle = DOTween.Sequence();
        runningSequences.sequenceAngle.AppendCallback(() => SetState(null));
        runningSequences.sequenceAngle.Append(DOTween.To(() => camPosition.Angle, x => camPosition.Angle = x, cp.Angle, timeTransition));
        runningSequences.sequenceAngle.AppendCallback(() => SetState(newState));

        //Set FOV
        runningSequences.sequenceFOV = DOTween.Sequence();
        runningSequences.sequenceFOV.Append(DOTween.To(() => virtualCam.m_Lens.FieldOfView, x => virtualCam.m_Lens.FieldOfView = x, cp.FOV, timeTransition));

        //Set distance
        runningSequences.sequenceDistance = DOTween.Sequence();
        runningSequences.sequenceDistance.Append(DOTween.To(() => transposer.m_CameraDistance, x => transposer.m_CameraDistance = x, cp.DistanceToViewer, timeTransition));

        if (resetOffset)
        {
            CinemachineCameraOffset offset = GetComponent<CinemachineCameraOffset>();

            if (runningSequences.sequenceOffset != null)
            {
                runningSequences.sequenceOffset.Kill();
            }

            runningSequences.sequenceOffset = DOTween.Sequence();
            runningSequences.sequenceOffset.Append(DOTween.To(() => offset.m_Offset, x => offset.m_Offset = x, Vector3.zero, timeTransition));
        }

        runningSequences.sequenceAngle.Play();
        runningSequences.sequenceFOV.Play();
        runningSequences.sequenceDistance.Play();
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

    public void EnterCombat(float time, float width = 50)
    {
        CameraInCombat ic = GetComponent<CameraInCombat>();
        ic.ConfinerWidth = width;
        if (ic)
        {
            StartTransition(ic, ic.Profile, time, true);
        }
    }

    public void EnterOOC(float time)
    {
        CameraOutOfCombat ooc = GetComponent<CameraOutOfCombat>();
        if (ooc)
        {
            StartTransition(ooc, ooc.Profile, time);
        }
    }
}
