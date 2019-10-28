using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using Sirenix.OdinInspector;

public class Health : MonoBehaviour
{
    float TimeBetweenBeats => (1 / beatsPerMinutes) * 60;
    int numberBeat = 200;

    Vector2 anchorMin;
    Vector2 anchorMax;

    [TabGroup("Visual")]
    [SerializeField]
    Vector2 newAnchorMin;

    [TabGroup("Visual")]
    [SerializeField]
    Vector2 newAnchorMax;

    [TabGroup("Visual")]
    [SerializeField]
    Image img;

    [TabGroup("Visual")]
    [SerializeField]
    Text txt;

    [TabGroup("Visual")]
    [SerializeField]
    Text debugTxt;

    [TabGroup("Gameplay")]
    [SerializeField]
    int startingFrequency;

    [TabGroup("Gameplay")]
    [SerializeField]
    int minimalFrequency;

    [TabGroup("Gameplay")]
    [SerializeField]
    int maximalFrequency;

    [TabGroup("Gameplay")]
    [SerializeField]
    AnimationCurve hitCurve;

    [TabGroup("Gameplay")]
    [SerializeField]
    MultiReference referenceMultiply;

    float beatsPerMinutes;
    float accumulator = 0;

    float currentMultiplier = 1;

    [TabGroup("Gameplay")]
    [SerializeField]
    float freezeTime;
    float lastTimeAction;

    IEnumerator healthCoroutine;

    public enum MultiReference
    {
        OriginalValue,
        CurrentValue
    }

    public void Start()
    {
        anchorMin = img.GetComponent<RectTransform>().anchorMin;
        anchorMax = img.GetComponent<RectTransform>().anchorMax;
        beatsPerMinutes = startingFrequency;

        //Just for proto
        DOTween.Init();
    }

    private void Update()
    {
        accumulator += Time.deltaTime;
        if(accumulator > TimeBetweenBeats)
        {
            accumulator = 0;
            Beat();
        }

        if (debugTxt)
            debugTxt.text = Mathf.Round(beatsPerMinutes).ToString();

        img.color = (beatsPerMinutes > maximalFrequency ? Color.red : Color.white);
    }

    void Beat()
    {
        Sequence seqMin = DOTween.Sequence();
        Sequence seqMax = DOTween.Sequence();

        //Avoid to have long animation at low beat rate
        float durationSequence = Mathf.Min(0.1f, TimeBetweenBeats / 2);

        seqMin.Append(img.GetComponent<RectTransform>().DOAnchorMin(newAnchorMin, durationSequence));
        seqMin.Append(img.GetComponent<RectTransform>().DOAnchorMin(anchorMin, durationSequence));
        seqMin.Play();

        seqMax.Append(img.GetComponent<RectTransform>().DOAnchorMax(newAnchorMax, durationSequence));
        seqMax.Append(img.GetComponent<RectTransform>().DOAnchorMax(anchorMax, durationSequence));
        seqMax.Play();

        numberBeat--;
        txt.text = numberBeat.ToString();
    }



    public void MultiplyBeat(float value)
    {
        beatsPerMinutes = value * (referenceMultiply == MultiReference.OriginalValue ? startingFrequency : beatsPerMinutes);
        CheckNewBeats();
    }

    void CheckNewBeats()
    {
        beatsPerMinutes = Mathf.Max(minimalFrequency, beatsPerMinutes);
    }

    public void Hit(float damage, float duration , bool multiply = true)
    {
        float newValue;
        if (multiply)
            newValue = beatsPerMinutes * damage;
        else
            newValue = beatsPerMinutes + damage;

        SetHealth(beatsPerMinutes, newValue, duration);
    }

    public void NewAction(float multiplier)
    {
        if (multiplier > currentMultiplier)
        {
            currentMultiplier = multiplier;
            lastTimeAction = Time.timeSinceLevelLoad;
            SetHealth(beatsPerMinutes, startingFrequency * multiplier, 3);
        }
        else if ((int)multiplier == (int)currentMultiplier)
        {
            lastTimeAction = Time.timeSinceLevelLoad;
        }
        else
        {
            beatsPerMinutes = startingFrequency * multiplier;

            //Freeze time over , can be less
            if (Time.timeSinceLevelLoad - lastTimeAction > freezeTime)
            {
                lastTimeAction = Time.timeSinceLevelLoad;
                SetHealth(beatsPerMinutes, startingFrequency * multiplier, 3);
            }
        }
    }

    IEnumerator SetHealthCoroutine(float from, float to, float duration)
    {
        float normalizedTime = 0;
        while (normalizedTime < 1)
        {
            normalizedTime += Time.deltaTime / duration;
            beatsPerMinutes = Mathf.Lerp(from, to, hitCurve.Evaluate(normalizedTime));
            CheckNewBeats();
            yield return null;
        }
        healthCoroutine = null;
    }

    public void SetHealth(float from , float to, float duration)
    {
        if (healthCoroutine != null)
        {
            StopCoroutine(healthCoroutine);
        }
        healthCoroutine = SetHealthCoroutine(from, to, duration);
        StartCoroutine(healthCoroutine);
    }
}
