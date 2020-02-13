using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioCulling : MonoBehaviour
{
    SphereCollider cullingSphere = null;

    AK.Wwise.Event eventToCull = null;
    [SerializeField]
    AK.Wwise.RTPC occlusionRTPC = null;
    [SerializeField]
    GameObject audioListener = null;

    List<GameObject> occludedObjects = new List<GameObject>();

    private void Start()
    {
      cullingSphere = gameObject.GetComponent<SphereCollider>();
    }
    private void OnTriggerEnter(Collider other)
    {
        if (other.name == "Clamped Emitter")
        {
            eventToCull = other.GetComponentInParent<EventPositionConfiner>().Event;
            eventToCull.Post(other.gameObject);
            occludedObjects.Add(other.gameObject);
        }
        else if(other.tag == "Enemy")
        {
            eventToCull = other.GetComponent<Enemy>().moveSound;
            eventToCull.Post(other.gameObject);
            //occludedObjects.Add(other.gameObject);
        }
        else if (other.tag == "AmbSound")
        {
            eventToCull = other.GetComponent<AmbEvent>().ambEvent;           
            eventToCull.Post(other.gameObject);
            occludedObjects.Add(other.gameObject);
        }
    }
    private void OnTriggerExit(Collider other)
    {
        if (other.name == "Clamped Emitter")
        {
            eventToCull = other.GetComponentInParent<EventPositionConfiner>().Event;
            eventToCull.Stop(other.gameObject, 2);
            occludedObjects.Remove(other.gameObject);
        }
        else if (other.tag == "Enemy")
        {
            eventToCull = other.GetComponent<Enemy>().moveSound;
            eventToCull.Stop(other.gameObject,1);
            //occludedObjects.Remove(other.gameObject);
        }
        else if (other.tag == "AmbSound")
        {
            eventToCull = other.GetComponent<AmbEvent>().ambEvent;
            eventToCull.Stop(other.gameObject, 1);
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

                Physics.Raycast(occludedObject.transform.position, direction, out hitInfo, cullingSphere.radius, LayerMask.GetMask("AudioCulling","ClearCamera"));

                if (hitInfo.collider != null && hitInfo.collider.tag == "Wall")
                {
                    AkSoundEngine.SetRTPCValue(occlusionRTPC.Id, 1, occludedObject.gameObject);
                    Debug.DrawRay(occludedObject.transform.position, direction, Color.red);
                }
                else 
                {
                    AkSoundEngine.SetRTPCValue(occlusionRTPC.Id, 0, occludedObject.gameObject);
                    Debug.DrawRay(occludedObject.transform.position, direction, Color.green);
                }
            }
        }
    }
}
