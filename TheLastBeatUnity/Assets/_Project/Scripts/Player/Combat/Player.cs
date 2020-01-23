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
    [TabGroup("General")] [SerializeField]
    float hitFreezeFrameDuration = 0.2f;

    [TabGroup("Movement")] [SerializeField]
    float speed = 7.5f;
    Vector3 movement = Vector3.zero;
    Rigidbody rb = null;
    [TabGroup("Movement")] [SerializeField]
    float rotationSpeed = 8;
    Vector3 rotation = Vector3.zero;
    Quaternion deltaRotation = Quaternion.identity;
    public Vector3 CurrentDirection { get; set; }
    [TabGroup("Movement")] [Range(0, 1)] [SerializeField] [Tooltip("Ignore input for right stick under this value")]
    float holdlessThreshold = 0.7f;

    //If you are doing something (dash , attack animation, etc...) or if game paused, temporary block input
    public override bool BlockInput => blockInput;

    [TabGroup("Blink")] [SerializeField]
    BlinkParams blinkParameters = null;
    [TabGroup("Rush")] [SerializeField]
    RushParams rushParameters = null;
    [TabGroup("Rush")] [SerializeField]
    RewindRushParameters rushRewindParameters = null;
    Dictionary<EInputAction, Ability> abilities = new Dictionary<EInputAction, Ability>();

    [TabGroup("References")] [SerializeField]
    DelegateCollider delegateColl = null;
    public DelegateCollider DelegateColl => delegateColl;
    [TabGroup("References")] [SerializeField]
    Health healthSystem = null;
    [TabGroup("References")] [SerializeField]
    Pyramid pyramid = null;
    [TabGroup("References")] [SerializeField]
    Transform visualPart = null;
    public Transform VisualPart => visualPart;
    public Transform CurrentFootOnGround { get; private set; }
    [TabGroup("References")] [SerializeField]
    AK.Wwise.Event stopEvent = null;
    [TabGroup("References")] [SerializeField]
    AK.Wwise.Event hitPlayer = null;
    [TabGroup("References")]
    public GameObject RushParticles = null;

    [HideInInspector]
    public bool LoseLifeOnAbilities = true;
    [HideInInspector]
    public PlayerStatus Status { get; private set; }
    [HideInInspector]
    public GameObject ColliderObject = null;
    public Collider CurrentTarget => pyramid.NearestEnemy;

    public void SetFoot(Transform trsf)
    {
        CurrentFootOnGround = trsf;
    }

    private void Start()
    {
        Status = GetComponent<PlayerStatus>();
        ColliderObject = GetComponent<CapsuleCollider>().gameObject;
        rb = GetComponent<Rigidbody>();
        healthSystem.Player = this;

        blinkParameters.AttachedPlayer = rushParameters.AttachedPlayer = rushRewindParameters.AttachedPlayer = this;
        BlinkAbility blink = new BlinkAbility(blinkParameters, blinkParameters.HealPerCorrectBeat);
        abilities.Add(EInputAction.BLINK, blink);

        rushParameters.blinkAbility = blink;
        RushAbility rush = new RushAbility(rushParameters, rushParameters.HealPerCorrectBeat);
        abilities.Add(EInputAction.RUSH, rush);

        RewindRushAbility rewindRush = new RewindRushAbility(rushRewindParameters, rushRewindParameters.HealPerCorrectBeat);
        abilities.Add(EInputAction.REWINDRUSH, rewindRush);
        rush.RewindRush = rewindRush;

        if (SceneHelper.DeathCount > 0)
            SceneHelper.Instance.Respawn(transform);
    }

    public void DebtRush(float value)
    {
        (abilities[EInputAction.RUSH] as RushAbility).AddDebt(value);
    }

    public override void ProcessInput(Rewired.Player player)
    {
        // Direction Inputs
        CurrentDirection = new Vector3(player.GetAxis("MoveX"), 0, player.GetAxis("MoveY"));
        pyramid.LeftStickEnabled = (CurrentDirection != Vector3.zero);

        // Abilities Inputs
        if (Status.CurrentStatus == EPlayerStatus.DEFAULT)
        {
            Ability ability = null;

            foreach (EInputAction action in (EInputAction[])Enum.GetValues(typeof(EInputAction)))
            {
                if (player.GetButtonDown(action.ToString()) && abilities.TryGetValue(action, out ability))
                {
                    ability.Launch();
                }
            }
        }

        Vector3 directionLook = new Vector3(player.GetAxis("FocusX"), 0, player.GetAxis("FocusY"));
        if (directionLook.magnitude > holdlessThreshold)
        {
            pyramid.RightStickEnabled = true;
            pyramid.OverlookDirection(directionLook);
        }
        else
        {
            pyramid.RightStickEnabled = false;
        }

        if (CurrentTarget != null)
        {
            Vector3 positionToLook = new Vector3(CurrentTarget.transform.position.x, transform.position.y, CurrentTarget.transform.position.z);
            pyramid.transform.LookAt(positionToLook);
        }

        if (CurrentDirection != Vector3.zero && !pyramid.RightStickEnabled)
        {
            pyramid.OverlookDirection(CurrentDirection);
        }
    }

    private void FixedUpdate()
    {
        if (!BlockInput && CurrentDirection != Vector3.zero)
        {
            // Rotation
            rotation = new Vector3(0, Vector3.SignedAngle(transform.forward, CurrentDirection, Vector3.up), 0);
            deltaRotation = Quaternion.Euler(rotation * Time.deltaTime * rotationSpeed);
            rb.MoveRotation(rb.rotation * deltaRotation);

            // Movement
            movement = CurrentDirection * speed;
            rb.velocity = movement;
            Status.SetMoving(true);
        }
        else
            Status.SetMoving(false);
    }

    public void ModifyPulseValue(float value, bool fromEnemy = false)
    {
        if (healthSystem.InCriticMode)
        {
            Die();
        }
        else
        {
            if (!fromEnemy)
                healthSystem.ModifyPulseValue(value);
            else if (Status.CurrentStatus != EPlayerStatus.RUSHING && Status.CurrentStatus != EPlayerStatus.BLINKING)
            {
                Ability rewindRush;
                if (abilities.TryGetValue(EInputAction.REWINDRUSH, out rewindRush))
                    ((RewindRushAbility)rewindRush).ResetCooldown();
                StartCoroutine(SioHitAnim(value));
            }
        }
    }

    IEnumerator SioHitAnim(float value)
    {
        Status.GetHit();
        yield return StartCoroutine(SceneHelper.Instance.FreezeFrameCoroutine(hitFreezeFrameDuration));
        Status.StopHit();
        healthSystem.ModifyPulseValue(value);
    }

    void Die()
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
        Status.SetMoving(false);

        while (cursor < objective)
        {
            float tempValue = (objective * Time.deltaTime / duration);
            cursor += tempValue;
            transform.Rotate(Vector3.right * tempValue, Space.Self);
            yield return null;
        }

        SceneHelper.Instance.RecordDeath(transform.position);
        SceneHelper.Instance.StartFade(() => SceneManager.LoadScene(SceneManager.GetActiveScene().name), 0.5f, Color.black);
    }

    private void Update()
    {
        foreach (KeyValuePair<EInputAction, Ability> abilityPair in abilities)
            abilityPair.Value.Update(Time.deltaTime);
    }

    IEnumerator currentHurt;
    Dictionary<Material, Color> allColors = new Dictionary<Material, Color>();
    public void HurtAnimation(float timeScale, int nbLoop)
    {
        if (currentHurt != null)
        {
            StopCoroutine(currentHurt);

            //Get back colors
            foreach (Material mat in visualPart.GetComponentInChildren<SkinnedMeshRenderer>().materials)
            {
                mat.color = allColors[mat];
            }
        }

        hitPlayer.Post(gameObject);
        currentHurt = HurtCoroutine(timeScale, nbLoop);
        StartCoroutine(currentHurt);
    }

    IEnumerator HurtCoroutine(float timeScale, int nbLoop)
    {
        foreach (Material mat in visualPart.GetComponentInChildren<SkinnedMeshRenderer>().materials)
        {
            allColors[mat] = mat.color;
        }
        float cursorTime = 0;

        for (int i = 0; i < nbLoop; i++)
        {
            while (cursorTime < 1)
            {
                cursorTime += Time.deltaTime / timeScale;
                foreach (Material mat in allColors.Keys)
                {
                    mat.color = Color.Lerp(allColors[mat], Color.red, cursorTime);
                }
                yield return null;
            }

            while (cursorTime > 0)
            {
                cursorTime -= Time.deltaTime / timeScale;
                foreach (Material mat in allColors.Keys)
                {
                    mat.color = Color.Lerp(allColors[mat], Color.red, cursorTime);
                }
                yield return null;
            }
        }
    }
}
