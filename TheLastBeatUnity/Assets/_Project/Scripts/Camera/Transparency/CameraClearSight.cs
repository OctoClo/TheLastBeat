using UnityEngine;
using Cinemachine;

public class CameraClearSight : MonoBehaviour
{
    [SerializeField]
    Transform player = null;
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
        player = transposer.FollowTarget;

    }

    private void Update()
    {
        Ray ray = new Ray(transform.position, player.transform.position - transform.position);

        RaycastHit[] hits = Physics.RaycastAll(ray, Vector3.Distance(player.position , transform.position));
        
        foreach (RaycastHit hit in hits)
        {
            Renderer renderer = hit.collider.GetComponent<Renderer>();
            
            if (!renderer || hit.collider.isTrigger || hit.transform == player)
                continue;

            renderer.material.SetFloat("_Transparent", 0.2f);
        }
    }
}