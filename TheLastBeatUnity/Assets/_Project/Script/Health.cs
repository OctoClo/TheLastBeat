using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Sirenix.OdinInspector;
using DG.Tweening;

public class Health : MonoBehaviour
{
    #region properties
    Vector2 anchorMin;
    Vector2 anchorMax;

    [SerializeField]
    bool debugMode;

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

    Rect windowRect = new Rect(20, 20, 120, 50);

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

    [TabGroup("Gameplay")]
    [SerializeField]
    [ValidateInput("Positive", "This value must be > 0")]
    float freezeTime;

    public bool Positive(float value)
    {
        return value > 0;
    }

    //Time stamp of the last action
    float lastTimeAction;
    float beatsPerMinutes;
    float accumulator = 0;

    [TabGroup("Gameplay")]
    [SerializeField]
    [ValidateInput("Positive", "This value must be > 0")]
    float timeBeforeTachy;
    float currentTimeBeforeTachy;
    bool inTachycardie = false;

    float TimeBetweenBeats => (1 / beatsPerMinutes) * 60;
    int numberBeat = 200;
    float currentMultiplier = 1;


    IEnumerator healthCoroutine;

    public enum MultiReference
    {
        OriginalValue,
        CurrentValue
    }
    #endregion

    public void Start()
    {
        anchorMin = img.GetComponent<RectTransform>().anchorMin;
        anchorMax = img.GetComponent<RectTransform>().anchorMax;
        beatsPerMinutes = startingFrequency;
    }

    private void Update()
    {
        accumulator += Time.deltaTime;
        if(accumulator > TimeBetweenBeats)
        {
            accumulator = 0;
            Beat();
        }

        //Heart Beat too high ! staying too long will trigger tachy mode
        if (maximalFrequency < beatsPerMinutes)
        {
            currentTimeBeforeTachy -= Time.deltaTime;
        }
        else
        {
            currentTimeBeforeTachy = timeBeforeTachy;
        }

        //countdown just reached 0 , enter tachy mode
        if (currentTimeBeforeTachy < 0 && ! inTachycardie)
        {
            SetTachycardie(true);
        }
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

    public void Hit(float damage, float duration , bool multiply = true)
    {
        StartHealthTransition(beatsPerMinutes, CalculateNewValue(damage, false), duration);
    }

    public float CalculateNewValue(float value, bool multipy = true)
    {
        float output;
        if (multipy)
            output = (referenceMultiply == MultiReference.OriginalValue ? startingFrequency : beatsPerMinutes) * value;
        else
            output = beatsPerMinutes + value;

        output = Mathf.Max(minimalFrequency, output);
        return output;
    }

    //Add an action to the current combo
    public void NewAction(float multiplier)
    {
        if (multiplier > currentMultiplier)
        {
            currentMultiplier = multiplier;
            lastTimeAction = Time.timeSinceLevelLoad;
            StartHealthTransition(beatsPerMinutes, CalculateNewValue(multiplier), 3);
        }
        else if ((int)multiplier == (int)currentMultiplier)
        {
            lastTimeAction = Time.timeSinceLevelLoad;
        }
        else
        {
            beatsPerMinutes = startingFrequency * multiplier;

            //Freeze time over , heartBeat can down
            if (Time.timeSinceLevelLoad - lastTimeAction > freezeTime)
            {
                lastTimeAction = Time.timeSinceLevelLoad;
                currentMultiplier = multiplier;
                StartHealthTransition(beatsPerMinutes, CalculateNewValue(multiplier), 3);
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
            yield return null;
        }
        healthCoroutine = null;
    }

    void StartHealthTransition(float from , float to, float duration)
    {
        //Only one health animation at the same time
        if (healthCoroutine != null)
        {
            StopCoroutine(healthCoroutine);
        }
        healthCoroutine = SetHealthCoroutine(from, to, duration);
        StartCoroutine(healthCoroutine);
    }

    //What to do when you enter / leave tachycardie ?
    public void SetTachycardie(bool value)
    {
        inTachycardie = value;
        if (value)
        {
            img.color = Color.red;
        }
        else
        {
            img.color = Color.white;
        }
    }

    public void OnGUI()
    {
        if (debugMode)
        {
            windowRect = GUI.Window(0, windowRect, DisplayWindow, "Debug");
        }
    }

    void DisplayWindow(int id)
    {
        GUI.Label(new Rect(10, 20, 100, 20),beatsPerMinutes.ToString());
    }
}
