using UnityEngine;
using Cinemachine;

public class CameraClearSight : MonoBehaviour
{
    [Header("Transparency settings")] [SerializeField]
    bool debug = false;
    [SerializeField]
    Transform player = null;
    CapsuleCollider playerCapsule = null;
    [SerializeField]
    Material transparentMaterial = null;
    [SerializeField]
    float fadeInTimeout = 0.6f;
    [SerializeField]
    float fadeOutTimeout = 0.2f;
    [SerializeField]
    float targetTransparency = 0.3f;
    

    CinemachineFramingTransposer transposer = null;
    float distanceToPlayer = 5.0f;
    
    private void Start()
    {
        transposer = GetComponent<CinemachineVirtualCamera>().GetCinemachineComponent<CinemachineFramingTransposer>();
        distanceToPlayer = transposer.m_CameraDistance;
        playerCapsule = player.gameObject.GetComponentInChildren<CapsuleCollider>();
    }

    private void Update()
    {
        RaycastHit[] hits;
        Vector3 capsuleTop = transform.position + Vector3.up * playerCapsule.radius;
        Vector3 capsuleBottom = transform.position - Vector3.up * playerCapsule.radius;
        Vector3 direction = player.position - transform.position;
        
        if (debug)
        {
            Debug.DrawLine(capsuleTop, capsuleTop + direction, Color.cyan, 0.5f);
            Debug.DrawLine(capsuleBottom, capsuleBottom + direction, Color.cyan, 0.5f);
        }
        
        hits = Physics.CapsuleCastAll(capsuleTop, capsuleBottom, playerCapsule.radius, direction, distanceToPlayer);
        
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
                autoTransparent.TransparentMaterial = transparentMaterial;
                autoTransparent.FadeInTimeout = fadeInTimeout;
                autoTransparent.FadeOutTimeout = fadeOutTimeout;
                autoTransparent.TargetTransparency = targetTransparency;
            }
            
            autoTransparent.BeTransparent();
        }
    }
}