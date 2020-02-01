using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyRecordBeat : Beatable
{
    Enemy enemy = null;

    protected override void Start()
    {
        enemy = GetComponent<Enemy>();
        base.Start();
    }

    public override void Beat()
    {
        enemy.OnBeat();
    }
}
