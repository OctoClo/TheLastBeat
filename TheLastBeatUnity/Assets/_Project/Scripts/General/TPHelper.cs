using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Rewired;
using Doozy.Engine;

public class TPHelper : MonoBehaviour
{
    [SerializeField]
    private Player player = null;
    private Transform playerTransform = null;
    [SerializeField]
    private Transform[] TPZones = new Transform[6];
    [SerializeField]
    private GameObject[] CurrentZones = new GameObject[6];
    [SerializeField]
    private GameObject[] ZonePrefabsToSpawn = new GameObject[6];

    private Vector3[] ZonePositions = new Vector3[6];

    bool TPMenu = false;

    private void OnEnable()
    {
        Message.AddListener<GameEventMessage>(OnMessage);
    }

    private void OnDisable()
    {
        Message.RemoveListener<GameEventMessage>(OnMessage);
    }

    private void Start()
    {
        playerTransform = player.transform;

        for (int i = 0; i < ZonePositions.Length; i++)
        {
            ZonePositions[i] = CurrentZones[i].transform.position;
        }
    }

    private void OnMessage(GameEventMessage message)
    {
        if (message == null)
            return;

        if (message.EventName.Substring(0, 6).Equals("TPZone"))
        {
            int zoneNb = (int)System.Char.GetNumericValue(message.EventName[6]);
            zoneNb--;
            TPPlayer(TPZones[zoneNb]);
            DeactivateZonesExcept(zoneNb);
        }
    }

    private void TPPlayer(Transform zone)
    {
        player.ModifyPulseValue(-100);
        playerTransform.position = zone.position + Vector3.up;
        playerTransform.forward = zone.forward;
    }

    private void DeactivateZonesExcept(int zoneException)
    {
        for (int i = 0; i < CurrentZones.Length; i++)
        {
            if (i == zoneException)
            {
                Destroy(CurrentZones[i]);
                CurrentZones[i] = Instantiate(ZonePrefabsToSpawn[i], ZonePositions[i], Quaternion.identity);
                EnemyZone[] enemyZones = CurrentZones[i].GetComponentsInChildren<EnemyZone>();
                foreach (EnemyZone enemyZone in enemyZones)
                {
                    enemyZone.player = player;
                }
                continue;
            }

            CurrentZones[i].SetActive(false);
        }
    }

    private void Update()
    {
        if (ReInput.players.GetPlayer(0).GetButtonDown("TPMenu"))
        {
            TPMenu = !TPMenu;
            GameEventMessage.SendEvent("TPMenu");
        }
    }

    public void OnToggleLoseLife(bool loseLife)
    {
        player.LoseLifeOnAbilities = loseLife;
    }
}
