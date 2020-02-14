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

    [SerializeField] [TabGroup("Sound")]
    AK.Wwise.Event healSound = null;

    [HideInInspector]
    public bool Dying = false;

    public Player Player { get; set; }
    public int HPLeft => (int)maximalPulse - (int)currentPulse;

    float ratioPulse => 1 - ((currentPulse - minimalPulse) / (maximalPulse - minimalPulse));
    HealthVisual visual;

    protected override void Start()
    {
        base.Start();
        visual = new HealthVisual(visualParams);
        visual.UpdateColor(HPLeft);
        visual.UpdateContainer(HPLeft);
    }

    public bool InCriticMode => ratioPulse == 0;

    public override void Beat()
    {
        BeatSequence();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.O))
        {
            ModifyPulseValue(-1, false);
        }
        if (Input.GetKeyDown(KeyCode.L))
        {
            ModifyPulseValue(1, false);
        }
    }

    public void BeatSequence()
    {
        if (InCriticMode && !Dying)
        {
            visual.ScreenShake();
            visual.UIScreenShake();
            visual.RestartScreeningSeq();
        }
    }

    private void OnGUI()
    {
        if (debugMode)
        {
            debugWindowRect = GUI.Window(0, debugWindowRect, DebugWindow, "Debug");
        }
    }

    public void GetStunned()
    {
        visual.LaunchScreeningShield();
    }

    public void ModifyPulseValue(float deltaValue, bool fromEnemy)
    {
        //HealSound
        if (deltaValue < 0 && ratioPulse != 1.0f)
        {
            healSound.Post(gameObject);
        }
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
            visual.HurtAnimationUI(fromEnemy);
            visual.UIScreenShake();
        }
        else
        {
            visual.LaunchScreeningHeal();
        }

        visual.UpdateColor(HPLeft);
        visual.UpdateContainer(HPLeft);

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
            ModifyPulseValue(-1,false);
        }
        
        if (GUI.Button(new Rect(30, 50 , 80 , 25), "-"))
        {
            ModifyPulseValue(1, false);
        }

        GUI.Label(new Rect(20, 75, 100, 25), ratioPulse.ToString());
    }
}
