using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
using TMPro;
using DG.Tweening;

public class EnemyDeadEvent : GameEvent { public Enemy enemy = null; }

public class Enemy : MonoBehaviour
{
    [TabGroup("General")] [SerializeField]
    float speed = 5;
    public float Speed => speed;
    [TabGroup("General")] [SerializeField]
    int maxLives = 10;
    int lives = 10;

    [TabGroup("Behaviour")] [Header("Wander")] [SerializeField] [Tooltip("How much time the enemy will wait before going to another spot (random in [x, y]")]
    Vector2 waitBeforeNextMove = new Vector2(2, 5);
    [TabGroup("Behaviour")] [Header("Prepare Attack")] [SerializeField] [Tooltip("How much time the enemy will wait between chasing and prepare attack animation")]
    float waitBeforePrepareAnim = 0.5f;
    [TabGroup("Behaviour")] [SerializeField] [Tooltip("How long the prepare attack animation will be")]
    float prepareAnimDuration = 2;
    [TabGroup("Behaviour")] [Header("Attack")] [SerializeField] [Tooltip("How much time the enemy will wait between preparing attack animation and attacking animation")]
    float waitBeforeAttackAnim = 0.25f;
    [TabGroup("Behaviour")] [SerializeField] [Tooltip("How long the attack animation will be")]
    float attackAnimDuration = 0.5f;
    [TabGroup("Behaviour")] [SerializeField] [Tooltip("How much the enemy will dive towards the player")]
    float attackForce = 4;
    [TabGroup("Behaviour")] [SerializeField] [Tooltip("How many pulse intensity the player will lose if hit")]
    int attackDamage = 5;
    [TabGroup("Behaviour")] [Header("Recover")] [SerializeField] [Tooltip("How much time the enemy will wait after an attack")]
    float recoverAnimDuration = 2;
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
    public Material Material { get; private set; }
    Collider collid;
    public EnemyWeaponHitbox WeaponHitbox { get; private set; }
    public EnemyDetectionZone DetectionZone { get; private set; }
    public EnemyWanderZone WanderZone { get; private set; }
    public Player Player { get; private set; }

    // States
    Dictionary<EEnemyState, EnemyState> states;
    public EEnemyState CurrentStateEnum { get; private set; }
    EnemyState currentState;
    [HideInInspector]
    public bool ComeBack = false;
    [HideInInspector]
    public bool ChaseAgain = false;
    [HideInInspector]
    public Sequence CurrentMove = null;
    [HideInInspector]
    public bool InWanderZone = false;

    // Misc
    EEnemyType type = EEnemyType.DEFAULT;
    bool isTarget = false;
    bool isAttacking = false;
    bool hasAlreadyAttacked = false;

    private void Start()
    {
        Material = GetComponent<MeshRenderer>().material;
        WeaponHitbox = GetComponentInChildren<EnemyWeaponHitbox>();
        collid = GetComponent<Collider>();

        lives = maxLives;
        lifeText.text = lives.ToString();
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
            states.Add(EEnemyState.WANDER, new EnemyStateWander(this, waitBeforeNextMove));
            states.Add(EEnemyState.CHASE, new EnemyStateChase(this));
            states.Add(EEnemyState.PREPARE_ATTACK, new EnemyStatePrepareAttack(this, waitBeforePrepareAnim, prepareAnimDuration));
            states.Add(EEnemyState.ATTACK, new EnemyStateAttack(this, waitBeforeAttackAnim, attackAnimDuration, attackForce));
            states.Add(EEnemyState.RECOVER_ATTACK, new EnemyStateRecoverAttack(this, recoverAnimDuration));
            states.Add(EEnemyState.COME_BACK, new EnemyStateComeBack(this));
            states.Add(EEnemyState.STUN, new EnemyStateStun(this));

            CurrentStateEnum = EEnemyState.WANDER;
            states.TryGetValue(CurrentStateEnum, out currentState);
            currentState.Enter();
        }
    }

    private void Update()
    {
        if (type == EEnemyType.DEFAULT)
        {
            EEnemyState newStateEnum = currentState.UpdateState(Time.deltaTime);
            ChangeState(newStateEnum);
        }
    }

    public void GetAttacked(bool onRythm)
    {
        lives--;
        if (lives == 0)
        {
            EventManager.Instance.Raise(new EnemyDeadEvent { enemy = this });
            Destroy(gameObject);
            return;
        }

        lifeText.text = lives.ToString() + " PV";

        if (onRythm)
            StartCoroutine(BlinkBlue());
        
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

    void ChangeState(EEnemyState newStateEnum)
    {
        EnemyState newState;
        if (newStateEnum != CurrentStateEnum && states.TryGetValue(newStateEnum, out newState))
        {
            SoundManager.BeatDetection bd = SoundManager.Instance.LastBeat;
            Debug.LogFormat("{0} to {1} : {2}s error, interval {3}", CurrentStateEnum, newStateEnum, (bd.lastTimeBeat + bd.beatInterval) - TimeManager.Instance.SampleCurrentTime(), bd.beatInterval);
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
        UpdateColor();
    }

    public void SetSelected(bool selected)
    {
        isTarget = selected;
        UpdateColor();
    }

    private void UpdateColor()
    {
        if (isTarget)
            Material.color = Color.green;
        else
            Material.color = Color.yellow;
    }

    public void SetStateText(string text)
    {
        stateText.text = text;
    }

    public void KillCurrentTween()
    {
        if (CurrentMove != null)
        {
            CurrentMove.Kill();
            CurrentMove = null;
        }
    }

    public void StartAttacking()
    {
        isAttacking = true;
        hasAlreadyAttacked = false;
        collid.isTrigger = true;
    }

    public void StopAttacking()
    {
        isAttacking = false;
        collid.isTrigger = false;
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

    private void OnTriggerEnter(Collider other)
    {
        if (isAttacking && !hasAlreadyAttacked && other.gameObject.CompareTag("Player"))
        {
            Player.ModifyPulseValue(attackDamage, true);
            hasAlreadyAttacked = true;
        }
    }

    private void OnDestroy()
    {
        if (type == EEnemyType.DEFAULT)
        {
            states.Clear();
        }
    }
}
