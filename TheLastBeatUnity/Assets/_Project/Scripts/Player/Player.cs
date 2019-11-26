﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Rewired;
using Sirenix.OdinInspector;
using DG.Tweening;
using System;

public class Player : Inputable
{
    [TabGroup("Movement")] [SerializeField] [ValidateInput("CheckPositive", "This value must be > 0")]
    float speed = 7.5f;
    [TabGroup("Movement")] [SerializeField] [ValidateInput("CheckPositive", "This value must be > 0")]
    float maxRotationPerFrame = 30;
    public Vector3 CurrentDirection { get; set; }

    //If you are doing something (dash , attack animation, etc...) or if game paused, temporary block input
    public override bool BlockInput => (blockInput || Status.Dashing || Status.Stunned);

    [TabGroup("Blink")] [SerializeField]
    BlinkParams blinkParameters;

    [TabGroup("Rush")][SerializeField]
    RushParams rushParameters;

    [TabGroup("Rush")][SerializeField]
    RewindRushParameters rushRewindParameters;

    float rushChainTimer = 0;
    List<Enemy> chainedEnemies = new List<Enemy>();

    [HideInInspector]
    public PlayerStatus Status;
    [HideInInspector]
    public PlayerAnim Anim;
    [HideInInspector]
    public GameObject ColliderObject;

    [SerializeField]
    Health healthSystem;
    public Health Health => healthSystem;

    Dictionary<EInputAction, Ability> abilities;
    [HideInInspector]
    public FocusZone FocusZone = null;
    Enemy currentTarget = null;

    bool CheckPositive(float value) { return value > 0; }

    private void Start()
    {
        blinkParameters.AttachedPlayer = rushParameters.AttachedPlayer = rushRewindParameters.AttachedPlayer = this;
        Status = GetComponent<PlayerStatus>();
        Anim = GetComponent<PlayerAnim>();
        FocusZone = GetComponentInChildren<FocusZone>();
        FocusZone.playerStatus = Status;
        ColliderObject = GetComponentInChildren<CapsuleCollider>().gameObject;

        abilities = new Dictionary<EInputAction, Ability>();

        Ability blink = new BlinkAbility(blinkParameters);
        abilities.Add(EInputAction.BLINK, blink);

        Ability rush = new RushAbility(rushParameters);
        abilities.Add(EInputAction.RUSH, rush);

        Ability rewindRush = new RewindRushAbility(rushRewindParameters);
        abilities.Add(EInputAction.REWINDRUSH, rewindRush);
    }

    public override void ProcessInput(Rewired.Player player)
    {
        Vector3 direction = new Vector3(player.GetAxis("MoveX"), 0, player.GetAxis("MoveY"));
        CurrentDirection = direction;
        Anim.SetMovement(direction);

        // Look at
        currentTarget = FocusZone.GetCurrentTarget();
        Vector3 lookVector;
        if (currentTarget)
        {
            lookVector = new Vector3(currentTarget.transform.position.x, transform.position.y, currentTarget.transform.position.z) - transform.position;
            transform.rotation = Quaternion.RotateTowards(transform.rotation, Quaternion.LookRotation(lookVector), maxRotationPerFrame);
        }
        else if (direction != Vector3.zero)
        {
            lookVector = direction;
            lookVector.Normalize();
            transform.rotation = Quaternion.RotateTowards(transform.rotation, Quaternion.LookRotation(lookVector), maxRotationPerFrame);
        }

        // Movement and abilities
        if (!Status.Dashing)
        {
            Ability ability = null;

            foreach (EInputAction action in (EInputAction[])Enum.GetValues(typeof(EInputAction)))
            {
                if (player.GetButtonDown(action.ToString()))
                {
                    abilities.TryGetValue(action, out ability);

                    if (ability != null)
                        ability.Launch();
                }
            }

            if (ability == null)
            {
                Vector3 movement = direction * Time.deltaTime * speed;
                transform.Translate(movement, Space.World);
            }
        }
    }

    private void Update()
    {
        foreach (KeyValuePair<EInputAction, Ability> abilityPair in abilities)
            abilityPair.Value.Update(Time.deltaTime / Time.timeScale);

        if (chainedEnemies.Count > 0 && !Status.Dashing)
        {
            rushChainTimer -= Time.deltaTime;

            if (rushChainTimer < 0)
                ResetChainedEnemies();
        }
    }

    public Enemy GetCurrentTarget()
    {
        return FocusZone.GetCurrentTarget();
    }

    public List<Enemy> GetChainedEnemies()
    {
        return chainedEnemies;
    }

    public void AddChainedEnemy(Enemy enemy)
    {
        chainedEnemies.Add(enemy);
        rushChainTimer = rushRewindParameters.chainEnnemy;
    }

    public void ResetChainedEnemies()
    {
        chainedEnemies.Clear();
    }
}
