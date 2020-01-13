using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DelegateCollider : MonoBehaviour
{
    public delegate void DelegateColl(Collider coll);
    public event DelegateColl OnTriggerEnterDelegate;
    public event DelegateColl OnTriggerExitDelegate;

    public delegate void DelegateColli(Collision coll);
    public event DelegateColli OnCollisionEnterDelegate;
    public event DelegateColli OnCollisionExitDelegate;

    private void OnTriggerEnter(Collider other)
    {
        OnTriggerEnterDelegate?.Invoke(other);
    }

    private void OnTriggerExit(Collider other)
    {
        OnTriggerExitDelegate?.Invoke(other);
    }

    private void OnCollisionEnter(Collision collision)
    {
        OnCollisionEnterDelegate?.Invoke(collision);
    }

    private void OnCollisionExit(Collision collision)
    {
        OnCollisionExitDelegate?.Invoke(collision);
    }
}
