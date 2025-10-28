using BepInEx.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace PropHuntMod
{
    internal class ModConfiguration
    {
        public ConfigEntry<bool> disableDamage;
        public ConfigEntry<KeyCode> swapPropKey;
        public ConfigEntry<KeyCode> hideHornetKey;
        public ConfigEntry<KeyCode> resetKey;
        public ConfigEntry<float> attackCooldown;
        public void LoadConfig(ConfigFile Config)
        {
            disableDamage = Config.Bind("General", "DisableDamage", true, "Disables all damage to the player with the exception of other players and terrain");
            swapPropKey = Config.Bind("General", "KeySwapProp", KeyCode.P, "The key to swap props");
            hideHornetKey = Config.Bind("General", "KeyHideHornet", KeyCode.H, "The key to hide hornet in the event that she becomes visible while hiding");
            resetKey = Config.Bind("General", "KeyReset", KeyCode.R, "The key to unhide and remove the active prop");
            attackCooldown = Config.Bind("General", "AttackCooldown", 2f, "How long the seeker should have to wait between attacks");
        }
    }
}
