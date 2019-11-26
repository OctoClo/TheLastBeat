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
    [HideInInspector]
    public Player AttachedPlayer;
}

public abstract class Ability
{
    protected Player player;

    public Ability(Player newPlayer)
    {
        player = newPlayer;
    }

    public virtual void Launch() { }

    public virtual void End() { }

    public virtual void Update(float deltaTime) { }
}
