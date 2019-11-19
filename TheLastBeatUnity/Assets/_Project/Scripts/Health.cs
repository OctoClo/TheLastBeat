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
    Rect debugWindowRect = new Rect(20, 20, 240, 160 );

    [SerializeField] [TabGroup("Visual")]
    RectTransform healthBackgroundRect;

    [SerializeField] [Range(1, 5)] [TabGroup("Visual")]
    float healthBackgroundNewScale = 0;

    [SerializeField] [TabGroup("Gameplay")] 
    float currentPulse = 50;
    Vector3 temporarySize;

    [SerializeField] [TabGroup("Visual")]
    Image colorChange;

    [SerializeField] [TabGroup("Gameplay")] [InfoBox("Cela inclus l'action qui a fait passé en berserk, n'importe quel valeur en negatif pour infini", InfoMessageType.None)]
    int actionBeforeDeath = 3;

    [SerializeField] [TabGroup("Gameplay")] 
    bool dieAtMissInputBerserk = false;

    [SerializeField] [TabGroup("Gameplay")] 
    float minimalPulse = 0;

    [SerializeField] [TabGroup("Gameplay")] 
    float maximalPulse = 100;

    int currentActionCountdownHealth;

    [SerializeField] [TabGroup("ZoneGeneration")]
    AnimationCurve pulseMultiplier = null;

    [SerializeField] [TabGroup("ZoneGeneration")]
    Color colorZone;

    [SerializeField] [TabGroup("ZoneGeneration")]
    float lengthZone;

    [SerializeField] [TabGroup("ZoneGeneration")]
    string labelZone;

    [SerializeField]
    List<PulseZone> allZones = new List<PulseZone>();

    [Button][TabGroup("ZoneGeneration")]
    void Generate()
    {
        PulseZone pz = ScriptableObject.CreateInstance<PulseZone>();
        pz.Length = lengthZone;
        pz.ModifierInZone = pulseMultiplier;
        pz.colorRepr = colorZone;
        pz.name = labelZone;
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
        OnZoneChanged(CurrentZone);
        temporarySize = healthBackgroundRect.transform.localScale;
    }

    void Die()
    {
        Debug.Log("died");
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
    bool IsLastPulseZone => CurrentZone == allZones[allZones.Count - 1];
    float ratioPulse = 0;

    public override void Beat()
    {
        BeatSequence();
    }

    public void BeatSequence()
    {
        if (IsLastPulseZone)
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
        PulseZone previousZone = CurrentZone;
        currentPulse += previousZone.ModifierInZone.Evaluate(ratioPulse) * deltaValue;
        currentPulse = Mathf.Clamp(currentPulse, minimalPulse, maximalPulse);

        if (CurrentZone != previousZone)
        {
            OnZoneChanged(previousZone);
        }

        if (IsLastPulseZone && countAsAction)
        {
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

    void OnZoneChanged(PulseZone previous)
    {
        TransitionColor(CurrentZone.colorRepr);
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
        GUI.Label(new Rect(20, 95, 100, 25), CurrentZone.name);

        float startX = 20;
        float startY = 130;
        float height = 25;

        float width = 200;
        float totalPulse = allZones.Sum(x => x.Length);
        float cursor = 0;

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
