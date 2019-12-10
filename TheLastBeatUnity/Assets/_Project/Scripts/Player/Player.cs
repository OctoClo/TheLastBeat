using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Rewired;
using Sirenix.OdinInspector;
using DG.Tweening;
using System;
using UnityEngine.SceneManagement;
using System.Linq;

public class Player : Inputable
{
    [TabGroup("Movement")] [SerializeField]
    float speed = 7.5f;
    [TabGroup("Movement")] [SerializeField]
    float maxRotationPerFrame = 30;

    [TabGroup("Movement")] [SerializeField]
    Transform groundRotationReference;

    public Vector3 CurrentDirection { get; set; }

    //If you are doing something (dash , attack animation, etc...) or if game paused, temporary block input
    public override bool BlockInput => (blockInput || Status.Dashing || Status.Stunned || Status.Blinking);

    [TabGroup("Blink")] [SerializeField]
    BlinkParams blinkParameters = null;

    [TabGroup("Rush")][SerializeField]
    RushParams rushParameters = null;

    [TabGroup("Rush")][SerializeField]
    RewindRushParameters rushRewindParameters = null;

    [SerializeField]
    AK.Wwise.Event stopEvent = null;

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
    Transform visualPart = null;
    public Transform VisualPart => visualPart;

    Dictionary<EInputAction, Ability> abilities = new Dictionary<EInputAction, Ability>();
    IReadOnlyDictionary<EInputAction, Ability> Abilities => abilities;

    [HideInInspector]
    public FocusZone FocusZone = null;

    [SerializeField]
    BeatManager beatManager = null;
    public BeatManager BeatManager => beatManager;

    public Enemy CurrentTarget { get; private set; }

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

        rushParameters.blinkAbility = blink;
        RushAbility rush = new RushAbility(rushParameters);
        abilities.Add(EInputAction.RUSH, rush);

        RewindRushAbility rewindRush = new RewindRushAbility(rushRewindParameters);
        abilities.Add(EInputAction.REWINDRUSH, rewindRush);

        rush.RewindRush = rewindRush;
        if (SceneHelper.DeathCount > 0)
        {
            SceneHelper.Instance.Respawn(transform);
        }
    }

    public override void ProcessInput(Rewired.Player player)
    {
        Vector3 direction = new Vector3(player.GetAxis("MoveX"), 0, player.GetAxis("MoveY"));
        CurrentDirection = direction;
        Anim.SetMovement(direction);

        // Look at
        LookAtCurrentTarget();
        if (!CurrentTarget && direction != Vector3.zero)
        {
            Vector3 lookVector = direction;
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

    public void LookAtCurrentTarget()
    {
        CurrentTarget = FocusZone.GetCurrentTarget();
        if (CurrentTarget)
        {
            Vector3 lookVector = new Vector3(CurrentTarget.transform.position.x, transform.position.y, CurrentTarget.transform.position.z) - transform.position;
            transform.rotation = Quaternion.RotateTowards(transform.rotation, Quaternion.LookRotation(lookVector), 360);
        }
    }

    public void Die()
    {
        DOTween.KillAll();
        blockInput = true;
        SceneHelper.Instance.RecordDeath(transform.position);
        stopEvent.Post(gameObject);
        StartCoroutine(DieCoroutine());
    }

    IEnumerator DieCoroutine()
    {
        float objective = 90;
        float duration = 0.5f;
        float cursor = 0;
        Anim.SetMovement(Vector3.zero);

        while (cursor < objective)
        {
            float tempValue = (objective * Time.deltaTime / duration);
            cursor += tempValue;
            transform.Rotate(Vector3.right * tempValue, Space.Self);
            yield return null;
        }

        SceneHelper.Instance.RecordDeath(transform.position);
        SceneHelper.Instance.StartFade(() => SceneManager.LoadScene(SceneManager.GetActiveScene().name), 2, Color.black);
    }

    private void Update()
    {
        foreach (KeyValuePair<EInputAction, Ability> abilityPair in abilities)
            abilityPair.Value.Update(Time.deltaTime);


        //Need remove
        if (Input.GetKeyDown(KeyCode.K))
            Die();
    }

    public Enemy GetCurrentTarget()
    {
        return FocusZone.GetCurrentTarget();
    }
}
