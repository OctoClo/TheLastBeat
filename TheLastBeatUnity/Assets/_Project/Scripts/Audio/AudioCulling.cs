using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioCulling : MonoBehaviour
{
    EventPositionConfiner ambEmitter = null;
    Enemy enemyEmitter = null;

    AK.Wwise.Event eventToCull = null;
    [SerializeField]
    AK.Wwise.RTPC occlusionRTPC = null;
    [SerializeField]
    GameObject audioListener = null;

    List<GameObject> occludedObjects = new List<GameObject>();


    private void OnTriggerEnter(Collider other)
    {
        if (other.name == "Clamped Emitter")
        {
            ambEmitter = other.GetComponentInParent<EventPositionConfiner>();
            eventToCull = ambEmitter.Event;
            eventToCull.Post(other.gameObject);
            occludedObjects.Add(other.gameObject);
        }
        else if(other.tag == "Enemy")
        {
            enemyEmitter = other.GetComponent<Enemy>();
            eventToCull = enemyEmitter.moveSound;
            eventToCull.Post(other.gameObject);
            occludedObjects.Add(other.gameObject);
        }
    }
    private void OnTriggerExit(Collider other)
    {
        if (other.name == "Clamped Emitter")
        {
            ambEmitter = other.GetComponentInParent<EventPositionConfiner>();
            eventToCull = ambEmitter.Event;
            eventToCull.Stop(other.gameObject, 2);
            occludedObjects.Remove(other.gameObject);
        }
        else if (other.tag == "Enemy")
        {
            enemyEmitter = other.GetComponent<Enemy>();
            eventToCull = enemyEmitter.moveSound;
            eventToCull.Stop(other.gameObject,1);
            occludedObjects.Remove(other.gameObject);
        }
    }

    private void Update()
    {
        if (occludedObjects.Count != 0)
        {
            foreach(GameObject occludedObject in occludedObjects)
            {
                 Vector3 direction = audioListener.transform.position - occludedObject.transform.position;
                 RaycastHit hitInfo;

                 Physics.Raycast(occludedObject.transform.position, direction, out hitInfo);

                if (hitInfo.collider != null && hitInfo.collider.tag != "Wall")
                {
                    AkSoundEngine.SetRTPCValue(occlusionRTPC.Id, 0, occludedObject.gameObject);
                    Debug.DrawRay(occludedObject.transform.position, direction, Color.green);
                    Debug.Log(hitInfo.collider);
                }
                else if (hitInfo.collider != null && hitInfo.collider.tag == "Wall")
                {
                    AkSoundEngine.SetRTPCValue(occlusionRTPC.Id, 1, occludedObject.gameObject);
                    Debug.DrawRay(occludedObject.transform.position, direction, Color.red);
                    Debug.Log(hitInfo.collider);
                }
            }
        }
    }
}
