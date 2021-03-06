﻿using System.Collections;
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

    public Transform VfxFolder = null;

    [SerializeField]
    AnimationCurve defaultAnimationCurve = null;

    Sequence seq;
    public static Vector3 LastDeathPosition = Vector3.zero;
    public static int DeathCount = 0;

    List<EnemyZone> zonesChasingPlayer = new List<EnemyZone>();

    Dictionary<Transform, Vector3> screenShakeMemory = new Dictionary<Transform, Vector3>();
    public delegate void boolParams(bool value);
    public event boolParams OnCombatStatusChange;
    public Player MainPlayer { get; private set; }

    [SerializeField]
    public Color ColorSlow = Color.gray;

    [SerializeField]
    public float JitRatio = 0.2f;
    int numberZone = 0;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            MainPlayer = GameObject.FindObjectOfType<Player>();
        }
    }

    public void AddZone()
    {
        if (numberZone == 0)
            OnCombatStatusChange?.Invoke(true);

        numberZone++;
    }

    public void RemoveZone()
    {
        numberZone--;

        if (numberZone == 0)
            OnCombatStatusChange?.Invoke(false);
    }

    public void AddZoneChasing(EnemyZone zone)
    {
        bool wasInCombat = (zonesChasingPlayer.Count > 0);

        if (!zonesChasingPlayer.Contains(zone))
            zonesChasingPlayer.Add(zone);
    }

    public void RemoveZoneChasing(EnemyZone zone)
    {
        bool wasInCombat = (zonesChasingPlayer.Count > 0);

        if (zonesChasingPlayer.Contains(zone))
            zonesChasingPlayer.Remove(zone);
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

    public void StartFade(UnityAction lambda, float duration , Color color, bool independant = false)
    {
        seq = DOTween.Sequence();
        seq.Append(img.DOColor(color, duration));
        seq.AppendCallback(() => lambda());
        seq.SetUpdate(independant);
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
        DOTween.Init(null, null, LogBehaviour.Verbose);
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

    public float ComputeTimeScale(Sequence sequence , float mustFinishIn)
    {
        float timeLeft = sequence.Duration(false) - sequence.Elapsed(false);
        return timeLeft / mustFinishIn;
    }

    public float ComputeTimeScale(Animator animator, float mustFinishIn)
    {
        AnimatorStateInfo state = animator.GetCurrentAnimatorStateInfo(0);
        float elapsedTime = (state.normalizedTime % 1.0f) * state.length;
        float timeLeft = state.length - elapsedTime;
        return timeLeft / mustFinishIn;
    }

    public void ScreenshakeGameObject(Transform trsf , float duration , float intensity , AnimationCurve curve = null)
    {
        StopAllCoroutines();
        StartCoroutine(ScreenShakeCoroutine(trsf, duration, intensity, curve == null ? defaultAnimationCurve : curve));
    }

    IEnumerator ScreenShakeCoroutine(Transform trsf, float duration, float intensity, AnimationCurve curve)
    {
        if (screenShakeMemory.ContainsKey(trsf))
            trsf.position = screenShakeMemory[trsf];

        screenShakeMemory[trsf] = trsf.position;
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
        screenShakeMemory.Remove(trsf);
    }

    public IEnumerator FreezeFrameCoroutine(float duration)
    {
        Time.timeScale = 0;
        yield return new WaitForSecondsRealtime(duration);
        Time.timeScale = 1;
    }

    public void FreezeFrameTween(float duration)
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
