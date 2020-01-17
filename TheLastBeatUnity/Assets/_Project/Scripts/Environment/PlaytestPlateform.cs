using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlaytestPlateform : MonoBehaviour
{
    public GameObject groupTrigger;
    MeshRenderer platMesh;
    int nbLeft;

    // Start is called before the first frame update
    void Start()
    {
        platMesh = GetComponent<MeshRenderer>();
        nbLeft = groupTrigger.transform.childCount;
    }

    // Update is called once per frame
    void Update()
    {
        if (groupTrigger.transform.childCount == 0)
        {
            platMesh.enabled = true;
        }
    }
}
