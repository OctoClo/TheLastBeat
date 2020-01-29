using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyRecordBar : Beatable
{
    Enemy enemy = null;

    protected override void Start()
    {
        enemy = GetComponent<Enemy>();
        base.Start();
    }

    public override void Beat()
    {
        enemy.OnBar();
    }
}
