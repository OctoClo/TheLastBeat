using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[RequireComponent(typeof(Collider))]
public class EventPositionConfiner : MonoBehaviour
{
    [Header("Event to clamp to AkAudioListener")]
    public AK.Wwise.Event Event = new AK.Wwise.Event();

    [Header("Settings")]
    public float UpdateInterval = 0.05f;

    #region private variables
    private IEnumerator positionClamperRoutine;

    //private Collider trigger;
    private Transform targetTransform;

    private GameObject eventEmitter;
    private Collider[] triggerArray;
    //private Collider actualtrigger;
    #endregion

    private void Awake()
    {
        triggerArray = GetComponents<Collider>();
        foreach (Collider trigger in triggerArray)
        {
            trigger.isTrigger = true;
        }


        eventEmitter = new GameObject("Clamped Emitter");
        eventEmitter.transform.parent = transform;
        eventEmitter.layer = LayerMask.NameToLayer("AudioBox");
        Rigidbody RB = eventEmitter.AddComponent<Rigidbody>();
        RB.isKinematic = true;
        SphereCollider SPC = eventEmitter.AddComponent<SphereCollider>();
        SPC.isTrigger = true;
        eventEmitter.AddComponent<AkGameObj>();
    }

    private void OnEnable()
    {
        var listenerGameObject = FindObjectOfType<AkAudioListener>();

        if (listenerGameObject != null)
        {
            targetTransform = listenerGameObject.transform;
        }
        else
        {
            Debug.LogError(this + ": No GameObject with 'AkAudioListener' Component found! Aborting.");
            enabled = false;
        }

        Event.Post(eventEmitter);

        positionClamperRoutine = ClampEmitterPosition();
        StartCoroutine(positionClamperRoutine);
    }

    private void OnDisable()
    {
        Event.Stop(eventEmitter);

        if (positionClamperRoutine != null)
        {
            StopCoroutine(positionClamperRoutine);
        }
    }

    IEnumerator ClampEmitterPosition()
    {
        while (true)
        {
            float minDistance = float.PositiveInfinity;
            Vector3 closestPoint = Vector3.zero;
            foreach (Collider trigger in triggerArray)
            {
                Vector3 triggerClosest = trigger.ClosestPoint(targetTransform.position);
                float distance = Vector3.Distance(triggerClosest, targetTransform.position);
                if (distance < minDistance)
                {
                    minDistance = distance;
                    closestPoint = triggerClosest;
                }
            }
            eventEmitter.transform.position = closestPoint;
            yield return new WaitForSecondsRealtime(UpdateInterval);
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.blue;
        if (eventEmitter != null)
        {
            Gizmos.DrawSphere(eventEmitter.transform.position, 0.2f);
        }
    }

    public void setEvent(AK.Wwise.Event Ev)
    {
        Event = Ev;
        Event.Post(eventEmitter);
    }

}