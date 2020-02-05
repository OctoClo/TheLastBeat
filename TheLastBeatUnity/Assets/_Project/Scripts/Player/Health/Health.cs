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
    Rect debugWindowRect = new Rect(20, 20, 240, 100);

    [SerializeField] [TabGroup("Visual")]
    VisualParams visualParams = null;

    [SerializeField] [TabGroup("Gameplay")] 
    float currentPulse = 50;

    [SerializeField] [TabGroup("Gameplay")] 
    float minimalPulse = 0;

    [SerializeField] [TabGroup("Gameplay")] 
    float maximalPulse = 100;

    [SerializeField] [TabGroup("Sound")]
    AK.Wwise.State inCritic = null;

    [SerializeField] [TabGroup("Sound")]
    AK.Wwise.State outCritic = null;

    public Player Player { get; set; }

    float ratioPulse => 1 - ((currentPulse - minimalPulse) / (maximalPulse - minimalPulse));
    HealthVisual visual;

    protected override void Start()
    {
        base.Start();
        visual = new HealthVisual(visualParams);
        visual.UpdateColor(ratioPulse);
    }

    public bool InCriticMode => ratioPulse == 0;

    public override void Beat()
    {
        BeatSequence();
    }

    public void BeatSequence()
    {
        if (InCriticMode)
        {
            visual.ScreenShake();
            visual.UIScreenShake();
        }

        visual.RegularBeat();
    }

    private void OnGUI()
    {
        if (debugMode)
        {
            debugWindowRect = GUI.Window(0, debugWindowRect, DebugWindow, "Debug");
        }
    }

    public void ModifyPulseValue(float deltaValue)
    {
        //No longer critic mode
        if (ratioPulse == 0 && currentPulse + deltaValue > minimalPulse)
        {
            visual.ExitCriticState();
            outCritic.SetValue();
        }

        //Hurted
        currentPulse += deltaValue;
        currentPulse = Mathf.Clamp(currentPulse, minimalPulse, maximalPulse);

        if (deltaValue > 0)
        {
            visual.ScreenShake();
            Player.HurtAnimation(0.25f, 3);
            visual.HurtAnimationUI();
            visual.UIScreenShake();
        }

        visual.UpdateColor(ratioPulse);

        //Enter critic mode
        if (InCriticMode)
        {
            inCritic.SetValue();
            visual.EnterCriticState();
        }
    }

    void DebugWindow(int windowID)
    {
        if (GUI.Button(new Rect(30,25, 80, 25), "+"))
        {
            ModifyPulseValue(-5);
        }
        
        if (GUI.Button(new Rect(30, 50 , 80 , 25), "-"))
        {
            ModifyPulseValue(5);
        }

        GUI.Label(new Rect(20, 75, 100, 25), ratioPulse.ToString());
    }
}
