using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
using DG.Tweening;
using System;

public class Player : Inputable
{
    [TabGroup("General")] [SerializeField]
    float hitFreezeFrameDuration = 0.2f;
    [TabGroup("General")] [SerializeField]
    bool startWithRewind = false;

    [TabGroup("Movement")] [SerializeField]
    public float maxSpeed = 5;
    [TabGroup("Movement")] [SerializeField]
    float thrust = 10;
    bool previouslyMoving = false;
    public Vector3 CurrentDirection { get; set; }
    public float CurrentDeltaY { get; private set; }
    Rigidbody rb = null;
    public Rigidbody Rb => rb;
    [TabGroup("Movement")] [SerializeField]
    float rotationSpeed = 8;
    Vector3 rotation = Vector3.zero;
    Quaternion deltaRotation = Quaternion.identity;
    [TabGroup("Movement")] [Range(0, 1)] [SerializeField] [Tooltip("Ignore input for right stick under this value")]
    float holdlessThreshold = 0.7f;
    [TabGroup("Movement")] [SerializeField]
    AK.Wwise.Event playClotheSFX = null;
    [TabGroup("Movement")] [SerializeField]
    AK.Wwise.Event stopClotheSFX = null;

    //If you are doing something (dash , attack animation, etc...) or if game paused, temporary block input
    public override bool BlockInput => blockInput || Status.CurrentStatus != EPlayerStatus.DEFAULT || end;
    bool end = false;
    bool runToEnd = false;

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
    [TabGroup("References")]
    [SerializeField]
    Cinemachine.CinemachineTargetGroup targetGroup = null;
    public Cinemachine.CinemachineTargetGroup TargetGroup => targetGroup;

    public Transform VisualPart => visualPart;
    public Transform CurrentFootOnGround { get; private set; }
    [TabGroup("References")] [SerializeField]
    AK.Wwise.Event stopEvent = null;
    [TabGroup("References")] [SerializeField]
    AK.Wwise.Event hitPlayer = null;
    [TabGroup("References")]
    public GameObject RushParticles = null;
    [TabGroup("References")] [SerializeField]
    GameObject hitVfxPrefab = null;
    [TabGroup("References")] [SerializeField]
    GameObject lastHitVfxPrefab = null;

    [HideInInspector]
    public bool LoseLifeOnAbilities = true;
    [HideInInspector]
    public PlayerStatus Status { get; private set; }
    [HideInInspector]
    public GameObject ColliderObject = null;
    public Collider CurrentTarget => pyramid.NearestEnemy;

    GameObject skinnedRenderer = null;
    public delegate void noParams();
    public event noParams OnOk;
    //Used for blink
    public bool InDanger {get; set;}
    Vector3 previousDirection = Vector3.zero;

    private void Awake()
    {
        Cursor.visible = false;
        if (SceneHelper.DeathCount > 0)
        {
            skinnedRenderer = GetComponentInChildren<SkinnedMeshRenderer>().gameObject;
            skinnedRenderer.SetActive(false);
        }
    }

    public void Reappear()
    {
        skinnedRenderer.SetActive(true);
    }

    private void Start()
    {
        InDanger = false;
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

        if (SceneHelper.DeathCount > 0)
        {
            SceneHelper.Instance.Respawn();
        }

        if (startWithRewind)
            AddRewindRush();
    }

    public void AddRewindRush()
    {
        RewindRushAbility rewindRush = new RewindRushAbility(rushRewindParameters, rushRewindParameters.HealPerCorrectBeat);
        abilities.Add(EInputAction.REWINDRUSH, rewindRush);
        (abilities[EInputAction.RUSH] as RushAbility).RewindRush = rewindRush;
    }

    public override void OnInputExit()
    {
        CurrentDirection = Vector3.zero;
    }

