﻿using System.Collections;
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
    AK.Wwise.Event explosionEvent = null;

    public EnemyStateExplode(Enemy newEnemy, int waitBefore, float distanceFollow, float speed, float force, int damageToPlayer, int damageToEnemies, GameObject newExplosionPrefab, EnemyExplosionArea newExplosionArea, AK.Wwise.Event newExplosionEvent) : base(newEnemy)
    {
        stateEnum = EEnemyState.EXPLODE;

        waitBeats = waitBefore;
        blastForce = force;
        blastDamageToPlayer = damageToPlayer;
        blastDamageToEnemies = damageToEnemies;

        distanceFollowMax = distanceFollow;
        speedFollow = speed;

        explosionPrefab = newExplosionPrefab;
        explosionArea = newExplosionArea;
        explosionEvent = newExplosionEvent;
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
                    enemy.model.SetActive(false);
                    GameObject explosion = GameObject.Instantiate(explosionPrefab, enemy.transform.position, Quaternion.identity);
                    explosion.transform.SetParent(SceneHelper.Instance.VfxFolder);
                    explosionArea.Explode(blastForce, blastDamageToPlayer, blastDamageToEnemies, enemy.Player);
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
