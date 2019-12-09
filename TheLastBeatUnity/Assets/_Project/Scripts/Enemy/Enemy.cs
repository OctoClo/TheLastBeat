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
    [TabGroup("General")] [SerializeField]
    TextMeshProUGUI lifeText = null;
    [TabGroup("General")] [SerializeField]
    TextMeshProUGUI stateText = null;

    [TabGroup("Stun")] [SerializeField]
    float stunDuration = 1;
    float stunTimer = 0;
    bool stunned = false;
    [TabGroup("Stun")] [SerializeField] [Range(0.0f, 1.0f)]
    float[] chancesToGetStunned = new float[5];
    int stunCounter = 0;

    EEnemyType type = EEnemyType.DEFAULT;
    bool isTarget = false;
    Material material = null;

    public int PulseDamage = 5;
    public EnemyWeaponHitbox WeaponHitbox { get; private set; }

    Dictionary<EEnemyState, EnemyState> states;
    public EEnemyState CurrentStateEnum { get; private set; }
    EnemyState currentState;

    [HideInInspector]
    public bool ComeBack = false;
    [HideInInspector]
    public bool ChaseAgain = false;
    [HideInInspector]
    public Sequence CurrentMove = null;

    public EnemyDetectionZone DetectionZone { get; private set; }
    public EnemyWanderZone WanderZone { get; private set; }
    public Player Player { get; private set; }

    private void Start()
    {
        material = GetComponent<MeshRenderer>().material;
        WeaponHitbox = GetComponentInChildren<EnemyWeaponHitbox>();

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
            states.Add(EEnemyState.WANDER, new EnemyStateWander(this));
            states.Add(EEnemyState.CHASE, new EnemyStateChase(this));
            states.Add(EEnemyState.PREPARE_ATTACK, new EnemyStatePrepareAttack(this));
            states.Add(EEnemyState.ATTACK, new EnemyStateAttack(this));
            states.Add(EEnemyState.RECOVER_ATTACK, new EnemyStateRecoverAttack(this));
            states.Add(EEnemyState.COME_BACK, new EnemyStateComeBack(this));

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
            EnemyState newState;
            if (newStateEnum != CurrentStateEnum && states.TryGetValue(newStateEnum, out newState))
            {
                currentState.Exit();
                CurrentStateEnum = newStateEnum;
                currentState = newState;
                currentState.Enter();
            }
        }

        /*if (stunTimer > 0)
        {
            stunTimer -= Time.deltaTime;

            if (stunTimer <= 0 && stunned)
            {
                UpdateColor();
                stunned = false;
            }
        }*/
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
        
        /*if (stunCounter < chancesToGetStunned.Length)
        {
            float stunPercentage = RandomHelper.GetRandom();
            if (stunPercentage < chancesToGetStunned[stunCounter])
            {
                stunned = true;
                stunCounter++;
                stunTimer = stunDuration;
                material.color = Color.blue;
            }
        }*/
    }

    IEnumerator BlinkBlue()
    {
        material.color = Color.blue;
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
            material.color = Color.green;
        else
            material.color = Color.red;
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
}
