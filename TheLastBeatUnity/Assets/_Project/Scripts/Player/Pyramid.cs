using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Linq;

public class Pyramid : MonoBehaviour
{
    [SerializeField]
    float length;
    public float Length => length;

    [SerializeField]
    float angle;

    List<Collider> potentialCollisions = new List<Collider>();
    List<Collider> insideCone = new List<Collider>();

    public BoxCollider BoxCollider { get; private set; }

    Vector3 position = Vector3.zero;
    Vector3 direction = Vector3.up * 10;
    Vector3 center => position + (direction.normalized * length);
    Vector3 left => position + (Quaternion.AngleAxis(angle / 2.0f, Vector3.up) * direction) * length;
    Vector3 right => position + (Quaternion.AngleAxis(-angle / 2.0f, Vector3.up) * direction) * length;

    public Collider NearestEnemy => insideCone.Count == 0 ? null : insideCone.OrderBy(x => Vector3.Distance(position, x.transform.position)).First();

    private void Start()
    {
        BoxCollider = gameObject.AddComponent<BoxCollider>();
        BoxCollider.isTrigger = true;
        RecomputePositions();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.GetComponent<Enemy>())
        {
            potentialCollisions.Add(other);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.GetComponent<Enemy>())
            potentialCollisions.Remove(other);
    }

    private void Update()
    {
        CheckOverlap();
    }

    void CheckOverlap()
    {
        insideCone.Clear();
        foreach (Collider coll in potentialCollisions)
        {
            if (IsInsideCone(coll.transform.position))
            {
                insideCone.Add(coll);
            }
        }
    }

    void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        direction = transform.forward;
        position = transform.position;

        Gizmos.DrawLine(transform.position, left);
        Gizmos.DrawLine(transform.position, right);
        Gizmos.DrawLine(left, right);
    }

    bool IsInsideCone(Vector3 position)
    {
        position = new Vector3(position.x, transform.position.y, position.z);
        Vector3 centerToPos = position - this.position;
        if (Vector3.Dot(centerToPos, direction) > 0 && Vector3.Angle(centerToPos, direction) < angle / 2.0f)
        {
            return true;
        }
        return false;
    }

    public void SetDirection(Vector3 newDirection)
    {
        direction = newDirection;
        RecomputePositions();
    }

    public void SetPosition(Vector3 newPosition)
    {
        position = newPosition;
        RecomputePositions();
    }

    public void DebugDraw()
    {
        direction = transform.forward;
        position = transform.position;

        Debug.DrawLine(right, left, Color.red);
        Debug.DrawLine(position, right, Color.red);
        Debug.DrawLine(position, left, Color.red);
    }

    void RecomputePositions()
    {
        direction = transform.forward;
        position = transform.position;

        BoxCollider.center = Vector3.forward * length * 0.5f;
        float dist = Vector3.Distance(right, center) * 2;
        BoxCollider.size = new Vector3(dist, 10, length);
    }
}
