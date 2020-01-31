using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DelegateCollider : MonoBehaviour
{
    public bool IsTriggering
    {
        get
        {
            return (Triggers.Count > 0);
        }
    }
    public List<Collider> Triggers = new List<Collider>();

    private void OnTriggerStay(Collider other)
    {
        if (!Triggers.Contains(other))
            Triggers.Add(other);
    }

    private void OnTriggerExit(Collider other)
    {
        if (Triggers.Contains(other))
            Triggers.Remove(other);
    }
}
