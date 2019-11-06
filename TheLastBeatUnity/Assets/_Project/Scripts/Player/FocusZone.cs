using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FocusZone : MonoBehaviour
{
    [SerializeField]
    Transform arrow;

    Vector3 targetPoint;
    Quaternion targetRotation;
    
    [SerializeField]
    Enemy target;

    private void Start()
    {

    }

    public Enemy GetCurrentTarget()
    {
        return target;
    }

    private void Update()
    {
        if (target)
        {
            targetPoint = new Vector3(target.transform.position.x, transform.position.y, target.transform.position.z) - transform.position;
            targetRotation = Quaternion.LookRotation(targetPoint, Vector3.up);
            transform.rotation = targetRotation;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        Enemy enemy = other.GetComponent<Enemy>();
        if (enemy)
        {
            if (!target)
            {
                target = enemy;
                target.SetSelected(true);
                arrow.gameObject.SetActive(true);
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (target)
        {
            if (GameObject.ReferenceEquals(other.gameObject, target.gameObject))
            {
                arrow.gameObject.SetActive(false);
                target.SetSelected(false);
                target = null;
            }
        }
    }
}
