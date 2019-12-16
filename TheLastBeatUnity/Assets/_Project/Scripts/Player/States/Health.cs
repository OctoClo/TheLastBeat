using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Sirenix.OdinInspector;
using System.Linq;
using UnityEditor;

public class Health : Beatable
{
    [SerializeField]
    bool debugMode = false;
    Rect debugWindowRect = new Rect(20, 20, 240, 180 );

    [SerializeField] [TabGroup("Visual")]
    VisualParams visualParams = null;

    [SerializeField] [TabGroup("Gameplay")] 
    float currentPulse = 50;

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

    public Player Player { get; set; }

#if UNITY_EDITOR
    [Button][TabGroup("Zone generation")]
    void Generate()
    {
        PulseZone pz = ScriptableObject.CreateInstance<PulseZone>();
        pz.Length = lengthZone;
        pz.colorRepr = colorZone;
        pz.name = labelZone;
        pz.ScaleModifier = scaleUI;
        allZones.Add(pz);
        AssetDatabase.CreateAsset(pz , path + "/" + labelZone + ".asset");
    }
#endif

    HealthVisual visual;

    protected override void Start()
    {
        base.Start();
        visual = new HealthVisual(visualParams);
        if (CurrentZone)
        {
            OnZoneChanged(CurrentZone);
        }
    }

    PulseZone Sample(float pulseValue)
    {
        if (allZones.Count == 0)
            return null;

        if (pulseValue < 0)
        {
            return allZones[0];
        }

        if (allZones.Sum(x => x.Length) < pulseValue)
        {
            return allZones[allZones.Count() - 1];
        }

        float cursor = 0;
        foreach(PulseZone pz in allZones)
        {
            if (pulseValue <= cursor + pz.Length && pulseValue >= cursor)
            {
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

    public override void Beat()
    {
        BeatSequence();
    }

    public void BeatSequence()
    {
        if (InCriticMode)
            return;

        visual.RegularBeat(CurrentZone);
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
        currentPulse += deltaValue;
        if (!CurrentZone)
        {
            return;
        }
        currentPulse = Mathf.Clamp(currentPulse, minimalPulse, maximalPulse);

        if (CurrentZone != previousZone)
        {
            OnZoneChanged(previousZone);
        }

        if (deltaValue > 0)
        {
            Player.HurtAnimation(0.25f, 3);
        }
    }

    void OnZoneChanged(PulseZone previous)
    {
        if (!CurrentZone)
            return;

        //Exited critical
        if (previous == allZones[allZones.Count - 1])
        {
            outCritic.SetValue();
            visual.ExitCriticState();
        }

        //Below limit
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
            visual.EnterCriticState(CurrentZone);
        }

        visual.SetRiftAnimation(allZones.IndexOf(CurrentZone), allZones.Count);
        visual.TransitionColor(CurrentZone.colorRepr);
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
