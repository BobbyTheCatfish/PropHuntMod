using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PropHuntMod.Utils
{
    public enum CustomPackets
    {
        PropSwap,
        RoundStart,
        PropLocation,
        HideStatus,
        PropFound,
        Sync,
        GameOver,
        SeekerStart,
        FailedAction
    }

    public enum GameState
    {
        NotStarted,
        SeekerWait,
        Playing
    }

    public enum CorrectiveActions
    {
        None,
        DisableClientProp,
        DisablePlayerProp,
        PreviousScene, // requires ticket
        ToggleHornetTrue,
        ToggleHornetFalse, // requires ticket
        RestoreLastProp, // requires ticket
        RestoreTriggerHandler,
        BecomeSeeker,
    }

    public enum MovementState
    {
        Normal,
        Move2D,
        MoveZ,
        Scale,
        Rotate
    }

    public enum MovementMethods
    {
        Numpad,
        KeyboardMovement,
        ControllerRightStick,
        ControllerLeftStick,
    }

    internal static class Consts
    {
        public const int MIN_X = -2;
        public const int MAX_X = 2;

        public const int MIN_Y = -4;
        public const int MAX_Y = 4;

        public const int MIN_Z = -8;
        public const int MAX_Z = 8;

        public const float MIN_S = 0.5f;
        public const float MAX_S = 2f;
    }
}
