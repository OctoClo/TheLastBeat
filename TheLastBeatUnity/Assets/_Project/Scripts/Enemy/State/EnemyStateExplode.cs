using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class EnemyStateExplode : EnemyState
{
    int waitBeats = 0;
    int beatCounter = 0;
    float blastForce = 0;
    int blastDamageToPlayer = 0;
    int blastDamageToEnemies = 0;

    bool waitNextBeat = true;

    float distanceFollowMax = 0;
    float speedFollow = 0;

    GameObject explosionPrefab = null;
    EnemyExplosionArea explosionArea = null;
    bool explosionBegun = false;
    AK.Wwise.Event explosionRiseEvent = null;
    AK.Wwise.Event explosionBurstEvent = null;

    GameObject explosionCircle = null;
    EnemyPulse[] pulses;

    public EnemyStateExplode(Enemy newEnemy, int waitBefore, float distanceFollow, float speed, float force, int damageToPlayer,int damageToEnemies,
                            GameObject newExplosionPrefab, EnemyExplosionArea newExplosionArea, AK.Wwise.Event newExplosionRiseEvent, AK.Wwise.Event newExplosionBurstEvent, GameObject circle) : base(newEnemy)
    {
        stateEnum = EEnemyState.EXPLODE;

        waitBeats = waitBefore;
        blastForce = force;
        blastDamageToPlayer = damageToPlayer;
        blastDamageToEnemies = damageToEnemies;

        explosionCircle = circle;
        pulses = enemy.GetComponentsInChildren<EnemyPulse>();

        distanceFollowMax = distanceFollow;
        speedFollow = speed;

        explosionPrefab = newExplosionPrefab;
        explosionArea = newExplosionArea;
        explosionRiseEvent = newExplosionRiseEvent;
        explosionBurstEvent = newExplosionBurstEvent;
    }

    public override void Enter()
    {
        base.Enter();
        enemy.Agent.enabled = true;
        enemy.Animator.SetTrigger("prepareExplosion");
    }

    public override void OnBeat()
    {
        if (waitNextBeat)
        {
            waitNextBeat = false;
            beatCounter = 0;
            enemy.Agent.speed = speedFollow;
            enemy.Agent.acceleration = speedFollow;
            enemy.Agent.autoBraking = true;
            explosionRiseEvent.Post(enemy.gameObject);
            explosionCircle.SetActive(true);
            foreach (EnemyPulse pulse in pulses)
                pulse.PulseFasterAndFaster(waitBeats * SoundManager.Instance.TimePerBeat);
        }
        else
        {
            beatCounter++;
            if (enemy)
                AkSoundEngine.SetRTPCValue("ExplosionRise", beatCounter + 1, enemy.gameObject, (int)SoundManager.Instance.TimePerBeat);

            if (beatCounter == waitBeats)
            {
                explosionBegun = true;
                Sequence animation = DOTween.Sequence();

                animation.AppendCallback(() =>
                {
                    enemy.Model.SetActive(false);
                    explosionCircle.SetActive(false);
                    GameObject explosion = GameObject.Instantiate(explosionPrefab, enemy.transform.position, Quaternion.identity);
                    explosion.transform.SetParent(SceneHelper.Instance.VfxFolder);
                    explosionArea.Explode(blastForce, blastDamageToPlayer, blastDamageToEnemies, enemy.Player);
                    explosionBurstEvent.Post(enemy.gameObject);
                });

                animation.Play();
            }
        }
    }

    public override EEnemyState UpdateState(float deltaTime)
    {
        if (explosionArea.ExplosionFinished)
        {
            enemy.Die();
        }
        else if (!explosionBegun)
        {
            if (Vector3.SqrMagnitude(enemy.transform.position - enemy.Player.transform.position) < distanceFollowMax)
            {
                // Player is too close, only look at him
                enemy.Agent.ResetPath();
                enemy.LookAtPlayer(deltaTime);
            }
            else
            {
                // Follow player
                enemy.Agent.SetDestination(enemy.Player.transform.position);
            }
        }

        return stateEnum;
    }
}
