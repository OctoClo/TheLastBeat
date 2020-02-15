using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Linq;

public class Pyramid : MonoBehaviour
{
    [SerializeField]
    float length = 0;
    public float Length => length;

    [SerializeField]
    float angle = 0;

    [SerializeField]
    Transform arrowVisual = null;

    List<Collider> potentialCollisions = new List<Collider>();
    List<Collider> insideCone = new List<Collider>();

    [SerializeField]
    GameObject prefabFocus = null;
    GameObject instantiatedFocus;

    public SphereCollider SphereCollider { get; private set; }

    Vector3 position = Vector3.zero;
    Vector3 direction = Vector3.up * 10;
    Vector3 center => position + (direction.normalized * length);
    Vector3 left => position + (Quaternion.AngleAxis(angle / 2.0f, Vector3.up) * direction) * length;
    Vector3 right => position + (Quaternion.AngleAxis(-angle / 2.0f, Vector3.up) * direction) * length;

    Collider nearest = null;
    public bool RightStickEnabled { get; set; }
    public bool LeftStickEnabled { get; set; }

    public Collider NearestEnemy
    {
        get
        {
            return nearest;
        }
        set
        {
            if (nearest != null)
            {
                Enemy enn = nearest.GetComponent<Enemy>();
                enn.EnemyKilled -= Nullifie;
                enn.StopFocus(instantiatedFocus);
            }

            nearest = value;

            if (nearest != null)
            {
                Enemy enn = nearest.GetComponent<Enemy>();
                enn.EnemyKilled += Nullifie;
                enn.StartFocus(instantiatedFocus);
            }
        }
    }

    void Nullifie()
    {
        NearestEnemy = null;
    }

    private void Start()
    {
        SphereCollider = gameObject.AddComponent<SphereCollider>();
        SphereCollider.isTrigger = true;
        instantiatedFocus = Instantiate(prefabFocus);
        RecomputePositions();
        RightStickEnabled = false;
    }

    private void OnTriggerEnter(Collider other)
    {
        Enemy enemy = other.GetComponent<Enemy>();
        if (enemy && !enemy.IsDying && !enemy.IsExploding)
        {
            potentialCollisions.Add(other);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (potentialCollisions.Contains(other))
        {
            potentialCollisions.Remove(other);
            
        }

        if (insideCone.Contains(other))
            insideCone.Remove(other);

        if (NearestEnemy == other)
        {
            NearestEnemy = null;
        }
    }

    private void Update()
    {
        RecomputePositions();
        CheckOverlap();
        arrowVisual.gameObject.SetActive(RightStickEnabled || LeftStickEnabled);
        insideCone = insideCone.Distinct().ToList();
        if (!RightStickEnabled && !LeftStickEnabled && NearestEnemy)
        {
            NearestEnemy = null;
        }

        if (NearestEnemy)
        {
            Enemy enemy = NearestEnemy.GetComponent<Enemy>();
            if (enemy.IsDying || enemy.IsExploding)
                NearestEnemy = null;
        }

        SetArrowScale(RightStickEnabled ? 1 : 0.5f);
        if (NearestEnemy)
        {
            transform.LookAt(NearestEnemy.transform.position);
            if (!RightStickEnabled)
                arrowVisual.transform.forward = transform.forward;

            SetArrowScale(Vector3.Distance(transform.position, NearestEnemy.transform.position) / length);
        }
        else
        {
            transform.forward = transform.parent.forward;
            transform.forward = new Vector3(transform.forward.x, 0, transform.forward.z);
        }
    }

    void CheckOverlap()
    {
        insideCone = insideCone.Distinct().ToList();
        foreach (Collider coll in potentialCollisions)
        {
            if (coll)
            {
                if (IsInsideCone(coll.transform.position, direction))
                {
                    insideCone.Add(coll);
                }

                if (!IsInsideCone(coll.transform.position, direction))
                {
                    insideCone.Remove(coll);
                }
            }
        }
        
        if (NearestEnemy == null)
            RecomputeNearest();
    }

    //Tempory set the pyramid direction , if an anemy is found keep this direction, otherwise go back
    public void OverlookDirection(Vector3 direct)
    {
        arrowVisual.gameObject.SetActive(true);

        //TODO : Get the nearest
        List<Collider> allCandidates = new List<Collider>();
        Enemy enemy = null;
        foreach (Collider coll in potentialCollisions)
        {
            
            if (coll && IsInsideCone(coll.transform.position, direct, true))
            {
                enemy = coll.GetComponent<Enemy>();
                if (!enemy.IsDying && !enemy.IsExploding)
                    allCandidates.Add(coll);
            }
        }
        
        arrowVisual.transform.forward = direct;
        if (allCandidates.Contains(NearestEnemy))
            arrowVisual.transform.LookAt(NearestEnemy.transform);

        if (allCandidates.Count > 0)
        {
            NearestEnemy = allCandidates.OrderBy(x => Vector3.Distance(transform.position, x.transform.position)).First();
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

    bool IsInsideCone(Vector3 position, Vector3 dir, bool debug = false)
    {
        position = new Vector3(position.x, transform.position.y, position.z);
        Vector3 centerToPos = position - this.position;
        if (Vector3.Dot(centerToPos, dir) > 0 && Vector3.Angle(centerToPos, dir) < angle / 2.0f)
        {
            return true;
        }
        return false;
    }

    public void RecomputePositions()
    {
        direction = transform.forward;
        position = transform.position;

        float dist = Vector3.Distance(right, center) * 2;
        SphereCollider.radius = length;
    }

    public void RecomputeNearest()
    {
        IEnumerable<Collider> result = insideCone.Where(x => x != null).OrderBy(x => Vector3.Distance(position, x.transform.position));
        if (result.Count() == 0 || potentialCollisions.Count == 0)
            NearestEnemy = null;
        else
            NearestEnemy = result.First();
    }

    void SetArrowScale(float ratio)
    {
        Transform child = arrowVisual.GetChild(0);
        Vector3 scale = child.localScale;
        Vector3 localPos = child.localPosition;

        child.localScale = new Vector3(child.localScale.x, child.localScale.y, 0.5f * ratio);
        child.localPosition = new Vector3(0, child.localPosition.y, 2.3f * ratio);
    }
}
