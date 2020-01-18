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
    protected Player player = null;
    protected float currentCooldown = 0;
    protected float cooldown = 0;
    protected float healCorrectBeat = 0;
    InputVisualAnimation visualAnimation = null;

    public Ability(Player newPlayer, float healCorrect)
    {
        player = newPlayer;
        healCorrectBeat = healCorrect;
        visualAnimation = GameObject.FindObjectOfType<InputVisualAnimation>();
    }

    public void CorrectBeat()
    {
        if (visualAnimation)
        {
            visualAnimation.CorrectBeat();
        }
    }

    public void PerfectBeat()
    {
        if (visualAnimation)
        {
            visualAnimation.PerfectBeat();
        }
    }

    public void WrongBeat()
    {
        if (visualAnimation)
        {
            visualAnimation.WrongBeat();
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
