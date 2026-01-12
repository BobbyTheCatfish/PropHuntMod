using BepInEx.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace PropHuntMod.Utils
{
    internal static class Config
    {
        static ConfigEntry<bool> _disableDamage;
        //static ConfigEntry<float> _attackCooldown;
        static ConfigEntry<KeyCode> _swapPropKey;
        static ConfigEntry<KeyCode> _hideHornetKey;
        static ConfigEntry<KeyCode> _resetKey;

        public static bool disableDamage { get { return _disableDamage.Value; } }
        //public static float attackCooldown { get { return _attackCooldown.Value; } }
        public static KeyCode swapPropKey { get { return _swapPropKey.Value; } }
        public static KeyCode hideHornetKey { get { return _hideHornetKey.Value; } }
        public static KeyCode resetKey { get { return _resetKey.Value; } }

        public static void LoadConfig(ConfigFile Config)
        {
            _disableDamage = Config.Bind("General", "DisableDamage", true, "Disables all damage to the player with the exception of other players and terrain");
            _swapPropKey = Config.Bind("General", "KeySwapProp", KeyCode.P, "The key to swap props");
            _hideHornetKey = Config.Bind("General", "KeyHideHornet", KeyCode.H, "The key to hide hornet in the event that she becomes visible while hiding");
            _resetKey = Config.Bind("General", "KeyReset", KeyCode.R, "The key to unhide and remove the active prop");
            //_attackCooldown = Config.Bind("General", "AttackCooldown", 2f, "How long the seeker should have to wait between attacks");
        }
    }
}
