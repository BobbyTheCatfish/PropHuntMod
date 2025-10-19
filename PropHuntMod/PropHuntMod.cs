using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using UnityEngine;

[BepInPlugin("com.bobbythecatfish.prophunt", "Prop Hunt", "0.1.0")]
public class PropHuntMod : BaseUnityPlugin
{
    private void Awake()
    {
        Logger.LogInfo("Prop Hunt loaded and intialized");

        Harmony.CreateAndPatchAll(typeof(PropHuntMod), null);
    }
}