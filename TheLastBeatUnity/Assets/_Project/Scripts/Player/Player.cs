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
    float speed;
    Vector3 previousPos;

    [Space]
    [Header("Dash")]
    [SerializeField] [ValidateInput("Positive", "This value must be > 0")]
    float dashDuration;
    bool dashing;
    [SerializeField]
    float chainMaxDuration;
    float chainTimer;
    [SerializeField]
    [Tooltip("The longer it is, the longer it take to change frequency")]
    float dashImpactBeatDelay;
    [SerializeField]
    float stunDuration;
    float stunTimer;
    bool stunned;

    [Space]
    [Header("Dash effects")]
    [SerializeField]
    CameraEffect cameraEffect;
    [SerializeField]
    float zoomDuration;
    [SerializeField]
    float zoomValue;
    [SerializeField]
    float slowMotionDuration;
    [SerializeField]
    ParticleSystem dashParticles;

    [Space]
    [Header("Blink")]
    [SerializeField]
    float blinkSpeed;

    [Space]
    [Header("References")]
    [SerializeField] [Required]
    Health health = null;
    [SerializeField] [Required]
    FocusZone focusZone;
    [SerializeField]
    float maxRotationPerFrame;

    public Vector3 DeltaMovement { get; set; }

    List<Enemy> chainedEnemies;
    Enemy currentTarget;
    Material material;

    public bool Positive(float value)
    {
        return value > 0;
    }

    public delegate void ColliderParams(Collider coll);
    public event ColliderParams TriggerEnter;

    private void Start()
    {
        TimeManager.Instance.SetPlayer(this);

        previousPos = transform.position;
        material = GetComponent<MeshRenderer>().material;

        dashing = false;
        stunned = false;
        chainedEnemies = new List<Enemy>();
    }

    public void OnTriggerEnter(Collider other)
    {
        if (TriggerEnter != null)
            TriggerEnter(other);
    }

    //If you are doing something (dash , attack animation , etc...) temporary block input
    public override bool BlockInput => (blockInput || dashing || stunned);

    public override void ProcessInput(Rewired.Player player)
    {
        previousPos = transform.position;

        Vector3 direction = new Vector3(player.GetAxis("MoveX"), 0, player.GetAxis("MoveY"));
        DeltaMovement = movement;

        // Rotation
        currentTarget = focusZone.GetCurrentTarget();
        if (currentTarget)
            transform.rotation = Quaternion.RotateTowards(transform.rotation, Quaternion.LookRotation(currentTarget.transform.position - transform.position), maxRotationPerFrame);
        else if (movement != Vector3.zero)
        {
            Vector3 direction = movement;
            direction.Normalize();

            Vector3 forward = transform.forward;
            transform.rotation = Quaternion.RotateTowards(transform.rotation, Quaternion.LookRotation(direction), maxRotationPerFrame);
            movement = transform.forward;
        }
    }

    IEnumerator Blink(Vector3 direction)
    {
        //dashParticles.Play();
        direction.Normalize();
        transform.position = transform.position + direction * blinkSpeed;
        yield return new WaitForSecondsRealtime(1);
        //dashParticles.Stop();
    }

    void Dash()
    {
        dashing = true;
        focusZone.playerDashing = true;
        health.NewAction(1.5f, dashImpactBeatDelay);
        TimeManager.Instance.SlowEnemies();
        //cameraEffect.StartZoom(zoomValue, zoomDuration, CameraEffect.ZoomType.Distance, CameraEffect.ValueType.Absolute);

        Sequence seq = DOTween.Sequence();

        Vector3 direction = new Vector3(currentTarget.transform.position.x, transform.position.y, currentTarget.transform.position.z) - transform.position;

        RaycastHit hit = GetObstacleOnDash(direction);

        // Dash towards the target
        if (hit.collider)
            direction = new Vector3(hit.point.x, transform.position.y, hit.point.z) - transform.position;
        else
        {
            direction *= 1.3f;
            gameObject.layer = LayerMask.NameToLayer("Player Dashing");
        }

        Vector3 goalPosition = direction + transform.position;
        seq.Append(transform.DOMove(goalPosition, dashDuration));
        
        if (hit.collider)
        {
            direction *= -0.5f;
            goalPosition += direction;
            seq.Append(transform.DOMove(goalPosition, dashDuration / 2.0f));
        }

        seq.AppendCallback(() => EndDash(hit));
        seq.Play();
    }

    void EndDash(RaycastHit hit)
    {
        dashing = false;
        focusZone.playerDashing = false;
        TimeManager.Instance.ResetEnemies();

        if (hit.collider)
        {
            if (hit.collider.gameObject.layer == LayerMask.NameToLayer("Stun"))
            {
                stunned = true;
                stunTimer = stunDuration;
                material.color = Color.blue;
            }
        }
        else
        {
            currentTarget.GetAttacked();
            chainedEnemies.Add(currentTarget);
            chainTimer = chainMaxDuration;
            gameObject.layer = LayerMask.NameToLayer("Default");
            TimeManager.Instance.SetTimeScale(1);
            StartCoroutine(WaitDuringSlowMotion());
        }
    }

    IEnumerator WaitDuringSlowMotion()
    {
        yield return new WaitForSecondsRealtime(slowMotionDuration);
        TimeManager.Instance.SetTimeScale(1);
        //cameraEffect.StartZoom(-zoomValue, zoomDuration, CameraEffect.ZoomType.Distance, CameraEffect.ValueType.Absolute);
    }

    RaycastHit GetObstacleOnDash(Vector3 direction)
    {
        RaycastHit[] hits = Physics.RaycastAll(transform.position, direction, direction.magnitude);

        foreach (RaycastHit hit in hits)
        {
            if (hit.collider.gameObject.layer != LayerMask.NameToLayer("Enemies"))
                return hit;
        }

        return new RaycastHit();
    }

    void RewindDash()
    {
        dashing = true;
        focusZone.overrideControl = true;
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
                focusZone.OverrideCurrentEnemy(enemy);

                direction = new Vector3(enemy.transform.position.x, goalPosition.y, enemy.transform.position.z) - goalPosition;
                direction *= 1.3f;

                goalPosition += direction;
                seq.Append(transform.DOMove(goalPosition, dashDuration));
                seq.AppendCallback(() => { enemy.GetAttacked(); });
            }
        }

        seq.Play();

        dashing = false;
        focusZone.overrideControl = false;
        TimeManager.Instance.ResetEnemies();
        gameObject.layer = LayerMask.NameToLayer("Default");
        chainedEnemies.Clear();
    }

    private void Update()
    {
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
