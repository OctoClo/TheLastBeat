using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Sirenix.OdinInspector;
using DG.Tweening;
using TMPro;

public class Health : MonoBehaviour
{
    #region properties
    [SerializeField]
    bool debugMode;
    Rect debugWindowRect = new Rect(20, 20, 120, 50);

    [TabGroup("Visual")] [SerializeField]
    Image healthBackground;
    RectTransform healthBackgroundRect;
    [TabGroup("Visual")] [SerializeField]
    TextMeshProUGUI healthText;

    [TabGroup("Visual")] [SerializeField] [Range(1, 5)]
    int healthBackgroundNewScale;
    float healthBackgroundCurrentScale; 

    [TabGroup("Gameplay")] [SerializeField]
    int startingFrequency;
    [TabGroup("Gameplay")] [SerializeField]
    int minimalFrequency;
    [TabGroup("Gameplay")] [SerializeField]
    int maximalFrequency;

    [TabGroup("Gameplay")] [SerializeField]
    AnimationCurve hitCurve;

    [TabGroup("Gameplay")] [SerializeField]
    MultiReference referenceMultiply;

    [TabGroup("Gameplay")] [SerializeField] [ValidateInput("Positive", "This value must be > 0")]
    float freezeTime;

    [TabGroup("Gameplay")] [SerializeField] [ValidateInput("Positive", "This value must be > 0")]
    float timeBeforeTachy;
    float currentTimeBeforeTachy;
    bool inTachycardie = false;

    //Time stamp of the last action
    float lastTimeAction;
    float beatsPerMinutes;
    float TimeBetweenBeats => (1 / beatsPerMinutes) * 60;
    float DurationSequence => Mathf.Min(0.1f, TimeBetweenBeats / 2);
    int numberBeat = 200;
    float currentMultiplier = 1;
    float accumulator = 0;
    bool pause;

    IEnumerator healthCoroutine;

    public bool Positive(float value)
    {
        return value > 0;
    }

    public enum MultiReference
    {
        OriginalValue,
        CurrentValue
    }
    #endregion

    private void OnEnable()
    {
        EventManager.Instance.AddListener<PauseEvent>(OnPauseEvent);
    }

    private void OnDisable()
    {
        EventManager.Instance.RemoveListener<PauseEvent>(OnPauseEvent);
    }

    public void Start()
    {
        pause = false;
        healthBackgroundRect = healthBackground.GetComponent<RectTransform>();
        healthBackgroundCurrentScale = healthBackgroundRect.localScale.x;
        beatsPerMinutes = startingFrequency;
        Beat();
    }

    private void Update()
    {
        if (!pause)
        {
            accumulator += Time.deltaTime;
            if (accumulator > TimeBetweenBeats)
            {
                accumulator = 0;
                Beat();
            }
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

    private void OnPauseEvent(PauseEvent e)
    {
        pause = e.pause;
    }

    public void Beat()
    {
        Sequence seqMin = DOTween.Sequence();
        Sequence seqMax = DOTween.Sequence();

        seqMin.Append(healthBackgroundRect.DOScale(healthBackgroundNewScale, DurationSequence));
        seqMin.Append(healthBackgroundRect.DOScale(healthBackgroundCurrentScale, DurationSequence));
        seqMin.Play();

        seqMax.Append(healthBackgroundRect.DOScale(healthBackgroundNewScale, DurationSequence));
        seqMax.Append(healthBackgroundRect.DOScale(healthBackgroundCurrentScale, DurationSequence));
        seqMax.Play();

        numberBeat--;
        healthText.text = numberBeat.ToString();
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
    public void NewAction(float multiplier, float duration)
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
                StartHealthTransition(beatsPerMinutes, CalculateNewValue(multiplier), duration);
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
        Debug.Assert(duration > 0);
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
            healthBackground.color = Color.red;
        }
        else
        {
            healthBackground.color = Color.white;
        }
    }

    public void OnGUI()
    {
        if (debugMode)
        {
            debugWindowRect = GUI.Window(0, debugWindowRect, DisplayWindow, "Debug");
        }
    }

    void DisplayWindow(int id)
    {
        GUI.Label(new Rect(70, 20, 50, 20),Mathf.Round(beatsPerMinutes).ToString());
    }
}
