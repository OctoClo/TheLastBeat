using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TimeManager
{
    Player player;
    List<Enemy> enemies = new List<Enemy>();

    public float CurrentTimeScale = 1;

    public void SetPlayer(Player newPlayer)
    {
        player = newPlayer;
    }

    public void AddEnemy(Enemy newEnemy)
    {
        enemies.Add(newEnemy);
    }

    static TimeManager timeManager = null;
    public static TimeManager Instance
    {
        get
        {
            if (timeManager == null)
                timeManager = new TimeManager();
            return timeManager;
        }
    }

    public void SetTimeScale(float timeScale)
    {
        CurrentTimeScale = timeScale;
        Time.timeScale = CurrentTimeScale;
    }

    public void SlowEnemies()
    {
        foreach (Enemy enemy in enemies)
            enemy.Slow();
    }

    public void ResetEnemies()
    {
        foreach (Enemy enemy in enemies)
            enemy.ResetSpeed();
    }
}
