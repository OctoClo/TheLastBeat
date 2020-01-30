using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
using System.Linq;
using UnityEditor;

public class SpriteMeshExtruder : MonoBehaviour
{
    [SerializeField]
    Vector3 extrudeDirection = Vector3.forward;

    [SerializeField]
    [FolderPath(RequireExistingPath = true, ParentFolder = "Assets")]
    string resultAssetPath = "";

    [SerializeField]
    string resultAssetName = "Default";

#if UNITY_EDITOR
    [Button(ButtonSizes.Gigantic)]
    public void GenerateAsset()
    {
        PolygonCollider2D polygon;
        if (GetComponent<PolygonCollider2D>())
        {
            polygon = GetComponent<PolygonCollider2D>();
        }
        else
        {
            polygon = gameObject.AddComponent<PolygonCollider2D>();
        }

        //Get vertices
        Mesh mesh = new Mesh();
        mesh.name = "Generated";

        List<Vector3> verticesFront = new List<Vector3>();
        List<Vector3> verticesBack = new List<Vector3>();

        foreach (Vector2 positionPoint in GetComponent<PolygonCollider2D>().points)
        {
            Vector3 vertex = transform.localToWorldMatrix.MultiplyPoint(new Vector3(positionPoint.x, positionPoint.y, 0));
            Vector3 vertexBack = vertex + extrudeDirection;
            verticesFront.Add(vertex);
            verticesBack.Add(vertexBack);
        }

        int offsetExtrude = verticesFront.Count();

        verticesFront.InsertRange(verticesFront.Count(), verticesBack);
        Vector3[] vertices = verticesFront.ToArray();

        List<int> indices = new List<int>();
        for (int i = 0; i < offsetExtrude; i++)
        {
            indices.Add(i);
            indices.Add((i + 1) % offsetExtrude);
            indices.Add(i + offsetExtrude);

            indices.Add((i + 1) % offsetExtrude);
            indices.Add(((i + 1) % offsetExtrude) + offsetExtrude);
            indices.Add(i + offsetExtrude);
        }

        for (int i = 2; i < offsetExtrude; i++)
        {
            indices.Add(0);
            indices.Add(i);
            indices.Add(i - 1);

            indices.Add(offsetExtrude);
            indices.Add((i - 1) + offsetExtrude);
            indices.Add(i + offsetExtrude);
        }

        mesh.vertices = vertices;
        mesh.triangles = indices.ToArray();

        GameObject gob = new GameObject();
        gob.AddComponent<MeshCollider>().convex = true;
        transform.parent = gob.transform;

        DestroyImmediate(GetComponent<PolygonCollider2D>());
        DestroyImmediate(this);

        if (resultAssetName == "")
        {
            resultAssetName = "Default";
        }

        AssetDatabase.CreateAsset(mesh, "Assets/" + resultAssetPath + "/" + "Mesh_" + resultAssetName + ".asset");
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        string localPath = "Assets/" + resultAssetPath + "/" + resultAssetName + ".prefab";

        // Make sure the file name is unique, in case an existing Prefab has the same name.
        localPath = AssetDatabase.GenerateUniqueAssetPath(localPath);

        // Create the new Prefab.
        PrefabUtility.SaveAsPrefabAsset(gob, localPath);
        Selection.objects = new Object[] { AssetDatabase.LoadMainAssetAtPath(localPath), AssetDatabase.LoadMainAssetAtPath("Assets/" + resultAssetPath + "/" + resultAssetName + ".asset") };
    }
#endif
}
