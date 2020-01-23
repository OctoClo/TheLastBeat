using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class EnemyStateExplode : EnemyState
{
    float waitBeforeExplosion = 0;
    float blastForce = 0;
    int blastDamageToPlayer = 0;
    int blastDamageToEnemies = 0;

    GameObject explosionPrefab = null;
    EnemyExplosionArea explosionArea = null;

    public EnemyStateExplode(Enemy newEnemy, float waitBefore, float force, int damageToPlayer, int damageToEnemies, GameObject newExplosionPrefab, EnemyExplosionArea newExplosionArea) : base(newEnemy)
    {
        stateEnum = EEnemyState.EXPLODE;

        waitBeforeExplosion = waitBefore;
        blastForce = force;
        blastDamageToPlayer = damageToPlayer;
        blastDamageToEnemies = damageToEnemies;

        explosionPrefab = newExplosionPrefab;
        explosionArea = newExplosionArea;
    }

    public override void Enter()
    {
        enemy.SetStateText("explode");

        Sequence animation = DOTween.Sequence();

        animation.InsertCallback(waitBeforeExplosion, () =>
        {
            enemy.model.SetActive(false);
            GameObject explosion = GameObject.Instantiate(explosionPrefab, enemy.transform.position, Quaternion.identity);
            explosion.transform.SetParent(SceneHelper.Instance.VfxFolder);
            explosionArea.Explode(blastForce, blastDamageToPlayer, blastDamageToEnemies, enemy.Player);
        });

        animation.Play();
    }

    public override EEnemyState UpdateState(float deltaTime)
    {
        if (explosionArea.ExplosionFinished)
        {
            enemy.Die();
        }

        return stateEnum;
    }
}
