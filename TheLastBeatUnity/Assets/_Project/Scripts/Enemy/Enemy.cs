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
    int lives = 10;
    protected int minLives = 0;

    [TabGroup("Behaviour")] [Header("Wander")] [SerializeField] [Tooltip("How much time the enemy will wait before going to another spot (random in [x, y]")]
    protected Vector2 waitBeforeNextMove = new Vector2(2, 5);
    [TabGroup("Behaviour")] [Header("Prepare Attack")] [SerializeField] [Tooltip("How much time the enemy will wait between chasing and prepare attack animation")]
    protected float waitBeforePrepareAnim = 0.5f;
    [TabGroup("Behaviour")] [SerializeField] [Tooltip("How long the prepare attack animation will be")]
    protected float prepareAnimDuration = 2;
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
    [TabGroup("Behaviour")] [Header("Stun")] [SerializeField] [Range(0.0f, 1.0f)]
    float[] chancesToGetStunned = new float[5];
    int stunCounter = 0;

    [TabGroup("References")] [SerializeField]
    TextMeshProUGUI lifeText = null;
    [TabGroup("References")] [SerializeField]
    TextMeshProUGUI stateText = null;
    [TabGroup("References")] [SerializeField]
    GameObject stunElements = null;
    [TabGroup("References")] [SerializeField]
    GameObject notStunElements = null;
    [TabGroup("References")]
    public GameObject model = null;
    MeshRenderer modelMeshRenderer = null;
    [TabGroup("References")] [SerializeField]
    AK.Wwise.Event hitEnemy = null;

    [TabGroup("Feedback")] [SerializeField]
    float screenDurationHit = 0;
    [TabGroup("Feedback")] [SerializeField]
    float screenIntensityHit = 0;

    public Material Material { get; private set; }
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
    Collider collid = null;
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
        modelMeshRenderer = model.GetComponent<MeshRenderer>();
        Material = modelMeshRenderer.material;
        WeaponHitbox = GetComponentInChildren<EnemyWeaponHitbox>();
        AttackHitbox = GetComponentInChildren<EnemyAttackHitbox>();
        collid = GetComponent<Collider>();
        Agent = GetComponent<NavMeshAgent>();
        Agent.speed = speed;

        lives = maxLives;
        lifeText.text = lives.ToString();
        baseAngularSpeed = Agent.angularSpeed;
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
        states.Add(EEnemyState.CHASE, new EnemyStateChase(this));
        states.Add(EEnemyState.PREPARE_ATTACK, new EnemyStatePrepareAttack(this, waitBeforePrepareAnim, prepareAnimDuration));
        states.Add(EEnemyState.ATTACK, new EnemyStateAttack(this, waitBeforeAttackAnim, attackAnimDuration, attackAnimDistance));
        states.Add(EEnemyState.RECOVER_ATTACK, new EnemyStateRecoverAttack(this, recoverAnimDuration));
        states.Add(EEnemyState.COME_BACK, new EnemyStateComeBack(this));
        states.Add(EEnemyState.STUN, new EnemyStateStun(this));
    }

    private void Update()
    {
        if (type == EEnemyType.DEFAULT)
        {
            EEnemyState newStateEnum = currentState.UpdateState(Time.deltaTime);
            ChangeState(newStateEnum);

            if (AttackHitbox.PlayerInHitbox && isAttacking && !HasAttackedPlayer)
            {
                Player.ModifyPulseValue(attackDamage, true);
                HasAttackedPlayer = true;
            }
        }
    }

    public void GetPushedBack()
    {
        ChangeState(EEnemyState.STUN);
    }

    public void GetAttacked(bool onRythm, float dmg = 1)
    {
        if (CurrentStateEnum == EEnemyState.EXPLODE)
            return;

        foreach (CameraEffect ce in CameraManager.Instance.AllCameras)
            ce.StartScreenShake(screenDurationHit, screenIntensityHit);
        
        lives -= (int)dmg;
        hitEnemy.Post(gameObject);

        if (lives > minLives)
        {
            lifeText.text = lives.ToString();
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
    }

    public virtual void StartDying()
    {
        Die();
    }

    public void Die()
    {
        EventManager.Instance.Raise(new EnemyDeadEvent { enemy = this });
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

    IEnumerator BlinkBlue()
    {
        Material.color = Color.blue;
        yield return new WaitForSecondsRealtime(0.3f);
    }

    public void SetStateText(string text)
    {
        stateText.text = text;
    }

    public void StartAttacking()
    {
        collid.isTrigger = true;
        isAttacking = true;
        HasAttackedPlayer = false;
    }

    public void StopAttacking()
    {
        collid.isTrigger = false;
        isAttacking = false;
    }

    public void BeginStun()
    {
        stunElements.SetActive(true);
        notStunElements.SetActive(false);
    }

    public void EndStun()
    {
        stunElements.SetActive(false);
        notStunElements.SetActive(true);
    }

    public void StartFocus(GameObject focusMark)
    {
        focusMark.SetActive(true);
        focusMark.transform.position = transform.position + Vector3.up;
        focusMark.transform.parent = transform;
        model.GetComponent<MeshRenderer>().materials[1].SetFloat("_Outline", 0.1f);
    }

    public void StopFocus(GameObject focusMark)
    {
        focusMark.transform.parent = null;
        focusMark.SetActive(false);
        model.GetComponent<MeshRenderer>().materials[1].SetFloat("_Outline", 0);
    }

    private void OnDestroy()
    {
        if (EnemyKilled != null)
            EnemyKilled();

        if (states != null)
            states.Clear();
    }
}
