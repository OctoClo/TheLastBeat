using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SlopeAdaptation : MonoBehaviour
{
    [SerializeField]
    Vector3 offsetRaycast = Vector3.zero;

    [SerializeField]
    float offsetY = 0;

    [SerializeField]
    bool adaptOnStart = false;

    [SerializeField]
    bool adaptOnUpdate = false;

    [SerializeField]
    float rayLength = 10;

    Mesh mesh;

    private void Start()
    {
        if (adaptOnStart)
            Adapt();
    }

    private void Update()
    {
        if (adaptOnUpdate && gameObject.activeSelf)
            Adapt(false);
    }

    public void Adapt(bool destroyEnd = true)
    {
        mesh = GetComponent<MeshFilter>().mesh;
        if (mesh.vertices.Length != 121)
        {
            throw new System.Exception("Not a plane mesh");
        }

        Vector3[] verts = mesh.vertices;
        for (int j = 0; j < 11; j++)
        {
            for (int i = 0; i < 11; i++)
            {
                Vector3 middlePoint = transform.TransformPoint(verts[(i * 11) + j]);
                middlePoint = new Vector3(middlePoint.x, transform.position.y, middlePoint.z) + offsetRaycast;
                Ray ray = new Ray(middlePoint, Vector3.down);
                Vector3 origin = verts[(i * 11) + j];

                foreach (RaycastHit hit in Physics.RaycastAll(ray, rayLength))
                {
                    if (hit.collider.gameObject.CompareTag("Slope"))  
                    {
                        Vector3 worldPosition = hit.point + (Vector3.up * offsetY);
                        verts[(i * 11) + j] = transform.InverseTransformPoint(worldPosition);
                        verts[(i * 11) + j] = new Vector3(origin.x, verts[(i * 11) + j].y, origin.z);
                        break;
                    }
                }
            }
        }

        mesh.vertices = verts;
        if (destroyEnd)
            Destroy(this);
    }
}
