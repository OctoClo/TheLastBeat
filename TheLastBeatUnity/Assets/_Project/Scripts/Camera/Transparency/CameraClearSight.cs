using UnityEngine;
using Cinemachine;

public class CameraClearSight : MonoBehaviour
{
    [Header("Transparency")][SerializeField]
    float targetTransparency = 0.3f;
    [SerializeField]
    float fadeInTimeout = 0.2f;
    [SerializeField]
    float fadeOutTimeout = 0.2f;

    [Header("General")][SerializeField]
    bool debug = false;
    [SerializeField]
    Transform player = null;
    [SerializeField]
    CapsuleCollider playerCapsule = null;

    CinemachineFramingTransposer transposer = null;
    float distanceToPlayer = 5.0f;

    private void Start()
    {
        transposer = GetComponent<CinemachineVirtualCamera>().GetCinemachineComponent<CinemachineFramingTransposer>();
        distanceToPlayer = transposer.m_CameraDistance;
    }

    private void Update()
    {
        Vector3 capsuleTop = transform.position + playerCapsule.center + Vector3.up * playerCapsule.height * 0.5f;
        Vector3 capsuleBottom = transform.position + playerCapsule.center - Vector3.up * playerCapsule.height * 0.5f;
        Vector3 direction = player.position - transform.position;

        if (debug)
        {
            Debug.DrawLine(capsuleTop, capsuleTop + direction, Color.cyan, 0.5f);
            Debug.DrawLine(capsuleBottom, capsuleBottom + direction, Color.cyan, 0.5f);
        }

        RaycastHit[] hits = Physics.CapsuleCastAll(capsuleTop, capsuleBottom, playerCapsule.radius, direction, distanceToPlayer, (1 << LayerMask.NameToLayer("ClearCamera")));
        
        foreach (RaycastHit hit in hits)
        {
            Renderer renderer = hit.collider.GetComponent<Renderer>();
            
            if (!renderer || hit.collider.isTrigger)
                continue;

            // Add the script if not found
            AutoTransparent autoTransparent = renderer.GetComponent<AutoTransparent>();
            if (!autoTransparent)
            {
                autoTransparent = renderer.gameObject.AddComponent<AutoTransparent>();
                autoTransparent.FadeInTimeout = fadeInTimeout;
                autoTransparent.FadeOutTimeout = fadeOutTimeout;
                autoTransparent.TargetTransparency = targetTransparency;
            }

            autoTransparent.BeTransparent();
        }
    }
}