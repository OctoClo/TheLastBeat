﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Rewired;
using Doozy.Engine;

public class TPHelper : MonoBehaviour
{
    [SerializeField]
    Player player = null;
    Transform playerTransform = null;
    [SerializeField]
    Transform[] TPZones = new Transform[6];
    [SerializeField]
    GameObject[] CurrentZones = new GameObject[6];

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
            CurrentZones[i].SetActive(i == zoneException);
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
}
