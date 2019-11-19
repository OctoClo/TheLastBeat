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
    Rect debugWindowRect = new Rect(20, 20, 120, 120 );

    [SerializeField] [TabGroup("Visual")]
    RectTransform healthBackgroundRect;

    [SerializeField] [Range(1, 5)] [TabGroup("Visual")]
    float healthBackgroundNewScale = 0;

    float currentPulse = 50;
    Vector3 temporarySize;

    [SerializeField] [TabGroup("Gameplay")]
    float minimalPulse = 20;

    [SerializeField] [TabGroup("Gameplay")]
    float lowLimitPulse = 60;

    [SerializeField] [TabGroup("Gameplay")]
    float highLimitPulse = 80;

    [SerializeField]
    [TabGroup("Gameplay")]
    float fakeLimitPulse = 100;

    [SerializeField] [TabGroup("Visual")]
    Image colorChange;

    [SerializeField] [TabGroup("Gameplay")]
    AnimationCurve inLimitModifierPulse = null;

    [SerializeField] [TabGroup("Visual")]
    Color calmPulseColor;

    [SerializeField] [TabGroup("Visual")]
    Color lowPulseColor;

    [SerializeField] [TabGroup("Visual")]
    Color highPulseColor;

    [SerializeField] [TabGroup("Visual")]
    Color inLimitColor;

    [SerializeField] [TabGroup("Visual")]
    Color berserkColor;

    [SerializeField] [TabGroup("Gameplay")] [InfoBox("Cela inclus l'action qui a fait passé en berserk", InfoMessageType.Warning)]
    int actionBeforeDeath = 3;
    int currentActionCountdownHealth;

    Sequence seq;
    Sequence colorseq;
    Sequence berserkSeq;

    CombatState currentState = CombatState.InCombat;

    enum CombatState
    {
        InCombat,
        OutOfCombat
    }

    public void Start()
    {
        OnPulseStateChanged(CurrentState);
        temporarySize = healthBackgroundRect.transform.localScale;
        Debug.Assert(minimalPulse < lowLimitPulse && lowLimitPulse < highLimitPulse && highLimitPulse < fakeLimitPulse, "limit pulse not sorted");
    }

    void Die()
    {
        Debug.Log("died");
    }

    PulseState CurrentState
    {
        get
        {
            if (currentPulse <= minimalPulse)
                return PulseState.Calm;

            if (currentPulse < lowLimitPulse)
                return PulseState.Low;

            if (currentPulse < highLimitPulse)
                return PulseState.High;

            if (currentPulse < fakeLimitPulse)
                return PulseState.InLimit;

            return PulseState.Berserk;
        }
    }

    enum PulseState
    {
        Calm,
        Low,
        High,
        InLimit,
        Berserk
    }

    public override void Beat()
    {
        BeatSequence();
    }

    public void BeatSequence()
    {
        if (CurrentState == PulseState.Berserk)
            return;

        seq = DOTween.Sequence();
        seq.Append(healthBackgroundRect.DOScale(temporarySize * healthBackgroundNewScale, sequenceDuration));
        seq.Append(healthBackgroundRect.DOScale(temporarySize, sequenceDuration));
        seq.Play();
    }

    private void OnGUI()
    {
        if (debugMode)
        {
            debugWindowRect = GUI.Window(0, debugWindowRect, DebugWindow, "Debug");
        }
    }

    public void ModifyPulseValue(float deltaValue, bool countAsAction = true)
    {
        PulseState previousState = CurrentState;
        if (CurrentState == PulseState.InLimit)
        {
            float ratio = (currentPulse - highLimitPulse) / (fakeLimitPulse - highLimitPulse);
            currentPulse += inLimitModifierPulse.Evaluate(ratio) * deltaValue;
        }
        else
        {
            currentPulse += deltaValue;
        }    
        
        if (CurrentState != previousState)
        {
            OnPulseStateChanged(previousState);
        }

        if (CurrentState == PulseState.Berserk && countAsAction)
        {
            currentActionCountdownHealth--;
            if (currentActionCountdownHealth <= 0)
            {
                Die();
            }
        }
    }

    void OnPulseStateChanged(PulseState previous)
    {
        if (previous == PulseState.Berserk && berserkSeq != null)
        {
            healthBackgroundRect.DOScale(temporarySize * 3, 0.1f);
            berserkSeq.Kill();
        }

        switch (CurrentState)
        {
            case PulseState.Calm:
                TransitionColor(calmPulseColor);
                break;

            case PulseState.Low:
                TransitionColor(lowPulseColor);
                break;

            case PulseState.High:
                TransitionColor(highPulseColor);
                break;

            case PulseState.InLimit:
                TransitionColor(inLimitColor);
                break;

            case PulseState.Berserk:
                currentActionCountdownHealth = actionBeforeDeath;
                if (berserkSeq != null && berserkSeq.IsPlaying())
                    berserkSeq.Kill();

                healthBackgroundRect.DOScale(temporarySize * 3, sequenceDuration);

                berserkSeq = DOTween.Sequence();
                berserkSeq.Append(DOTween.To(() => colorChange.color, x => colorChange.color = x, berserkColor, 0.3f));
                berserkSeq.Append(DOTween.To(() => colorChange.color, x => colorChange.color = x, Color.white, 0.3f));
                berserkSeq.SetLoops(-1);
                berserkSeq.Play();

                TransitionColor(berserkColor);
                break;
        }
    }

    void TransitionColor(Color newColor)
    {
        if (colorseq != null && colorseq.IsPlaying())
            colorseq.Kill();

        colorseq = DOTween.Sequence();
        colorseq.Append(DOTween.To(() => colorChange.color, x => colorChange.color = x, newColor, 0.5f));
    }

    void DebugWindow(int windowID)
    {
        if (GUI.Button(new Rect(30,25, 80, 25), "+"))
        {
            ModifyPulseValue(5, false);
        }
        
        if (GUI.Button(new Rect(30, 50 , 80 , 25), "-"))
        {
            ModifyPulseValue(-5, false);
        }

        GUI.Label(new Rect(20, 75, 100, 25), currentPulse.ToString());
        GUI.Label(new Rect(20, 95, 100, 25), CurrentState.ToString());
    }
}
