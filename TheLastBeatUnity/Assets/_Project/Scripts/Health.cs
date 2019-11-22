using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Sirenix.OdinInspector;
using DG.Tweening;
using TMPro;
using System.Linq;

public class Health : Beatable
{
    [SerializeField]
    bool debugMode = false;
    Rect debugWindowRect = new Rect(20, 20, 240, 180 );

    [SerializeField] [TabGroup("Visual")]
    RectTransform healthBackgroundRect;

    [SerializeField] [Range(1, 5)] [TabGroup("Visual")]
    float healthBackgroundNewScale = 0;

    [SerializeField] [TabGroup("Gameplay")] 
    float currentPulse = 50;
    Vector3 temporarySize;

    [SerializeField] [TabGroup("Visual")]
    Image colorChange;

    [SerializeField] [TabGroup("Visual")]
    AK.Wwise.State inLimit;

    [SerializeField] [TabGroup("Visual")]
    AK.Wwise.State outLimit;

    [SerializeField] [TabGroup("Gameplay")] [InfoBox("Cela inclus l'action qui a fait passé en berserk, n'importe quel valeur en negatif pour infini", InfoMessageType.None)]
    int actionBeforeDeath = 3;

    [SerializeField] [TabGroup("Gameplay")] 
    bool dieAtMissInputBerserk = false;

    [SerializeField] [TabGroup("Gameplay")] 
    float minimalPulse = 0;

    [SerializeField] [TabGroup("Gameplay")] 
    float maximalPulse = 100;

    int currentActionCountdownHealth;
    bool died = false;

    [SerializeField] [TabGroup("Zone generation")]
    AnimationCurve pulseMultiplier = null;

    [SerializeField] [TabGroup("Zone generation")]
    Color colorZone;

    [SerializeField] [TabGroup("Zone generation")]
    float lengthZone;

    [SerializeField] [TabGroup("Zone generation")]
    string labelZone;

    [SerializeField]
    List<PulseZone> allZones = new List<PulseZone>();

    [SerializeField][TabGroup("Zone generation")][Range(0,10)]
    float scaleUI;

    [Button][TabGroup("Zone generation")]
    void Generate()
    {
        PulseZone pz = ScriptableObject.CreateInstance<PulseZone>();
        pz.Length = lengthZone;
        pz.ModifierInZone = pulseMultiplier;
        pz.colorRepr = colorZone;
        pz.name = labelZone;
        pz.ScaleModifier = scaleUI;
        allZones.Add(pz);
    }

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
        Debug.Assert(allZones.Count > 0, "No segment");
        if (CurrentZone)
        {
            OnZoneChanged(CurrentZone);
        }
        temporarySize = healthBackgroundRect.transform.localScale;
    }

    void Die()
    {
        died = true;
    }

    PulseZone Sample(float pulseValue)
    {
        if (allZones.Count == 0)
            return null;

        if (pulseValue < 0)
        {
            ratioPulse = 0;
            return allZones[0];
        }

        if (allZones.Sum(x => x.Length) < pulseValue)
        {
            ratioPulse = 1;
            return allZones[allZones.Count() - 1];
        }

        float cursor = 0;
        foreach(PulseZone pz in allZones)
        {
            if (pulseValue <= cursor + pz.Length && pulseValue >= cursor)
            {
                ratioPulse = (pulseValue - cursor) / (pz.Length);
                return pz;
            }
            else
            {
                cursor += pz.Length;
            }
        }
        return null;
    }

    PulseZone CurrentZone => Sample(currentPulse);
    bool IsBerserkZone => CurrentZone == allZones[allZones.Count - 1];
    float ratioPulse = 0;
    Color colorDuringBerserk;

    public override void Beat()
    {
        BeatSequence();
    }

    public void BeatSequence()
    {
        if (!CurrentZone || IsBerserkZone)
            return;

        seq = DOTween.Sequence();
        seq.Append(healthBackgroundRect.DOScale(temporarySize * CurrentZone.ScaleModifier, sequenceDuration));
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
        PulseZone previousZone = CurrentZone;
        if (!CurrentZone)
        {
            currentPulse += deltaValue;
            return;
        }
            
        currentPulse += previousZone.ModifierInZone.Evaluate(ratioPulse) * deltaValue;
        currentPulse = Mathf.Clamp(currentPulse, minimalPulse, maximalPulse);

        if (CurrentZone != previousZone)
        {
            OnZoneChanged(previousZone);
        }

        if (IsBerserkZone && countAsAction)
        {
            //Not in rythm
            if (BeatManager.Instance.IsInRythm(TimeManager.Instance.SampleCurrentTime() , BeatManager.TypeBeat.BEAT))
            {
                currentActionCountdownHealth--;
                if (currentActionCountdownHealth == 0)
                {
                    Die();
                }
            }
            else
            {
                if (dieAtMissInputBerserk)
                    Die();
            }
        }
    }

    public override void MissedBeat()
    {
        if (IsBerserkZone && dieAtMissInputBerserk)
        {
            Die();
        }
    }

    void OnZoneChanged(PulseZone previous)
    {
        if (!CurrentZone)
            return;

        if (allZones.FindIndex(x => x == CurrentZone) < allZones.Count - 2)
        {
            outLimit.SetValue();
        }
        else
        {
            inLimit.SetValue();
        }

        //Entered berserk mode
        if (IsBerserkZone)
        {
            healthBackgroundRect.DOScale(temporarySize * CurrentZone.ScaleModifier, 0.1f);
            colorDuringBerserk = CurrentZone.colorRepr;
            berserkSeq = DOTween.Sequence();
            berserkSeq.Append(DOTween.To(() => colorChange.color, x => colorChange.color = x, Color.white, 0.1f));
            berserkSeq.Append(DOTween.To(() => colorChange.color, x => colorChange.color = x, colorDuringBerserk, 0.1f));
            berserkSeq.SetLoops(-1);
            berserkSeq.Play();
        }

        if (previous == allZones[allZones.Count - 1] && berserkSeq != null)
        {
            berserkSeq.Kill();
        }

        TransitionColor(CurrentZone.colorRepr);
    }

    void TransitionColor(Color newColor)
    {
        if (colorseq != null && colorseq.IsPlaying())
            colorseq.Kill();

        DOTween.To(() => colorChange.color, x => colorChange.color = x, newColor, 0.5f);
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

        if (CurrentZone)
            GUI.Label(new Rect(20, 95, 100, 25), CurrentZone.name);

        if (died)
            GUI.Label(new Rect(20, 160, 100, 25), "Died");

        float startX = 20;
        float startY = 130;
        float height = 25;

        float width = 200;
        float totalPulse = allZones.Sum(x => x.Length);
        float cursor = 0;

        //Draw rectangles with width relative to the sum of their value
        foreach(PulseZone pz in allZones)
        {
            float beginX = ((cursor / totalPulse) * width) + startX;
            float endX = (((cursor + pz.Length) / totalPulse) * width) + startX;
            DrawQuad(new Rect(beginX, startY, endX - beginX, height), pz.colorRepr);
            cursor += pz.Length;
        }

        float xRatio = Mathf.Clamp(currentPulse / totalPulse, 0,1) * width + startX;
        DrawQuad(new Rect(xRatio, startY, 0.1f, height), Color.white);
    }

    void DrawQuad(Rect position, Color color)
    {
        Texture2D texture = new Texture2D(1, 1);
        texture.SetPixel(0, 0, color);
        texture.Apply();
        GUI.skin.box.normal.background = texture;
        GUI.Box(position, GUIContent.none);
    }
}
