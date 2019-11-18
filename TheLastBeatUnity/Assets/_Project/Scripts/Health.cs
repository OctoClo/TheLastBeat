using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Sirenix.OdinInspector;
using DG.Tweening;
using TMPro;

public class Health : Beatable
{
    [SerializeField]
    bool debugMode = false;
    Rect debugWindowRect = new Rect(20, 20, 120, 80);

    [SerializeField]
    Image healthBackground = null;

    [SerializeField]
    RectTransform healthBackgroundRect;

    [SerializeField]
    TextMeshProUGUI healthText = null;

    [SerializeField]
    [Range(1, 5)]
    int healthBackgroundNewScale = 0;
    float healthBackgroundCurrentScale = 0;

    [SerializeField]
    float numberBeat;

    [SerializeField]
    float losePerPulse;

    public override void Beat()
    {
        BeatSequence();
    }

    private void Start()
    {
        healthBackgroundCurrentScale = healthBackgroundRect.transform.localScale.x;
    }
    public void BeatSequence()
    {
        Sequence seqMin = DOTween.Sequence();
        Sequence seqMax = DOTween.Sequence();

        seqMin.Append(healthBackgroundRect.DOScale(healthBackgroundNewScale, sequenceDuration));
        seqMin.Append(healthBackgroundRect.DOScale(healthBackgroundCurrentScale, sequenceDuration));
        seqMin.Play();

        seqMax.Append(healthBackgroundRect.DOScale(healthBackgroundNewScale, sequenceDuration));
        seqMax.Append(healthBackgroundRect.DOScale(healthBackgroundCurrentScale, sequenceDuration));
        seqMax.Play();

        numberBeat -= losePerPulse;
        healthText.text = numberBeat.ToString();
    }

    private void OnGUI()
    {
        if (debugMode)
        {
            debugWindowRect = GUI.Window(0, debugWindowRect, DebugWindow, "Debug");
        }
    }

    void DebugWindow(int windowID)
    {
        if (GUI.Button(new Rect(30,25, 80, 25), "+"))
        {
            losePerPulse++;
        }
        
        if (GUI.Button(new Rect(30, 50 , 80 , 25), "-"))
        {
            losePerPulse--;
        }
    }
}
