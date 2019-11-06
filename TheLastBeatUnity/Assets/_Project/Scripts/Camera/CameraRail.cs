using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraRail : MonoBehaviour
{
    public void Start()
    {
        StartCoroutine(Translate(Vector3.up * Time.deltaTime));
    }

    IEnumerator Translate(Vector3 toMove)
    {
        while (true)
        {
            transform.Translate(toMove);
            transform.Rotate(new Vector3(toMove.y , -toMove.x, 0), Space.Self);
            yield return null;
        }
    }
}
