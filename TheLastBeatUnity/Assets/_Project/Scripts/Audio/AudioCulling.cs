using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioCulling : MonoBehaviour
{
    EventPositionConfiner ambEmitter = null;
    Enemy enemyEmitter = null;

    AK.Wwise.Event eventToCull = null;

    private void OnTriggerEnter(Collider other)
    {
        if (other.name == "Clamped Emitter")
        {
            ambEmitter = other.GetComponentInParent<EventPositionConfiner>();
            eventToCull = ambEmitter.Event;
            eventToCull.Post(other.gameObject);
            Debug.Log("PostEvent");
        }
        else if(other.tag == "Enemy")
        {
            enemyEmitter = other.GetComponent<Enemy>();
            eventToCull = enemyEmitter.moveSound;
            eventToCull.Post(other.gameObject);
            Debug.Log("PostEvent");
        }
    }
    private void OnTriggerExit(Collider other)
    {
        if (other.name == "Clamped Emitter")
        {
            ambEmitter = other.GetComponentInParent<EventPositionConfiner>();
            eventToCull = ambEmitter.Event;
            eventToCull.Stop(other.gameObject, 2);
            Debug.Log("StopEvent");
        }
        else if (other.tag == "Enemy")
        {
            enemyEmitter = other.GetComponent<Enemy>();
            eventToCull = enemyEmitter.moveSound;
            eventToCull.Stop(other.gameObject,1);
            Debug.Log("StopEvent");
        }
    }
}
