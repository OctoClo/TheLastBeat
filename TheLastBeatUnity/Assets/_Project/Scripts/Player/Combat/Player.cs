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
    Vector3 movement = Vector3.zero;
    Rigidbody rb = null;
    [TabGroup("Movement")] [SerializeField]
    float rotationSpeed = 8;
    Vector3 lookVector = Vector3.zero;
    Vector3 rotation = Vector3.zero;
    Quaternion deltaRotation = Quaternion.identity;
    public Vector3 CurrentDirection { get; set; }

    [TabGroup("Movement")] [Range(0, 1)] [SerializeField]
    float holdlessThreshold = 0.7f;
    [TabGroup("Movement")] [SerializeField]
    bool debugMode = false;
    [TabGroup("Movement")] [SerializeField]
    GameObject prefabDebug = null;
    GameObject instantiatedDebug;

    //If you are doing something (dash , attack animation, etc...) or if game paused, temporary block input
    public override bool BlockInput => (blockInput || Status.Dashing || Status.Stunned || Status.Blinking);

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
    Pyramid pyr;
    [TabGroup("References")] [SerializeField]
    Transform visualPart = null;
    public Transform VisualPart => visualPart;
    public Transform CurrentFootOnGround { get; private set; }
    [TabGroup("References")] [SerializeField]
    AK.Wwise.Event stopEvent = null;
    [TabGroup("References")] [SerializeField]
    AK.Wwise.Event hitPlayer = null;

    [HideInInspector]
    public bool LoseLifeOnAbilities = true;
    [HideInInspector]
    public PlayerStatus Status { get; private set; }
    [HideInInspector]
    public PlayerAnim Anim = null;
    [HideInInspector]
    public GameObject ColliderObject = null;
    public Collider CurrentTarget => pyr.NearestEnemy;

    public void SetFoot(Transform trsf)
    {
        CurrentFootOnGround = trsf;
    }

    private void Start()
    {
        Status = GetComponent<PlayerStatus>();
        Anim = GetComponent<PlayerAnim>();
        ColliderObject = GetComponent<CapsuleCollider>().gameObject;
        rb = GetComponent<Rigidbody>();
        healthSystem.Player = this;

        blinkParameters.AttachedPlayer = rushParameters.AttachedPlayer = rushRewindParameters.AttachedPlayer = this;
        BlinkAbility blink = new BlinkAbility(blinkParameters);
        abilities.Add(EInputAction.BLINK, blink);

        rushParameters.blinkAbility = blink;
        RushAbility rush = new RushAbility(rushParameters);
        abilities.Add(EInputAction.RUSH, rush);

        RewindRushAbility rewindRush = new RewindRushAbility(rushRewindParameters);
        abilities.Add(EInputAction.REWINDRUSH, rewindRush);
        rush.RewindRush = rewindRush;

        if (SceneHelper.DeathCount > 0)
            SceneHelper.Instance.Respawn(transform);

        if (debugMode)
            instantiatedDebug = Instantiate(prefabDebug);
    }

    public override void ProcessInput(Rewired.Player player)
    {
        // Direction Inputs
        CurrentDirection = new Vector3(player.GetAxis("MoveX"), 0, player.GetAxis("MoveY"));
        Anim.SetMovement(CurrentDirection);

        // Abilities Inputs
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
        }

        Vector3 directionLook = new Vector3(player.GetAxis("FocusX"), 0, player.GetAxis("FocusY"));
        if (directionLook.magnitude > holdlessThreshold)
        {
            pyr.gameObject.GetComponent<Collider>().enabled = true;
            pyr.transform.forward = new Vector3(directionLook.x, 0, directionLook.z);
            pyr.RecomputeNearest();
            pyr.RecomputePositions();
        }
        else
        {
            if (CurrentTarget != null)
            {
                Vector3 positionToLook = new Vector3(CurrentTarget.transform.position.x, transform.position.y, CurrentTarget.transform.position.z);
                pyr.transform.LookAt(positionToLook);
            }
            else
            {
                pyr.gameObject.GetComponent<Collider>().enabled = false;
                pyr.Purge();
            }
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
        }
    }

    public void ModifyPulseValue(float value, bool fromEnemy = false)
    {
        if (healthSystem.InCriticMode)
        {
            Die();
        }
        else
        {
            healthSystem.ModifyPulseValue(value);
            if (fromEnemy)
            {
                Ability rewindRush;
                if (abilities.TryGetValue(EInputAction.REWINDRUSH, out rewindRush))
                    ((RewindRushAbility)rewindRush).ResetCooldown();
            }
        }
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

        if (Status.Dashing && CurrentTarget)
        {
            Vector3 positionToLook = new Vector3(CurrentTarget.transform.position.x, transform.position.y, CurrentTarget.transform.position.z);
            pyr.transform.LookAt(positionToLook);
        }

        if (debugMode)
        {
            if (CurrentTarget != null)
            {
                instantiatedDebug.SetActive(true);
                instantiatedDebug.transform.position = CurrentTarget.transform.position + Vector3.up;
            }
            else
            {
                instantiatedDebug.SetActive(false);
            }
        }
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
