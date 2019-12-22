using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class Test : MonoBehaviour
{
    Sequence seq; 
    // Start is called before the first frame update
    void Start()
    {
        DOTween.Init();
        seq = DOTween.Sequence();
        seq.Append(transform.DOMove(Vector3.one, 1));
        seq.Append(transform.DOMove(Vector3.zero * 100, 1));
        seq.SetLoops(-1);
        StartCoroutine(TestCor());
    }

    IEnumerator TestCor()
    {
        yield return new WaitForSeconds(Random.Range(0, 1.9f));
        SceneHelper.Instance.ComputeTimeScale(seq, 0.25f);
    }
}
