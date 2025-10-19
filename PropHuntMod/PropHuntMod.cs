using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using HarmonyLib.Tools;
using SSDebug.Model;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngineInternal;

[BepInPlugin("com.bobbythecatfish.prophunt", "Prop Hunt", "0.1.0")]
[BepInProcess("Hollow Knight Silksong.exe")]
public class PropHuntMod : BaseUnityPlugin
{
    GameObject hornet;
    GameObject cover;
    private void Awake()
    {
        tempLog("Prop Hunt Loaded.");
        Harmony.CreateAndPatchAll(typeof(PropHuntMod), null);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.P))
        {
            hornet = GameObject.FindGameObjectWithTag("Player");

            var position = hornet.transform;
            Console.WriteLine(position.localPosition);

            if (cover != null)
            {
                GameObject.Destroy(cover);
            }

            var clump = GameObject.Find("moss_clump_set (41)");
            cover = GameObject.Instantiate(clump, hornet.transform.position, hornet.transform.rotation, hornet.transform);


            //GameObject[] allGameObjects = Resources.FindObjectsOfTypeAll<GameObject>();

            //tempLog("--- Tags ---");
            //if (allGameObjects.Length > 0)
            //{
            //    foreach (GameObject obj in allGameObjects)
            //    {
            //        tempLog($"- {obj.name} ({obj.tag})");
            //    }
            //}
        }
    }

    private void tempLog(string msg)
    {
        Console.WriteLine(msg);
    }
}