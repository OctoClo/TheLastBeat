using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SlopeAdaptation : MonoBehaviour
{
    [SerializeField]
    Vector3 offsetRaycast;

    [SerializeField]
    float offsetY = 0;

    [SerializeField]
    bool adaptOnStart = false;

    Mesh mesh;

    private void Start()
    {
        if (adaptOnStart)
            Adapt();
    }

    public void Adapt()
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
            for (int i = 0; i < 11; i++)
            {
                if (gameObject.name.Contains("Line"))
                    Debug.Log(offsetY);

                Vector3 middlePoint = transform.TransformPoint(verts[(i * 11) + j]) + offsetRaycast;
                Ray ray = new Ray(middlePoint, Vector3.down);

                foreach (RaycastHit hit in Physics.RaycastAll(ray, 10))
                {
                    if (hit.collider.gameObject != gameObject)
                    {
                        Vector3 worldPosition = hit.point + (Vector3.up * offsetY);
                        verts[(i * 11) + j] = transform.InverseTransformPoint(worldPosition);
                        break;
                    }
                }
            }
        }

        mesh.vertices = verts;
        Destroy(this);
    }
}
