using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Sirenix.OdinInspector;
using DG.Tweening;
using TMPro;
using System.Linq;
using UnityEditor;

public class Health : Beatable
{
    [SerializeField]
    bool debugMode = false;
    Rect debugWindowRect = new Rect(20, 20, 240, 180 );

    [SerializeField] [TabGroup("Visual")]
    RectTransform healthBackgroundRect = null;

    [SerializeField] [TabGroup("Gameplay")] 
    float currentPulse = 50;
    Vector3 temporarySize = Vector3.zero;

    [SerializeField] [TabGroup("Visual")]
    Image colorChange = null;

    [SerializeField] [TabGroup("Visual")]
    Animator riftAnimation = null;

    [SerializeField] [TabGroup("Gameplay")] 
    float minimalPulse = 0;

    [SerializeField] [TabGroup("Gameplay")] 
    float maximalPulse = 100;

    [SerializeField][TabGroup("Sound")]
    AK.Wwise.State inLimit = null;

    [SerializeField] [TabGroup("Sound")]
    AK.Wwise.State outLimit = null;

    [SerializeField] [TabGroup("Sound")]
    AK.Wwise.State inCritic = null;

    [SerializeField] [TabGroup("Sound")]
    AK.Wwise.State outCritic = null;

    int currentActionCountdownHealth;

    [SerializeField] [TabGroup("Zone generation")]
    AnimationCurve pulseMultiplier = null;

    [SerializeField] [TabGroup("Zone generation")]
    Color colorZone = Color.black;

    [SerializeField] [TabGroup("Zone generation")]
    float lengthZone = 0;

    [SerializeField] [TabGroup("Zone generation")]
    string labelZone = "";

    [SerializeField]
    List<PulseZone> allZones = new List<PulseZone>();

    [SerializeField][TabGroup("Zone generation")][Range(0,10)]
    float scaleUI = 0;

    [SerializeField][TabGroup("Zone generation")][FolderPath]
    string path = "";

#if UNITY_EDITOR
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

        AssetDatabase.CreateAsset(pz , path + "/" + labelZone + ".asset");
    }
#endif

    Sequence seq = null;
    Sequence berserkSeq = null;

    protected override void Start()
    {
        base.Start();
        Debug.Assert(allZones.Count > 0, "No segment");
        if (CurrentZone)
        {
            OnZoneChanged(CurrentZone);
        }
        temporarySize = healthBackgroundRect.transform.localScale;
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
    public bool InCriticMode => CurrentZone == allZones[allZones.Count - 1];
    float ratioPulse = 0;
    Color colorDuringBerserk;

    public override void Beat()
    {
        BeatSequence();
    }

    public void BeatSequence()
    {
        if (InCriticMode)
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
    }

    void OnZoneChanged(PulseZone previous)
    {
        if (!CurrentZone)
            return;

        if (previous == allZones[allZones.Count - 1])
            outCritic.SetValue();

        if (allZones.FindIndex(x => x == CurrentZone) < allZones.Count - 2)
        {
            outLimit.SetValue();
        }
        else
        {
            if (InCriticMode)
                inCritic.SetValue();
            else
                inLimit.SetValue();
        }

        //Entered berserk mode
        if (InCriticMode)
        {
            healthBackgroundRect.DOScale(temporarySize * CurrentZone.ScaleModifier, 0.1f);
            colorDuringBerserk = CurrentZone.colorRepr;
            berserkSeq = DOTween.Sequence();
            berserkSeq.Append(DOTween.To(() => colorChange.color, x => colorChange.color = x, Color.white, 0.1f));
            berserkSeq.Append(DOTween.To(() => colorChange.color, x => colorChange.color = x, colorDuringBerserk, 0.1f));
            berserkSeq.SetLoops(-1);
            berserkSeq.Play();
            riftAnimation.SetInteger("indexState", 3);
        }

        if (previous == allZones[allZones.Count - 1] && berserkSeq != null)
        {
            berserkSeq.Kill();
        }

        if (allZones.IndexOf(CurrentZone) == allZones.Count - 2)
        {
            riftAnimation.SetInteger("indexState", 2);
        }
        else if (allZones.IndexOf(CurrentZone) == allZones.Count - 3)
        {
            riftAnimation.SetInteger("indexState", 1);
        }
        else
        {
            riftAnimation.SetInteger("indexState", 0);
        }

        TransitionColor(CurrentZone.colorRepr);
    }

    void TransitionColor(Color newColor)
    {
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
