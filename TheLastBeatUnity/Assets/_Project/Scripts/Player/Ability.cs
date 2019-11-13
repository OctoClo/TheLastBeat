using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum EInputAction
{
    BLINK,
    RUSH,
    REWINDRUSH
}

public abstract class Ability
{
    protected Player player;

    public Ability(Player newPlayer)
    {
        player = newPlayer;
    }

    public abstract void Launch();

    public abstract void Update(float deltaTime);
}
