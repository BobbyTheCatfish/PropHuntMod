using BepInEx.Configuration;
using UnityEngine;
using PropHuntMod.Networking.Client;
using PropHuntMod.Props;


namespace PropHuntMod.Utils
{
    internal static class Config
    {
        public const string ModName = "Prop Hunt";
        public const string ModVersion = "1.0.1";
        public const int SSMPApiVersion = 1;
        public const int MaxSwapCount = 0;

        public readonly static bool AllowDebugFeatures = false;

        static ConfigEntry<bool> _disableDamage;
        static ConfigEntry<KeyCode> _swapPropKey;
        static ConfigEntry<KeyCode> _hideHornetKey;
        static ConfigEntry<KeyCode> _resetKey;

        static ConfigEntry<KeyCode> _propPositionReset;
        static ConfigEntry<MovementMethods> _movementMethod;

        static ConfigEntry<int> _seekerWaitTime;
        static ConfigEntry<int> _seekerCount;
        static ConfigEntry<float> _attackCooldown;

        public static bool DisableDamage => _disableDamage.Value && Client.GameState != GameState.NotStarted;
        //public static float attackCooldown { get { return _attackCooldown.Value; } }
        public static KeyCode SwapPropKey => _swapPropKey.Value;
        public static KeyCode HideHornetKey => _hideHornetKey?.Value ?? KeyCode.H;
        public static KeyCode ResetKey => _resetKey.Value;

        public static KeyCode PropPositionReset => _propPositionReset.Value;
        public static MovementMethods MovementMethod => _movementMethod.Value;

        public static int SeekerCountdown => _seekerWaitTime.Value;
        public static int SeekerCount => _seekerCount.Value;

        public static float AttackCooldown => _attackCooldown.Value;

        public static void LoadConfig(ConfigFile Config)
        {
            
            _disableDamage = Config.Bind("General", "DisableDamage", true, "Disables all damage to the player with the exception of other players and terrain");
            _swapPropKey = Config.Bind("General", "KeySwapProp", KeyCode.P, "The key to swap props");
            _resetKey = Config.Bind("General", "KeyReset", KeyCode.R, "The key to unhide and remove the active prop");

            _movementMethod = Config.Bind("Prop Movement", "Movement Method", MovementMethods.Numpad, "Which control style to use for prop movement");
            _movementMethod.SettingChanged += (a, b) => { PropMovementControls.MovementState = MovementState.Normal; };

            _propPositionReset = Config.Bind("Prop Movement", "Reset Position", KeyCode.Keypad5, "Resets the prop position");

            _seekerWaitTime = Config.Bind("Server Settings", "Seeker Wait Time", 30, "How long the seekers have to wait for before they can start seeking");
            _seekerCount = Config.Bind("Server Settings", "Seeker Count", 1, "How many seekers per round?");
            _attackCooldown = Config.Bind("Server Settings", "AttackCooldown", 0f, "How long the seekers should have to wait between attacks");

            if (AllowDebugFeatures)
            {
                _hideHornetKey = Config.Bind("General", "KeyHideHornet", KeyCode.H, "The key to hide hornet in the event that she becomes visible while hiding");
            }
        }
    }
}
