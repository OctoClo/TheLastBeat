using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using UnityEngine.UI;
using UnityEngine.Events;

public class SceneHelper : MonoBehaviour
{
    public static SceneHelper Instance { get; private set; }

    [SerializeField]
    Image img = null;

    [SerializeField]
    Transform respawnPlace = null;

    Sequence seq;
    public static Vector3 LastDeathPosition = Vector3.zero;
    public static int DeathCount = 0;

    private void Start()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
    }
    
    public void RecordDeath(Vector3 position)
    {
        DeathCount++;
        LastDeathPosition = position;
    }

    public void Respawn(Transform target)
    {
        target.forward = respawnPlace.forward;
        target.position = respawnPlace.position + Vector3.up;
    }

    public void StartFade(UnityAction lambda, float duration , Color color)
    {
        seq = DOTween.Sequence();
        seq.Append(img.DOColor(color, duration));
        seq.AppendCallback(() => lambda());
        seq.Play();
    }

    private void OnDestroy()
    {
        DOTween.KillAll();
    }

    public Vector3 RotatePointAroundPivot(Vector3 point, Vector3 pivot, Vector3 angles)
    {
        return Quaternion.Euler(angles) * (point - pivot) + pivot;
    }

    [RuntimeInitializeOnLoadMethod]
    public static void Init()
    {
        DOTween.Init();
    }

    public List<Vector3> GetCollisions(Collider coll, Vector3 origin, Vector3 direction, float maxDistance)
    {
        List<Vector3> output = new List<Vector3>();
        origin += Vector3.up * 0.3f;
        Ray ray = new Ray(origin, direction);
        Ray reverted = new Ray(RotatePointAroundPivot(origin, coll.transform.position, Vector3.up * 180), -direction);
        foreach(RaycastHit hit in Physics.RaycastAll(ray , maxDistance))
        {
            if (hit.collider == coll)
            {
                output.Add(hit.point);
                Debug.DrawLine(hit.point, origin, Color.red,1);
                break;
            }
        }

        foreach (RaycastHit hit in Physics.RaycastAll(reverted, maxDistance))
        {
            if (hit.collider == coll)
            {
                output.Add(hit.point);
                Debug.DrawLine(hit.point, RotatePointAroundPivot(origin, coll.transform.position, Vector3.up * 180), Color.blue, 1);
                break;
            }
        }
        
        return output;
    }

    public void ComputeTimeScale(Sequence sequence , float mustFinishIn)
    {
        float timeLeft = sequence.Duration(false) - sequence.Elapsed(false);
        float newCoeff = (sequence.timeScale * mustFinishIn) / timeLeft;
        sequence.timeScale = 1 / newCoeff;
        StartCoroutine(VerifTest(sequence, mustFinishIn));
    }

    IEnumerator VerifTest(Sequence seq, float wait)
    {
        yield return new WaitForSeconds(wait);
        Debug.Log("End " + seq.ElapsedPercentage(false));
    }
}
