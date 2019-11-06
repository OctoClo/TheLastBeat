using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Rewired;

public class FocusZone : MonoBehaviour
{
    [SerializeField]
    Transform arrow;

    Enemy currentTarget;
    Vector3 targetLookVector;
    Quaternion targetRotation;

    [SerializeField]
    List<Enemy> potentialTargets;

    private void Start()
    {
        potentialTargets = new List<Enemy>();
    }

    public Enemy GetCurrentTarget()
    {
        return currentTarget;
    }

    private void Update()
    {
        if (currentTarget)
        {
            targetLookVector = new Vector3(currentTarget.transform.position.x, transform.position.y, currentTarget.transform.position.z) - transform.position;
            targetRotation = Quaternion.LookRotation(targetLookVector, Vector3.up);
            transform.rotation = targetRotation;

            if (ReInput.players.GetPlayer(0).GetButtonDown("NextTarget"))
                SelectNextTarget();
            else if (ReInput.players.GetPlayer(0).GetButtonDown("PreviousTarget"))
                SelectPreviousTarget();
        }
    }

    private void SelectNextTarget()
    {
        if (potentialTargets.Count > 1)
        {
            currentTarget = potentialTargets[(potentialTargets.IndexOf(currentTarget) + 1) % potentialTargets.Count];
        }
    }

    private void SelectPreviousTarget()
    {
        if (potentialTargets.Count > 1)
        {
            int previousIndex = potentialTargets.IndexOf(currentTarget) - 1;
            if (previousIndex < 0)
                previousIndex = potentialTargets.Count - 1;
            currentTarget = potentialTargets[previousIndex];
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        Enemy enemy = other.GetComponent<Enemy>();
        if (enemy && !potentialTargets.Contains(enemy))
        {
            potentialTargets.Add(enemy);
            potentialTargets.Sort((enemy1, enemy2) => enemy1.transform.position.x.CompareTo(enemy2.transform.position.x));

            if (!currentTarget)
            {
                currentTarget = enemy;
                currentTarget.SetSelected(true);
                arrow.gameObject.SetActive(true);
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        Enemy enemy = other.GetComponent<Enemy>();
        if (enemy && potentialTargets.Contains(enemy))
        {
            potentialTargets.Remove(enemy);

            if (currentTarget && GameObject.ReferenceEquals(other.gameObject, currentTarget.gameObject))
            {
                arrow.gameObject.SetActive(false);
                currentTarget.SetSelected(false);
                currentTarget = null;
            }
        }
    }
}