    public override void ProcessInput(Rewired.Player player)
    {
        if (!BlockInput)
        {
            // Direction Inputs
            CurrentDirection = new Vector3(player.GetAxis("MoveX"), 0, player.GetAxis("MoveY"));
            pyramid.LeftStickEnabled = (CurrentDirection != Vector3.zero);

            Ability ability = null;
            foreach (EInputAction action in (EInputAction[])Enum.GetValues(typeof(EInputAction)))
            {
                if (player.GetButtonDown(action.ToString()) && abilities.TryGetValue(action, out ability))
                    ability.Launch();
            }

            HandlePyramid(player);
        }

        if (player.GetButtonDown("Ok") && OnOk != null)
            OnOk();

        HandlePyramid(player);
    }

    private void HandlePyramid(Rewired.Player player)
    {
        Vector3 directionLook = new Vector3(player.GetAxis("FocusX"), 0, player.GetAxis("FocusY"));
        bool pyramidEnabled = (directionLook.magnitude > holdlessThreshold);
        pyramid.RightStickEnabled = pyramidEnabled;
        if (pyramidEnabled)
            pyramid.OverlookDirection(directionLook);

        if (CurrentTarget != null)
        {
            Vector3 positionToLook = new Vector3(CurrentTarget.transform.position.x, transform.position.y, CurrentTarget.transform.position.z);
            pyramid.transform.LookAt(positionToLook);
        }

        if (CurrentDirection != Vector3.zero && !pyramid.RightStickEnabled)
            pyramid.OverlookDirection(CurrentDirection);
    }

    private void FixedUpdate()
    {
        if (!BlockInput || runToEnd)
        {
            if (runToEnd)
                CurrentDirection = new Vector3(0, 0, 1);

            if (CurrentDirection != Vector3.zero)
            {
                // Rotation
                rotation = new Vector3(0, Vector3.SignedAngle(transform.forward, CurrentDirection, Vector3.up), 0);
                deltaRotation = Quaternion.Euler(rotation * Time.deltaTime * rotationSpeed);
                rb.MoveRotation(rb.rotation * deltaRotation);

                // Movement
                rb.AddForce(CurrentDirection * thrust);
                SetMoving(true);

                // Clamp velocity
                Vector3 flatVelocity = new Vector3(rb.velocity.x, 0, rb.velocity.z);
                if (flatVelocity.magnitude > maxSpeed && Status.CurrentStatus == EPlayerStatus.DEFAULT)
                    rb.AddForce(flatVelocity.normalized * maxSpeed);
            }
            else
            {
                SetMoving(false);
            }

            if (Status.CurrentStatus == EPlayerStatus.DEFAULT && previousDirection != Vector3.zero && CurrentDirection == Vector3.zero)
            {
                rb.velocity = new Vector3(0, rb.velocity.y, 0);
            }
        }
        else
            SetMoving(false);

        if (Status.CurrentStatus != EPlayerStatus.RUSHING && Status.CurrentStatus !=  EPlayerStatus.BLINKING)
            rb.velocity = Vector3.ClampMagnitude(rb.velocity, maxSpeed);

        rb.AddForce(Physics.gravity * 3, ForceMode.Acceleration);

        previousDirection = CurrentDirection;        
    }

    private void Update()
    {
        foreach (KeyValuePair<EInputAction, Ability> abilityPair in abilities)
            abilityPair.Value.Update(Time.deltaTime);

        HandleMovementY();
    }

    private void SetMoving(bool moving)
    {
        if (!previouslyMoving && moving)
        {
            playClotheSFX.Post(gameObject);
        }
        else if (previouslyMoving && !moving)
        {
            stopClotheSFX.Post(gameObject);
        }

        Status.SetMoving(moving);
        previouslyMoving = moving;
    }

