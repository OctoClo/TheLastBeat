using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Rewired;
using Sirenix.OdinInspector;
using DG.Tweening;

public class Player : Inputable
{
    [Header("Movement")]
    [SerializeField]
    float speed = 7.5f;
    [SerializeField]
    float maxRotationPerFrame = 30;
    public Vector3 CurrentDirection { get; set; }

    //If you are doing something (dash , attack animation, etc...) or if game paused, temporary block input
    public override bool BlockInput => (blockInput || dashing || stunned);

    Dictionary<EInputAction, Ability> abilities;  
    
    

    [SerializeField]
    float chainMaxDuration = 2;
    float chainTimer = 0;
    List<Enemy> chainedEnemies = new List<Enemy>();

    bool dashing = false;

    [SerializeField]
    float stunDuration = 0.5f;
    float stunTimer = 0;
    bool stunned = false;

    [TabGroup("Rush")] [SerializeField] [ValidateInput("Positive", "This value must be > 0")]
    float rushDuration = 0.5f;
    [TabGroup("Rush")] [SerializeField] [ValidateInput("Positive", "This value must be > 0")]
    float rushZoomDuration = 0.5f;
    [TabGroup("Rush")] [SerializeField] [ValidateInput("Positive", "This value must be > 0")]
    float rushZoomValue = 5;
    [TabGroup("Rush")] [SerializeField] [ValidateInput("Positive", "This value must be > 0")]
    float rushSlowMoDuration = 0.5f;
    [TabGroup("Rush")] [SerializeField] [Tooltip("The longer it is, the longer it take to change frequency")]
    float rushImpactBeatDelay = 0;

    [TabGroup("Blink")] [SerializeField]
    float blinkSpeed = 5;
    [TabGroup("Blink")] [SerializeField]
    ParticleSystem blinkParticles = null;

    [Space] [Header("References")]
    [SerializeField] [Required]
    Health health = null;
    [SerializeField]
    CameraEffect cameraEffect = null;
    public FocusZone FocusZone = null;

    Enemy currentTarget = null;
    Material material = null;

    public bool Positive(float value)
    {
        return value > 0;
    }

    private void Start()
    {
        TimeManager.Instance.SetPlayer(this);
        material = GetComponent<MeshRenderer>().material;

        abilities = new Dictionary<EInputAction, Ability>();

        Ability blink = new BlinkAbility(this, blinkSpeed, blinkParticles);
        abilities.Add(EInputAction.BLINK, blink);

        Ability rush = new RushAbility(this, rushDuration, rushZoomDuration, rushZoomValue, rushSlowMoDuration, rushImpactBeatDelay);
        abilities.Add(EInputAction.RUSH, rush);
    }

    public override void ProcessInput(Rewired.Player player)
    {
        Vector3 direction = new Vector3(player.GetAxis("MoveX"), 0, player.GetAxis("MoveY"));
        CurrentDirection = direction;

        // Rotation
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

        // Translation and dashing
        if (!dashing)
        {
            if (player.GetButtonDown(EInputAction.BLINK.ToString()))
            {
                Ability blink = null;
                abilities.TryGetValue(EInputAction.BLINK, out blink);

                if (blink != null)
                    blink.Launch();
            }
            else if (player.GetButtonDown(EInputAction.RUSH.ToString()) && currentTarget)
            {
                Ability rush = null;
                abilities.TryGetValue(EInputAction.RUSH, out rush);

                if (rush != null)
                    rush.Launch();
            }
            else if (player.GetButtonDown("RewindRush"))
                RewindRush();
            else
            {
                Vector3 movement = direction * Time.deltaTime * speed;
                transform.Translate(movement, Space.World);
            }
        }
    }

    public Enemy GetCurrentTarget()
    {
        return FocusZone.GetCurrentTarget();
    }

    void RewindRush()
    {
        dashing = true;
        FocusZone.overrideControl = true;
        TimeManager.Instance.SlowEnemies();
        gameObject.layer = LayerMask.NameToLayer("Player Dashing");

        Sequence seq = DOTween.Sequence();
        Vector3 direction;
        Vector3 goalPosition = transform.position;
        chainedEnemies.Reverse();

        foreach (Enemy enemy in chainedEnemies)
        {
            if (enemy)
            {
                FocusZone.OverrideCurrentEnemy(enemy);

                direction = new Vector3(enemy.transform.position.x, goalPosition.y, enemy.transform.position.z) - goalPosition;
                direction *= 1.3f;

                goalPosition += direction;
                seq.Append(transform.DOMove(goalPosition, rushDuration));
                seq.AppendCallback(() => { enemy.GetAttacked(); });
            }
        }

        seq.Play();

        dashing = false;
        FocusZone.overrideControl = false;
        TimeManager.Instance.ResetEnemies();
        gameObject.layer = LayerMask.NameToLayer("Default");
        chainedEnemies.Clear();
    }

    private void Update()
    {
        foreach (KeyValuePair<EInputAction, Ability> abilityPair in abilities)
            abilityPair.Value.Update(Time.deltaTime * TimeManager.Instance.CurrentTimeScale);

        if (stunned)
        {
            stunTimer -= Time.deltaTime;
            
            if (stunTimer <= 0)
            {
                stunned = false;
                material.color = Color.white;
            }
        }

        if (chainedEnemies.Count > 0 && !dashing)
        {
            chainTimer -= Time.deltaTime;

            if (chainTimer < 0)
                chainedEnemies.Clear();
        }
    }
}
