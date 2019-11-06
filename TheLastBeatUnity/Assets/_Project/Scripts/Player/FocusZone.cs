using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FocusZone : MonoBehaviour
{
    [SerializeField]
    Transform arrow;
    [SerializeField]
    float angleBetweenArrowAndTarget;
    float angleBetweenArrowAndTargetPrevious;

    Vector3 targetPoint;
    Quaternion targetRotation;
    
    [SerializeField]
    Enemy target;

    private void Start()
    {

    }

    private void Update()
    {
        if (target)
        {
            /*angleBetweenArrowAndTarget = Vector3.SignedAngle(transform.parent.forward, -target.transform.forward, Vector3.up);
            arrow.Rotate(0, 0, angleBetweenArrowAndTargetPrevious - angleBetweenArrowAndTarget);
            angleBetweenArrowAndTargetPrevious = angleBetweenArrowAndTarget;*/
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
                angleBetweenArrowAndTargetPrevious = Vector3.SignedAngle(transform.parent.forward, -target.transform.forward, Vector3.up);
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
