using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class Test : Slowable
{ 
    Sequence seq;
    // Start is called before the first frame update
    void Start()
    {
        DOTween.Init();
        seq = BuildSequence();
        seq.Append(transform.DOScaleX(10, 0.5f));
        seq.Append(transform.DOScaleX(5, 0.5f));
        seq.SetLoops(-1);
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            PersonalTimeScale = 0.4f;
        }
    }
}
