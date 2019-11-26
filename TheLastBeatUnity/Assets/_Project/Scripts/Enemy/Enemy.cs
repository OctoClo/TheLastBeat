using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
using TMPro;
using DG.Tweening;

public class EnemyDeadEvent : GameEvent { public Enemy enemy; }

public class Enemy : MonoBehaviour
{
    [TabGroup("General")] [SerializeField]
    float speed = 5;
    public float Speed => speed;

    [TabGroup("General")] [SerializeField]
    int maxLives = 10;
    int lives = 10;
    [TabGroup("General")] [SerializeField]
    int pulseDamage = 5;
    public EnemyWeaponHitbox WeaponHitbox { get; private set; }
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
    Material material = null;

    Transform player = null;

    bool isTarget = false;

    Dictionary<EEnemyState, EnemyState> states;
    EEnemyState currentStateEnum;
    EnemyState currentState;

    private void Start()
    {
        material = GetComponent<MeshRenderer>().material;
        WeaponHitbox = GetComponentInChildren<EnemyWeaponHitbox>();

        lives = maxLives;
        lifeText.text = lives.ToString();
    }

    public void ZoneInitialize(EnemyWanderZone newWanderZone, EnemyDetectionZone newDetectionZone, Transform newPlayer)
    {
        player = newPlayer;

        states = new Dictionary<EEnemyState, EnemyState>();
        states.Add(EEnemyState.WANDER, new EnemyStateWander(this, newWanderZone, newDetectionZone));
        states.Add(EEnemyState.CHASE, new EnemyStateChase(this, player));
        states.Add(EEnemyState.PREPARE_ATTACK, new EnemyStatePrepareAttack(this));
        states.Add(EEnemyState.ATTACK, new EnemyStateAttack(this));

        currentStateEnum = EEnemyState.WANDER;
        states.TryGetValue(currentStateEnum, out currentState);
        currentState.Enter();
    }

    private void Update()
    {
        EEnemyState newStateEnum = currentState.UpdateState(Time.deltaTime);
        EnemyState newState;
        if (newStateEnum != currentStateEnum && states.TryGetValue(newStateEnum, out newState))
        {
            currentState.Exit();
            currentStateEnum = newStateEnum;
            currentState = newState;
            currentState.Enter();
        }
        
        /*stunTimer -= Time.deltaTime;

        if (stunTimer <= 0 && stunned)
        {
            material.color = isTarget ? Color.green : Color.red;
            stunned = false;
        }*/
    }

    private void FixedUpdate()
    {
        currentState.FixedUpdateState();
    }

    public void GetAttacked()
    {
        lives--;
        if (lives == 0)
        {
            EventManager.Instance.Raise(new EnemyDeadEvent { enemy = this });
            Destroy(gameObject);
            return;
        }

        lifeText.text = lives.ToString() + " PV";
        
        /*if (stunCounter < chancesToGetStunned.Length)
        {
            float stunPercentage = Random.value;
            if (stunPercentage < chancesToGetStunned[stunCounter])
            {
                stunned = true;
                stunCounter++;
                stunTimer = stunDuration;
                material.color = Color.blue;
            }
        }*/
    }

    public void SetSelected(bool selected)
    {
        if (selected)
            material.color = Color.green;
        else
            material.color = Color.red;
    }

    public void SetStateText(string text)
    {
        stateText.text = text;
    }
}
