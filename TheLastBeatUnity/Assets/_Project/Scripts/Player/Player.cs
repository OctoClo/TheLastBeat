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
    public override bool BlockInput => (blockInput || Status.Dashing || Status.Stunned);

    [TabGroup("Rush")] [SerializeField] [ValidateInput("CheckPositive", "This value must be > 0")]
    float rushDuration = 0.5f;
    [TabGroup("Rush")] [SerializeField] [ValidateInput("CheckPositive", "This value must be > 0")]
    float rushRewindDuration = 0.5f;
    [TabGroup("Rush")] [SerializeField] [ValidateInput("CheckPositive", "This value must be > 0")]
    float rushZoomDuration = 0.5f;
    [TabGroup("Rush")] [SerializeField] [ValidateInput("CheckPositive", "This value must be > 0")]
    float rushZoomValue = 5;
    [TabGroup("Rush")] [SerializeField] [ValidateInput("CheckPositive", "This value must be > 0")]
    float rushSlowMoDuration = 0.5f;
    [TabGroup("Rush")] [SerializeField] [Tooltip("The longer it is, the longer it take to change frequency")]
    float rushImpactBeatDelay = 0;
    [TabGroup("Rush")] [SerializeField] [ValidateInput("CheckPositive", "This value must be > 0")]
    float rushChainMaxInterval = 2;
    [TabGroup("Rush")] [SerializeField]
    float pulsationCostRush = 0;
    [TabGroup("Rush")] [SerializeField]
    float pulsationCostRewind =0;
    [TabGroup("Rush")] [SerializeField]
    AK.Wwise.Event soundRushOffBeat = null;
    [TabGroup("Rush")] [SerializeField]
    AK.Wwise.Event soundRushOnBeat = null;
    [TabGroup("Rush")] [SerializeField]
    AK.Wwise.State rewindNormalState = null;
    [TabGroup("Rush")] [SerializeField]
    AK.Wwise.State rewindState = null;
    float rushChainTimer = 0;
    List<Enemy> chainedEnemies = new List<Enemy>();

    [TabGroup("Blink")] [SerializeField] [ValidateInput("CheckPositive", "This value must be > 0")]
    float blinkSpeed = 5;
    [TabGroup("Blink")] [SerializeField]
    float pulsationCostBlink = 0;
    [TabGroup("Blink")] [SerializeField]
    AK.Wwise.Event soundBlink = null;
    [TabGroup("Blink")] [SerializeField] [Required]
    ParticleSystem blinkParticles = null;

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
        Status = GetComponent<PlayerStatus>();
        Anim = GetComponent<PlayerAnim>();
        FocusZone = GetComponentInChildren<FocusZone>();
        FocusZone.playerStatus = Status;
        ColliderObject = GetComponentInChildren<CapsuleCollider>().gameObject;

        abilities = new Dictionary<EInputAction, Ability>();

        Ability blink = new BlinkAbility(this, blinkSpeed, blinkParticles,soundBlink,pulsationCostBlink);
        abilities.Add(EInputAction.BLINK, blink);

        Ability rush = new RushAbility(this, rushDuration, rushZoomDuration, rushZoomValue, rushSlowMoDuration, rushImpactBeatDelay, pulsationCostRush, soundRushOnBeat , soundRushOffBeat);
        abilities.Add(EInputAction.RUSH, rush);

        Ability rewindRush = new RewindRushAbility(this, rushRewindDuration, pulsationCostRewind, rewindNormalState , rewindState);
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
                    if (abilities.TryGetValue(action, out ability))
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
        rushChainTimer = rushChainMaxInterval;
    }

    public void ResetChainedEnemies()
    {
        chainedEnemies.Clear();
    }
}
