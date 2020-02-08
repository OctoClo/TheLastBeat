using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
using TMPro;
using DG.Tweening;
using UnityEngine.AI;

public class EnemyDeadEvent : GameEvent { public Enemy enemy = null; }

public class Enemy : Slowable
{
    [TabGroup("General")] [SerializeField]
    float speed = 8f;
    [TabGroup("General")] [SerializeField]
    int maxLives = 10;
    protected int lives = 10;
    protected int minLives = 0;

    [TabGroup("Behaviour")] [Header("Wander")] [SerializeField] [Tooltip("How much time the enemy will wait before going to another spot (random in [x, y]")]
    protected Vector2 waitBeforeNextMove = new Vector2(2, 5);
    [TabGroup("Behaviour")] [Header("Chase")] [SerializeField] [Tooltip("How close to the player the enemy will follow")]
    protected float chaseDistance = 2.5f;
    [TabGroup("Behaviour")] [Header("Prepare Attack")] [SerializeField] [Tooltip("How much time the enemy will wait between chasing and prepare attack animation")]
    protected float waitBeforePrepareAnim = 0.5f;
    [TabGroup("Behaviour")] [SerializeField] [Tooltip("How long the prepare attack animation will be, in beats")]
    protected int prepareAnimDurationBeats = 2;
    [TabGroup("Behaviour")] [SerializeField] [Tooltip("How much time the enemy will wait not moving after prepare anim")]
    protected float waitAfterPrepareAnim = 0.5f;
    [TabGroup("Behaviour")] [SerializeField] [Tooltip("How fast the enemy will turn towards the player")]
    protected float turnSpeed = 2.5f;
    [TabGroup("Behaviour")] [Header("Attack")] [SerializeField] [Tooltip("How much time the enemy will wait between preparing attack animation and attacking animation")]
    protected float waitBeforeAttackAnim = 0.25f;
    [TabGroup("Behaviour")] [SerializeField] [Tooltip("How long the attack animation will be")]
    protected float attackAnimDuration = 0.5f;
    [TabGroup("Behaviour")] [SerializeField] [Tooltip("How much the enemy will dive towards the player")]
    protected float attackAnimDistance = 10;
    [TabGroup("Behaviour")] [SerializeField] [Tooltip("How many HP the player will lose if hit")]
    protected int attackDamage = 5;
    [TabGroup("Behaviour")] [Header("Recover")] [SerializeField] [Tooltip("How much time the enemy will wait after an attack")]
    protected float recoverAnimDuration = 2;
    [TabGroup("Behaviour")] [Header("Stun")] [SerializeField]
    float stunDuration = 0;
    [TabGroup("Behaviour")] [SerializeField] [Range(0.0f, 1.0f)]
    float[] chancesToGetStunned = new float[5];
    int stunCounter = 0;
    [TabGroup("Behaviour")][Header("Audio")]
    public AK.Wwise.Event moveSound = null;
    [TabGroup("Behaviour")][SerializeField]
    AK.Wwise.Event dieSound = null;

    [TabGroup("References")] [SerializeField]
    GameObject stunElements = null;
    [TabGroup("References")] [SerializeField]
    AK.Wwise.Event hitEnemy = null;
    [TabGroup("References")] [SerializeField]
    UIEnemy informations = null;
    public UIEnemy Informations => informations;
    [TabGroup("References")]
    public Animator Animator = null;
    [TabGroup("References")]
    public GameObject Model = null;
    List<Material> outlineMats = new List<Material>();

    [TabGroup("Feedback")] [SerializeField]
    float screenDurationHit = 0;
    [TabGroup("Feedback")] [SerializeField]
    float screenIntensityHit = 0;

    public EnemyWeaponHitbox WeaponHitbox { get; private set; }
    public EnemyDetectionZone DetectionZone { get; private set; }
    public EnemyWanderZone WanderZone { get; private set; }
    public Player Player { get; private set; }
    EnemyAttackHitbox AttackHitbox;

    // States
    protected Dictionary<EEnemyState, EnemyState> states;
    public EEnemyState CurrentStateEnum { get; private set; }
    EnemyState currentState;
    [HideInInspector]
    public bool ComeBack = false;
    [HideInInspector]
    public bool ChaseAgain = false;
    [HideInInspector]
    public bool InWanderZone = false;
    [HideInInspector]
    public NavMeshAgent Agent = null;

    // Misc
    EEnemyType type = EEnemyType.DEFAULT;
    bool isAttacking = false;
    [HideInInspector]
    public bool HasAttackedPlayer = false;

    public delegate void noParams();
    public event noParams EnemyKilled;
    float baseAngularSpeed = 0;

    public override float Timescale
    {
        get
        {
            return base.Timescale;
        }
        set
        {
            base.Timescale = value;
            Agent.speed = speed * value;
            Agent.angularSpeed = baseAngularSpeed * value;
        }
    }

    protected virtual void Awake()
    {
        WeaponHitbox = GetComponentInChildren<EnemyWeaponHitbox>();
        AttackHitbox = GetComponentInChildren<EnemyAttackHitbox>();
        Agent = GetComponent<NavMeshAgent>();

        SkinnedMeshRenderer[] renderers = Model.GetComponentsInChildren<SkinnedMeshRenderer>();
        Material[] materials = null;
        foreach (SkinnedMeshRenderer renderer in renderers)
        {
            materials = renderer.materials;
            if (materials.Length > 1)
                outlineMats.Add(materials[1]);
        }

        Agent.speed = speed;
        lives = maxLives;
        informations.Init(maxLives);
        baseAngularSpeed = Agent.angularSpeed;
        //moveSound.Post(gameObject);
    }

