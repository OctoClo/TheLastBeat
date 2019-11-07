﻿using System.Collections;
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
            Enemy previousTarget = currentTarget;

            currentTarget = potentialTargets[(potentialTargets.IndexOf(currentTarget) + 1) % potentialTargets.Count];
            
            // Update target selected state
            if (currentTarget != previousTarget)
            {
                previousTarget.SetSelected(false, this);
                currentTarget.SetSelected(true, this);
            }
        }
    }

    private void SelectPreviousTarget()
    {
        if (potentialTargets.Count > 1)
        {
            Enemy previousTarget = currentTarget;

            // Process who is the previous target
            int previousIndex = potentialTargets.IndexOf(currentTarget) - 1;
            if (previousIndex < 0)
                previousIndex = potentialTargets.Count - 1;
            currentTarget = potentialTargets[previousIndex];

            // Update target selected state
            if (currentTarget != previousTarget)
            {
                previousTarget.SetSelected(false, this);
                currentTarget.SetSelected(true, this);
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        Enemy enemy = other.GetComponent<Enemy>();
        if (enemy && !potentialTargets.Contains(enemy))
        {
            // Add new enemy to potential targets and sort the list based on x position
            potentialTargets.Add(enemy);
            potentialTargets.Sort((enemy1, enemy2) => enemy1.transform.position.x.CompareTo(enemy2.transform.position.x));

            // If no current target, this enemy becomes the target
            if (!currentTarget)
            {
                currentTarget = enemy;
                currentTarget.SetSelected(true, this);
                arrow.gameObject.SetActive(true);
                
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        Enemy enemy = other.GetComponent<Enemy>();
        if (enemy && potentialTargets.Contains(enemy))
        {
            // Remove this enemy from potential targets
            potentialTargets.Remove(enemy);

            // If this enemy was the current target, unselect it
            if (currentTarget && GameObject.ReferenceEquals(other.gameObject, currentTarget.gameObject))
            {
                currentTarget.SetSelected(false, this);
                TrySelectAnotherTarget();
            }
        }
    }

    void TrySelectAnotherTarget()
    {
        currentTarget = null;

        // If there are other potential targets, select the next one
        if (potentialTargets.Count > 0)
        {
            currentTarget = potentialTargets[0];
            currentTarget.SetSelected(true, this);
        }
        else
            arrow.gameObject.SetActive(false);
    }

    public void TargetDestroyed()
    {
        potentialTargets.Remove(currentTarget);
        TrySelectAnotherTarget();
    }
}
