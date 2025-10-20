using BepInEx;
using BepInEx.Logging;
using GenericVariableExtension;
using HarmonyLib;
using HarmonyLib.Tools;
using PropHuntMod.Keybinds;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using static PropHuntMod.Logging.Logging;


namespace PropHuntMod
{
    [BepInPlugin("com.bobbythecatfish.prophunt", "Prop Hunt", "0.1.0")]
    [BepInProcess("Hollow Knight Silksong.exe")]
    public class PropHuntMod : BaseUnityPlugin
    {
    
        HornetManager hornet = new HornetManager();
        CoverManager cover = new CoverManager();


        private void Awake()
        {
            TempLog("Prop Hunt Loaded.");
            Harmony.CreateAndPatchAll(typeof(PropHuntMod), null);
        }

        private void Update()
        {
            //if (hornet.hornet != null)
            //{
            //    hornet.EnsureHornetHidden();
            //}
            // TOGGLE VISIBILITY
            if (Input.GetKeyDown(KeyCode.H))
            {
                TempLog("H");
				hornet.SetHornet();
				hornet.ToggleHornet();
            }
            if (Input.GetKeyDown(KeyCode.P))
            {
                TempLog("P");
                hornet.SetHornet();
                cover.EnableProp(hornet);
            }
            if (Input.GetKey(KeyCode.Keypad2)) cover.MoveProp(Direction.Down);
            if (Input.GetKey(KeyCode.Keypad4)) cover.MoveProp(Direction.Left);
            if (Input.GetKey(KeyCode.Keypad6)) cover.MoveProp(Direction.Right);
            if (Input.GetKey(KeyCode.Keypad8)) cover.MoveProp(Direction.Up);
        }
    }
}