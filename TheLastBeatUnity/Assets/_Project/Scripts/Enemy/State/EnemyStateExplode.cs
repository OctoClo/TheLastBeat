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

    GameObject explosionCircle = null;
    Material explosionCircleMat = null;
    Sequence blinkingSeq = null;
    Color startingColor = Color.white;
    Color endColor = Color.white;

    bool waitNextBeat = true;

    float distanceFollowMax = 0;
    float speedFollow = 0;

    GameObject explosionPrefab = null;
    EnemyExplosionArea explosionArea = null;
    bool explosionBegun = false;
    AK.Wwise.Event explosionEvent = null;

    public EnemyStateExplode(Enemy newEnemy, int waitBefore, float distanceFollow, float speed, float force, int damageToPlayer,int damageToEnemies,
                            GameObject newExplosionPrefab, EnemyExplosionArea newExplosionArea, AK.Wwise.Event newExplosionEvent, GameObject circle) : base(newEnemy)
    {
        stateEnum = EEnemyState.EXPLODE;

        waitBeats = waitBefore;
        blastForce = force;
        blastDamageToPlayer = damageToPlayer;
        blastDamageToEnemies = damageToEnemies;

        explosionCircle = circle;
        explosionCircleMat = explosionCircle.GetComponent<MeshRenderer>().material;
        startingColor = explosionCircleMat.color;
        endColor.a = startingColor.a;

        distanceFollowMax = distanceFollow;
        speedFollow = speed;

        explosionPrefab = newExplosionPrefab;
        explosionArea = newExplosionArea;
        explosionEvent = newExplosionEvent;
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
            explosionEvent.Post(enemy.gameObject);
            explosionCircle.SetActive(true);
        }
        else
        {
            beatCounter++;

            if (beatCounter == waitBeats)
            {
                explosionBegun = true;
                Sequence animation = DOTween.Sequence();

                animation.AppendCallback(() =>
                {
                    blinkingSeq.Kill();
                    enemy.Model.SetActive(false);
                    GameObject explosion = GameObject.Instantiate(explosionPrefab, enemy.transform.position, Quaternion.identity);
                    explosion.transform.SetParent(SceneHelper.Instance.VfxFolder);
                    explosionArea.Explode(blastForce, blastDamageToPlayer, blastDamageToEnemies, enemy.Player);
                });

                animation.Play();
            }
            else if (beatCounter == waitBeats - 1)
            {
                LaunchCircleBlink();
            }
        }
    }

    void LaunchCircleBlink()
    {
        blinkingSeq = DOTween.Sequence().AppendCallback(() => explosionCircleMat.color = Color.white).AppendInterval(0.08f)
                                        .AppendCallback(() => explosionCircleMat.color = startingColor).AppendInterval(0.08f)
                                        .SetUpdate(true).SetLoops(-1);
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