    private void HandleMovementY()
    {
        Ray ray = new Ray(transform.position, Vector3.down);
        bool foundFirst = false;
        Vector3 hitFirst = Vector3.zero;
        foreach (RaycastHit hit in Physics.RaycastAll(ray, 3))
        {
            if (hit.collider.gameObject.CompareTag("Slope"))
            {
                hitFirst = hit.point;
                foundFirst = true;
                break;
            }
        }

        Vector3 flatForward = new Vector3(transform.forward.x, 0, transform.forward.z);
        Ray ray2 = new Ray(transform.position + (flatForward.normalized * 0.01f), Vector3.down);
        bool foundSecond = false;
        Vector3 hitSecond = Vector3.zero;
        foreach (RaycastHit hit in Physics.RaycastAll(ray2, 3))
        {
            if (hit.collider.gameObject.CompareTag("Slope"))
            {
                foundSecond = true;
                hitSecond = hit.point;
                break;
            }
        }

        if (foundFirst && foundSecond)
            CurrentDeltaY = (hitSecond - hitFirst).normalized.y;
        else
            CurrentDeltaY = 0;
    }

    public void SetFoot(Transform trsf)
    {
        CurrentFootOnGround = trsf;
    }

    public void LaunchEnd()
    {
        end = true;
        runToEnd = true;
    }

    public void TpToLastPosition(Vector3 lastPosition)
    {
        runToEnd = false;
        CurrentDirection = Vector3.zero;
        rb.velocity = Vector3.zero;
        SetMoving(false);
        transform.position = lastPosition;
    }

    public void CancelRush()
    {
        (abilities[EInputAction.RUSH] as RushAbility).Cancel();
    }

    public void DebtRush(float value)
    {
        (abilities[EInputAction.RUSH] as RushAbility).AddDebt(value);
    }

    bool currentlyHit = false;
    public void ModifyPulseValue(float value, bool fromEnemy = false)
    {
        if (fromEnemy)
        {
            if (!currentlyHit && Status.CurrentStatus != EPlayerStatus.RUSHING && Status.CurrentStatus != EPlayerStatus.BLINKING)
            {
                bool deathIncoming = DeathIncoming(value);

                // Game feel
                /*DOTween.Sequence()
                    .AppendCallback(() => SceneHelper.Instance.StartFade(() => { }, 0.2f, SceneHelper.Instance.ColorSlow, true))
                    .InsertCallback(0, () => SceneHelper.Instance.FreezeFrameTween(0.2f))
                    .AppendCallback(() => SceneHelper.Instance.StartFade(() => { }, 0.2f, Color.clear, true));*/

                // Decrease musical layer
                Ability rush;
                if (abilities.TryGetValue(EInputAction.RUSH, out rush))
                    ((RushAbility)rush).LayerLost();

                // Reset rewind cooldown
                Ability rewindRush;
                if (abilities.TryGetValue(EInputAction.REWINDRUSH, out rewindRush))
                    ((RewindRushAbility)rewindRush).ResetCooldown();

                // Animation
                StartCoroutine(SioHitAnim(value, deathIncoming));

                if (deathIncoming)
                    Die();
            }
        }
        else
        {
            if (DeathIncoming(value))
            {
                Die();
                return;
            }

            healthSystem.ModifyPulseValue(value, false);
        }
    }

    bool DeathIncoming(float value)
    {
        return (healthSystem.InCriticMode && value > 0);
    }

    IEnumerator SioHitAnim(float value, bool dying)
    {
        currentlyHit = true;
        healthSystem.ModifyPulseValue(value, true);
        Instantiate(hitVfxPrefab, transform.position, Quaternion.identity, transform);
        if (dying)
            Instantiate(lastHitVfxPrefab, transform.position, Quaternion.identity, transform);
        Status.GetHit();
        yield return StartCoroutine(SceneHelper.Instance.FreezeFrameCoroutine(hitFreezeFrameDuration));
        Status.StopHit();
        currentlyHit = false;
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

    public void GetStunned(Vector3 kickbackDirection)
    {
        Status.GetStunned(kickbackDirection);
        healthSystem.GetStunned();
    }

    void Die()
    {
        DOTween.KillAll();
        blockInput = true;
        healthSystem.Dying = true;

        foreach (Enemy enn in GameObject.FindObjectsOfType<Enemy>())
            enn.Timescale = 0;

        SceneHelper.Instance.RecordDeath(transform.position);
        stopEvent.Post(gameObject);
        
        Status.DieAnimation();
    }
}