    public void ZoneInitialize(EEnemyType newType, EnemyWanderZone newWanderZone, EnemyDetectionZone newDetectionZone, Player newPlayer)
    {
        type = newType;
        DetectionZone = newDetectionZone;
        WanderZone = newWanderZone;
        Player = newPlayer;

        if (type == EEnemyType.DEFAULT)
        {
            states = new Dictionary<EEnemyState, EnemyState>();
            CreateStates();

            CurrentStateEnum = EEnemyState.WANDER;
            states.TryGetValue(CurrentStateEnum, out currentState);
            currentState.Enter();
        }
    }

    virtual protected void CreateStates()
    {
        states.Add(EEnemyState.WANDER, new EnemyStateWander(this, waitBeforeNextMove));
        states.Add(EEnemyState.CHASE, new EnemyStateChase(this, chaseDistance));
        states.Add(EEnemyState.PREPARE_ATTACK, new EnemyStatePrepareAttack(this, waitBeforePrepareAnim, prepareAnimDurationBeats, waitAfterPrepareAnim, SceneHelper.Instance.JitRatio));
        states.Add(EEnemyState.ATTACK, new EnemyStateAttack(this, waitBeforeAttackAnim + waitAfterPrepareAnim, attackAnimDuration, attackAnimDistance));
        states.Add(EEnemyState.RECOVER_ATTACK, new EnemyStateRecoverAttack(this, recoverAnimDuration));
        states.Add(EEnemyState.COME_BACK, new EnemyStateComeBack(this));
        states.Add(EEnemyState.STUN, new EnemyStateStun(this, stunDuration));
    }

    private void Update()
    {
        if (type == EEnemyType.DEFAULT)
        {
            EEnemyState newStateEnum = currentState.UpdateState(Time.deltaTime);
            ChangeState(newStateEnum);
            
            Animator.SetBool("moving", (Agent.velocity != Vector3.zero || isAttacking));

            if (AttackHitbox.PlayerInHitbox && isAttacking && !HasAttackedPlayer)
            {
                Player.ModifyPulseValue(attackDamage, true);
                HasAttackedPlayer = true;
            }
        }
    }

    public void OnBeat()
    {
        if (type == EEnemyType.DEFAULT)
            currentState.OnBeat();
    }

    public void OnBar()
    {
        if (type == EEnemyType.DEFAULT)
            currentState.OnBar();
    }

    public void LookAtPlayer(float deltaTime)
    {
        Vector3 targetDirection = Player.transform.position - transform.position;
        targetDirection.y = 0;
        Vector3 newDirection = Vector3.RotateTowards(transform.forward, targetDirection, turnSpeed * deltaTime, 0.0f);
        transform.rotation = Quaternion.LookRotation(newDirection);
    }

    public void GetPushedBack()
    {
        ChangeState(EEnemyState.STUN);
    }

    public bool GetAttacked(bool onRythm, float dmg = 1)
    {
        if (CurrentStateEnum == EEnemyState.EXPLODE)
            return true;

        foreach (CameraEffect ce in CameraManager.Instance.AllCameras)
            ce.StartScreenShake(screenDurationHit, screenIntensityHit);
        
        lives -= (int)dmg;
        informations.Life = lives;
        bool dying = (lives <= minLives);
        hitEnemy.Post(gameObject);

        if (!dying)
        {
            if (stunCounter < chancesToGetStunned.Length)
            {
                float stunPercentage = RandomHelper.GetRandom();
                if (stunPercentage < chancesToGetStunned[stunCounter])
                {
                    stunCounter++;
                    ChangeState(EEnemyState.STUN);
                }
            }
        }
        else
            StartDying();

        return dying;
    }

    public virtual void StartDying()
    {
        Die();
    }

    public void Die()
    {
        EventManager.Instance.Raise(new EnemyDeadEvent { enemy = this });
        dieSound.Post(gameObject);
        Destroy(gameObject);
    }

    protected void ChangeState(EEnemyState newStateEnum)
    {
        EnemyState newState;
        if (newStateEnum != CurrentStateEnum && states.TryGetValue(newStateEnum, out newState))
        {
            currentState.Exit();
            CurrentStateEnum = newStateEnum;
            currentState = newState;
            currentState.Enter();
        }
    }

    public void StartAttacking()
    {
        isAttacking = true;
        HasAttackedPlayer = false;
    }

    public void StopAttacking()
    {
        isAttacking = false;
    }

    public void BeginStun()
    {
        stunElements.SetActive(true);
    }

    public void EndStun()
    {
        stunElements.SetActive(false);
    }

    public void StartFocus(GameObject focusMark)
    {
        //focusMark.GetComponent<LockPoint>().SetLockPoint(transform);
        informations.StartFocus();
        foreach (Material outlineMat in outlineMats)
            outlineMat.SetFloat("_Outline", 0.0005f);
    }

    public void StopFocus(GameObject focusMark)
    {
        //focusMark.GetComponent<LockPoint>().SetLockPoint(null);
        informations.StopFocus();
        foreach (Material outlineMat in outlineMats)
            outlineMat.SetFloat("_Outline", 0);
    }

    private void OnDestroy()
    {
        EnemyKilled?.Invoke();

        if (states != null)
            states.Clear();
    }
}
