using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SlopeAdaptation : MonoBehaviour
{
    [SerializeField]
    Vector3 offsetRaycast;

    [SerializeField]
    float offsetY;

    Mesh mesh;
    // Start is called before the first frame update
    void Start()
    {
        mesh = GetComponent<MeshFilter>().mesh;
        if (mesh.vertices.Length != 121)
        {
            throw new System.Exception("Not a plane mesh");
        }

        Vector3 rayPosition = Vector3.zero;

        Vector3[] verts = mesh.vertices;
        for (int j = 0; j < 11; j++)
        {
            Vector3 middlePoint = transform.TransformPoint(verts[(5 * 11) + j] + offsetRaycast);
            float yPos = 0;

            Ray ray = new Ray(middlePoint, Vector3.down);
            foreach(RaycastHit hit in Physics.RaycastAll(ray , 10))
            {
                if (hit.collider.gameObject != gameObject)
                {
                    rayPosition = hit.point;
                    yPos = rayPosition.y;
                    for (int i = 0; i < 11; i++)
                    {
                        Vector3 position = verts[(i * 11) + j];
                        verts[(i * 11) + j] = new Vector3(position.x, yPos + offsetY, position.z);                    
                    }
                    break;
                }
            }
        }

        mesh.vertices = verts;
        Destroy(this);
    }
}
