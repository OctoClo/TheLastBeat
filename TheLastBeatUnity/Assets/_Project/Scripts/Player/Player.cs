using System.Collections;
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
    public override bool BlockInput => (blockInput || Status.Dashing || Status.Stunned || Status.Blinking);

    [TabGroup("Blink")] [SerializeField]
    BlinkParams blinkParameters = null;

    [TabGroup("Rush")][SerializeField]
    RushParams rushParameters = null;

    [TabGroup("Rush")][SerializeField]
    RewindRushParameters rushRewindParameters = null;

    [HideInInspector]
    public PlayerStatus Status { get; private set; }
    [HideInInspector]
    public PlayerAnim Anim = null;
    [HideInInspector]
    public GameObject ColliderObject = null;

    [SerializeField]
    Health healthSystem = null;
    public Health Health => healthSystem;

    [SerializeField]
    Transform model = null;
    public Transform Model => model;

    Dictionary<EInputAction, Ability> abilities = new Dictionary<EInputAction, Ability>();
    IReadOnlyDictionary<EInputAction, Ability> Abilities => abilities;

    [HideInInspector]
    public FocusZone FocusZone = null;

    [SerializeField]
    BeatManager beatManager = null;
    public BeatManager BeatManager => beatManager;

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

        BlinkAbility blink = new BlinkAbility(blinkParameters);
        abilities.Add(EInputAction.BLINK, blink);

        RushAbility rush = new RushAbility(rushParameters);
        abilities.Add(EInputAction.RUSH, rush);

        RewindRushAbility rewindRush = new RewindRushAbility(rushRewindParameters);
        abilities.Add(EInputAction.REWINDRUSH, rewindRush);

        rush.RewindRush = rewindRush;
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
                if (player.GetButtonDown(action.ToString()) && abilities.TryGetValue(action, out ability))
                {
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
            abilityPair.Value.Update(Time.deltaTime);
    }

    public Enemy GetCurrentTarget()
    {
        return FocusZone.GetCurrentTarget();
    }
}
