using BepInEx.Configuration;
using UnityEngine;



namespace PropHuntMod.Utils
{
    internal static class Config
    {
        public const string ModName = "Prop Hunt";
        public const string ModVersion = "1.0.0";
        public const int SSMPApiVersion = 1;
        public const int MaxSwapCount = 0;
        public const int SeekerCountdown = 30;

        public readonly static bool AllowDebugFeatures = false;

        static ConfigEntry<bool> _disableDamage;
        //static ConfigEntry<float> _attackCooldown;
        static ConfigEntry<KeyCode> _swapPropKey;
        static ConfigEntry<KeyCode> _hideHornetKey;
        static ConfigEntry<KeyCode> _resetKey;

        static ConfigEntry<KeyCode> _propPositionReset;
        static ConfigEntry<MovementMethods> _movementMethod;

        public static bool DisableDamage => _disableDamage.Value && PropHuntClient.GameState != GameState.NotStarted;
        //public static float attackCooldown { get { return _attackCooldown.Value; } }
        public static KeyCode SwapPropKey => _swapPropKey.Value;
        public static KeyCode HideHornetKey => _hideHornetKey?.Value ?? KeyCode.H;
        public static KeyCode ResetKey => _resetKey.Value;

        public static KeyCode PropPositionReset => _propPositionReset.Value;
        public static MovementMethods MovementMethod => _movementMethod.Value;

        public static void LoadConfig(ConfigFile Config)
        {
            
            _disableDamage = Config.Bind("General", "DisableDamage", true, "Disables all damage to the player with the exception of other players and terrain");
            _swapPropKey = Config.Bind("General", "KeySwapProp", KeyCode.P, "The key to swap props");
            _resetKey = Config.Bind("General", "KeyReset", KeyCode.R, "The key to unhide and remove the active prop");

            _movementMethod = Config.Bind("Prop Movement", "Movement Method", MovementMethods.Numpad, "Which control style to use for prop movement");
            _movementMethod.SettingChanged += (a, b) => { PropMovementControls.MovementState = MovementState.Normal; };

            _propPositionReset = Config.Bind("Prop Movement", "Reset Position", KeyCode.Keypad5, "Resets the prop position");


            if (AllowDebugFeatures)
            {
                _hideHornetKey = Config.Bind("General", "KeyHideHornet", KeyCode.H, "The key to hide hornet in the event that she becomes visible while hiding");
            }
            //_attackCooldown = Config.Bind("General", "AttackCooldown", 2f, "How long the seeker should have to wait between attacks");
        }
    }
}
