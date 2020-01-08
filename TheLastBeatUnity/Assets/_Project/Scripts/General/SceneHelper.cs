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

    [SerializeField]
    AnimationCurve defaultAnimationCurve = null;

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

    public List<Vector3> RayCastBackAndForth(Collider coll, Vector3 origin, Vector3 direction, float maxDistance)
    {
        direction.Normalize();
        List<Vector3> output = new List<Vector3>();
        origin += Vector3.up * 0.3f;
        Ray ray = new Ray(origin - (direction * 2) + (Vector3.up * 0.01f), direction * 10);
        Ray reverted = new Ray(RotatePointAroundPivot(origin - (direction * 2), coll.transform.position, Vector3.up * 180), -direction * 10);

        foreach(RaycastHit hit in Physics.RaycastAll(ray , maxDistance * 2))
        {
            if (hit.collider == coll)
            {
                output.Add(hit.point);
                break;
            }
        }

        foreach (RaycastHit hit in Physics.RaycastAll(reverted, maxDistance * 2))
        {
            if (hit.collider == coll)
            {
                output.Add(hit.point);
                break;
            }
        }
        
        return output;
    }

    public void ScreenshakeGameObject(Transform trsf , float duration , float intensity , AnimationCurve curve = null)
    {
        StopAllCoroutines();
        StartCoroutine(ScreenShakeCoroutine(trsf, duration, intensity, curve == null ? defaultAnimationCurve : curve));
    }

    IEnumerator ScreenShakeCoroutine(Transform trsf, float duration, float intensity, AnimationCurve curve)
    {
        Vector3 origin = trsf.position;
        float normalizedTime = 0;
        while (normalizedTime < 1)
        {
            Vector2 random = Random.insideUnitCircle.normalized;
            float currentIntensity = curve.Evaluate(normalizedTime) * intensity;
            trsf.position = origin + new Vector3(random.x * currentIntensity, random.y * currentIntensity, 0);
            normalizedTime += Time.deltaTime / duration;
            yield return null;
        }
        trsf.position = origin;
    }

    public void FreezeFrame(float duration)
    {
        Sequence seq = DOTween.Sequence();
        seq.AppendCallback(() => Time.timeScale = 0);
        seq.AppendInterval(duration);
        seq.AppendCallback(() => Time.timeScale = 1);
        seq.SetUpdate(true);
        seq.Play();
    }

    public void Rumble(float intensity , float duration)
    {
        InputDelegate.player.SetVibration(0, intensity, duration);
        InputDelegate.player.SetVibration(1, intensity, duration);
    }
}
