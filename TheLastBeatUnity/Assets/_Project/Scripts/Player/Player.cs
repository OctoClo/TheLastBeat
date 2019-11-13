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
    public Vector3 CurrentDirection { get; set; }

    //If you are doing something (dash , attack animation, etc...) or if game paused, temporary block input
    public override bool BlockInput => (blockInput || dashing || stunned);

    [Space] [Header("Dash")]
    [SerializeField] [ValidateInput("Positive", "This value must be > 0")]
    float dashDuration = 0.5f;
    bool dashing = false;
    [SerializeField]
    float chainMaxDuration = 2;
    float chainTimer = 0;
    [SerializeField]
    float stunDuration = 0.5f;
    float stunTimer = 0;
    bool stunned = false;
    [SerializeField] [Tooltip("The longer it is, the longer it take to change frequency")]
    float dashImpactBeatDelay = 0;

    [Space] [Header("Dash effects")]
    [SerializeField]
    CameraEffect cameraEffect = null;
    [SerializeField]
    float zoomDuration = 0.5f;
    [SerializeField]
    float zoomValue = 5;
    [SerializeField]
    float slowMotionDuration = 0.5f;
    [SerializeField]
    ParticleSystem dashParticles = null;

    [Space] [Header("Blink")]
    [SerializeField]
    float blinkSpeed = 5;

    [Space] [Header("References")]
    [SerializeField] [Required]
    Health health = null;
    [SerializeField] [Required]
    FocusZone focusZone;
    [SerializeField]
    float maxRotationPerFrame;

    List<Enemy> chainedEnemies = new List<Enemy>();
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
    }

    public override void ProcessInput(Rewired.Player player)
    {
        Vector3 direction = new Vector3(player.GetAxis("MoveX"), 0, player.GetAxis("MoveY"));
        CurrentDirection = direction;

        // Rotation
        currentTarget = focusZone.GetCurrentTarget();
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
            if (player.GetButtonDown("Blink"))
                StartCoroutine(Blink(direction));
            else if (player.GetButtonDown("Rush") && currentTarget)
                Rush();
            else if (player.GetButtonDown("RewindRush"))
                RewindRush();
            else
            {
                Vector3 movement = direction * Time.deltaTime * speed;
                transform.Translate(movement, Space.World);
            }
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

    void Rush()
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

        seq.AppendCallback(() => EndRush(hit));
        seq.Play();
    }

    void EndRush(RaycastHit hit)
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

    void RewindRush()
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

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("CombatZone"))
        {
            CameraMachine machine = GameObject.FindObjectOfType<CameraMachine>();
            if (machine)
            {
                machine.EnterCombat(0.5f);
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("CombatZone"))
        {
            CameraMachine machine = GameObject.FindObjectOfType<CameraMachine>();
            if (machine)
            {
                machine.EnterOOC(0.5f);
            }
        }
    }
}
