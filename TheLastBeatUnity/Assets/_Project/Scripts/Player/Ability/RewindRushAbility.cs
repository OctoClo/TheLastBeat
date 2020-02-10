using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using System.Linq;

[System.Serializable]
public class RewindRushParameters : AbilityParams
{
    public float Duration = 0;
    public float PulseCost = 0;
    public float Cooldown = 0;
    public float MaxTimeBeforeResetMarks = 0;
    public AK.Wwise.State RewindState = null;
    public AK.Wwise.State NormalState = null;
    public int MaxChained = 5;
    public float screenShakeDuration = 0;
    public float screenShakeIntensity = 0;
    public float rumbleIntensity = 0;
    public float rumbleDuration = 0;
    public float freezeFrameDuration = 0.1f;
    public GameObject prefabGhost;
    public Transform groundReference;
}

public class RewindRushAbility : Ability
{
    int missedInput = 0;
    Queue<Enemy> chainedEnemies = new Queue<Enemy>();
    public bool IsInCombo => chainedEnemies.Count > 0;
    float rushChainTimer = 0;
    bool attackOnRythm = false;
    RewindRushParameters parameters;
    float duration = 0;
    Sequence seq;
    bool blockReset = false;

    public RewindRushAbility(RewindRushParameters rrp, float healCorrect) : base(rrp.AttachedPlayer, healCorrect)
    {
        parameters = rrp;
        parameters.container.AbilityEarned();
    }

    public override void Launch()
    {
        if (chainedEnemies.Count > 0 && currentCooldown == 0 && player.Status.CurrentStatus == EPlayerStatus.DEFAULT && chainedEnemies.Count >= 4)
            RewindRush();
    }

    public override void Update(float deltaTime)
    {
        if (chainedEnemies.Count > 0 && currentCooldown == 0)
        {
            rushChainTimer -= Time.deltaTime;

            if (rushChainTimer < 0 && !blockReset)
                ResetCombo();
        }

        if (currentCooldown > 0)
            currentCooldown = Mathf.Max(currentCooldown - deltaTime, 0.0f);
    }

    void RewindRush()
    {
        blockReset = true;
        player.VisualPart.gameObject.SetActive(false);
        foreach(Enemy enn in GameObject.FindObjectsOfType<Enemy>())
        {
            enn.Timescale = 0;
        }

        // Init
        currentCooldown = cooldown;
        player.RushParticles.SetActive(true);
        parameters.RewindState.SetValue();
        player.Status.StartRushing();
        player.ColliderObject.layer = LayerMask.NameToLayer("Player Dashing");
        CheckRhythm();
        // Game feel
        CameraManager.Instance.SetBlend("InCombat", "FOV Rewind", (0.1f + parameters.Duration) * chainedEnemies.Count);
        CameraManager.Instance.SetBoolCamera(true, "Rewinding");

        Vector3 direction;
        Vector3 goalPosition = player.transform.position;
        if (seq != null)
            seq.Kill();
        seq = DOTween.Sequence();
        duration = SoundManager.Instance.GetTimeLeftNextBeat(true, 0.25f);
        foreach (Enemy enemy in chainedEnemies.Reverse())
        {
            if (enemy != null)
            {
                direction = new Vector3(enemy.transform.position.x, goalPosition.y, enemy.transform.position.z) - goalPosition;
                direction *= 1.3f;

                goalPosition += direction;
                seq.Append(player.transform.DOMove(goalPosition,duration).SetEase(Ease.Linear));
                seq.AppendCallback(() =>
                {
                    AkSoundEngine.PostTrigger("OnBeatRush", SoundManager.Instance.gameObject);
                    SpawnGhost(enemy);
                });
            }
        }

        seq.AppendCallback(() => End());
        seq.timeScale = 1;
        seq.Play();
    }


    void SpawnGhost(Enemy enemy)
    {
        duration = SoundManager.Instance.LastBeat.beatInterval * (chainedEnemies.Count <= 4 ? 1 : 0.5f);
        GameObject instantiated = GameObject.Instantiate(parameters.prefabGhost);
        instantiated.transform.position = parameters.groundReference.position;
        instantiated.GetComponent<Animator>().SetTrigger("rush");
        instantiated.GetComponent<Animator>().speed = 0;
        instantiated.transform.LookAt(enemy.transform.position);
        instantiated.transform.forward = new Vector3(instantiated.transform.forward.x, 0, instantiated.transform.forward.z);
        GameObject.Destroy(instantiated, 1.2f);
        SkinnedMeshRenderer meshRenderer = instantiated.transform.GetChild(1).GetComponent<SkinnedMeshRenderer>();
        DOTween.To(() => meshRenderer.material.GetFloat("_Bias"), x =>
        {
            if (meshRenderer)
            {
                foreach (Material mat in meshRenderer.materials)
                {
                    mat.SetFloat("_Bias", x);
                }
            }
        }, 1, 1);
    }

    void CheckRhythm()
    {
        attackOnRythm = SoundManager.Instance.IsInRythm(TimeManager.Instance.SampleCurrentTime(), SoundManager.TypeBeat.BEAT);
        if (attackOnRythm)
        {
            player.ModifyPulseValue(-healCorrectBeat);
            SceneHelper.Instance.Rumble(parameters.rumbleIntensity, parameters.rumbleDuration);
            CorrectBeat();
        }
        else if (player.LoseLifeOnAbilities)
        {
            player.ModifyPulseValue(parameters.PulseCost);
            WrongBeat();
        }
    }

    public void AddChainEnemy(Enemy enn)
    {
        rushChainTimer = parameters.MaxTimeBeforeResetMarks;

        if (chainedEnemies.Count >= parameters.MaxChained)
        {
            chainedEnemies.Dequeue().Informations.RemoveRewindMark();
        }

        if (chainedEnemies.Count(x => enn == x) < 3)
        {
            enn.Informations.AddRewindMark();
            chainedEnemies.Enqueue(enn);
        }

        parameters.container.UpdateDelegate(chainedEnemies.Count);
    }

    public void MissInput()
    {
        missedInput++;

        if (missedInput >= 2)
            ResetCombo();
    }

    public void ResetCombo()
    {
        while (chainedEnemies.Count > 0)
            chainedEnemies.Dequeue().Informations.RemoveRewindMark();
        missedInput = 0;
        if (parameters.container)
            parameters.container.UpdateDelegate(0);
    }

    public override void End()
    {
        Dictionary<Enemy, int> allDamages = new Dictionary<Enemy, int>();
        foreach (Enemy enn in chainedEnemies.ToList())
        {
            if (!enn)
                continue;

            if (allDamages.ContainsKey(enn))
                allDamages[enn]++;
            else
                allDamages[enn] = 1;
        }

        parameters.NormalState.SetValue();
        foreach(Enemy enn in allDamages.Keys)
        {
            enn.GetAttacked(attackOnRythm, allDamages[enn]);
        }

        foreach (CameraEffect ce in CameraManager.Instance.AllCameras)
            ce.StartScreenShake(parameters.screenShakeDuration, parameters.screenShakeIntensity);

        foreach (Enemy enn in GameObject.FindObjectsOfType<Enemy>())
        {
            enn.Timescale = 1;
        }

        player.RushParticles.SetActive(false);
        CameraManager.Instance.SetBoolCamera(false, "Rewinding");
        player.Status.StopRushing();
        player.gameObject.layer = LayerMask.NameToLayer("Default");
        ResetCombo();
        player.VisualPart.gameObject.SetActive(true);
        blockReset = false;
    }
}
