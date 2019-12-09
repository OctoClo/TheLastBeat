﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class EnemyStatePrepareAttack : EnemyState
{
    Vector3 scaleEndValues = Vector3.zero;
    bool animationFinished = false;

    public EnemyStatePrepareAttack(Enemy newEnemy) : base(newEnemy)
    {
        stateEnum = EEnemyState.PREPARE_ATTACK;
        scaleEndValues = new Vector3(1.5f, 1.5f, 1.5f);
    }

    public override void Enter()
    {
        enemy.SetStateText("prepare");

        animationFinished = false;
        enemy.CurrentMove = DOTween.Sequence();

        enemy.CurrentMove.AppendInterval(0.5f);
        enemy.CurrentMove.Append(enemy.transform.DOScale(scaleEndValues, 2));
        enemy.CurrentMove.AppendCallback(() =>
        {
            animationFinished = true;
            enemy.CurrentMove = null;
        });

        enemy.CurrentMove.Play();
    }

    public override EEnemyState UpdateState(float deltaTime)
    {
        if (animationFinished)
            return EEnemyState.ATTACK;
        
        return stateEnum;
    }
}
