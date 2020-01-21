using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum EInputAction
{
    BLINK,
    RUSH,
    REWINDRUSH
}

[System.Serializable]
public class AbilityParams
{
    [HideInInspector][Header("Gameplay")]
    public Player AttachedPlayer;
    public float HealPerCorrectBeat;
}

public abstract class Ability
{
    protected Player player;
    protected float currentCooldown = 0;
    protected float cooldown = 0;
    protected float healCorrectBeat = 4;
    InputVisualAnimation visualAnimation;
    BeatAtFeet beatFeet;
    public Ability(Player newPlayer)
    {
        player = newPlayer;
        visualAnimation = GameObject.FindObjectOfType<InputVisualAnimation>();
        beatFeet = GameObject.FindObjectOfType<BeatAtFeet>();
    }

    public void CorrectBeat()
    {
        if (visualAnimation)
        {
            visualAnimation.CorrectBeat();
        }

        if (beatFeet)
        {
            beatFeet.CorrectInput();
        }
    }

    public void PerfectBeat()
    {
        if (visualAnimation)
        {
            visualAnimation.PerfectBeat();
        }

        if (beatFeet)
        {
            beatFeet.PerfectInput();
        }
    }

    public void WrongBeat()
    {
        if (visualAnimation)
        {
            visualAnimation.WrongBeat();
        }

        if (beatFeet)
        {
            beatFeet.WrongInput();
        }
    }
    public virtual void ResetCooldown()
    {
        currentCooldown = 0;
    }

    public virtual void Launch() { }

    public virtual void End() { }

    public virtual void Update(float deltaTime) { }
}
