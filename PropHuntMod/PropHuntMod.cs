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
            GameObject[] allGameObjects = Resources.FindObjectsOfTypeAll<GameObject>();
            HashSet<string> uniqueTags = new HashSet<string>();

            foreach (GameObject go in allGameObjects)
            {
                if (!string.IsNullOrEmpty(go.tag) && go.tag != "Untagged")
                {
                    uniqueTags.Add(go.tag);
                }
            }

            tempLog("--- Found Unique Tags ---");
            if (uniqueTags.Count > 0)
            {
                foreach (string tag in uniqueTags)
                {
                    tempLog($"- {tag}");
                }
            }
        }
    }

    private void tempLog(string msg)
    {
        Console.WriteLine(msg);
    }
}