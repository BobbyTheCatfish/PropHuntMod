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

    public enum CorrectionActions
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
}
