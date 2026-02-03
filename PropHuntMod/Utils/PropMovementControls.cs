using InControl;
using PropHuntMod.Modifications;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using static HutongGames.PlayMaker.Actions.GamepadStickEvents;

namespace PropHuntMod.Utils
{
    internal class PropMovementControls
    {
        bool moved = false;
        readonly PlayerAction left;
        readonly PlayerAction right;
        readonly PlayerAction up;
        readonly PlayerAction down;
        readonly PlayerAction swap;
        readonly PlayerAction reset;

        internal static MovementState MovementState = MovementState.Normal;

        InputBlockingMechanism blockingMechanism = new InputBlockingMechanism();

        public PropMovementControls()
        {
            var inputs = InputHandler.Instance.inputActions;

            left = inputs.Left;
            right = inputs.Right;
            up = inputs.Up;
            down = inputs.Down;
            swap = inputs.Attack;
            reset = inputs.Jump;
        }

        void ControllerUpdate()
        {
            var cover = SelfCoverManager.instance;

            if (Input.GetKeyDown(Config.PropMode))
            {
                if (MovementState == MovementState.Normal) SetMovementState(MovementState.Move2D);
                else if (MovementState == MovementState.Move2D) SetMovementState(MovementState.MoveZ);
                else if (MovementState == MovementState.MoveZ) SetMovementState(MovementState.Rotate);
                else SetMovementState(MovementState.Normal);

                return;
            }

            if (Input.GetKeyDown(Config.PropPositionReset))
            {
                MoveProp(Direction.Reset);
                SetMovementState(MovementState.Normal);
                return;
            }


            var stick = InputManager.ActiveDevice.RightStick;
            if (stick == null) return;

            stick.Left.Enabled = true;
            stick.Right.Enabled = true;
            stick.Up.Enabled = true;
            stick.Down.Enabled = true;

            if (MovementState == MovementState.Move2D)
            {
                if (stick.Left.IsPressed && stick.Left.Value < .1) MoveProp(Direction.Left);
                else if (stick.Right.IsPressed && stick.Right.Value > .1) MoveProp(Direction.Right);
                
                if (stick.Up.IsPressed) MoveProp(Direction.Up);
                else if (stick.Down.IsPressed) MoveProp(Direction.Down);
            }
            else if (MovementState == MovementState.MoveZ)
            {
                if (stick.Left.IsPressed || stick.Up.IsPressed) MoveProp(Direction.Back);
                else if (stick.Right.IsPressed || stick.Down.IsPressed) MoveProp(Direction.Front);
            }
            else if (MovementState == MovementState.Rotate)
            {
                if (stick.Left.IsPressed) MoveProp(Direction.RotateLeft);
                else if (stick.Right.IsPressed) MoveProp(Direction.RotateRight);
            }

            if (!moved) cover.SendPropPosition();
            moved = false;
        }

        public void Update()
        {
            ControllerUpdate();
        }

        void MoveProp(Direction direction)
        {
            var stick = InputManager.ActiveDevice.RightStick;
            Log.LogInfo(stick.Left.Value, stick.Right.Value);
            moved = true;
            SelfCoverManager.instance.MoveProp(direction);
        }

        void SetMovementState(MovementState newState)
        {
            MovementState = newState;

            Log.LogInfo(newState);
            if (newState == MovementState.Move2D) HeroController.instance.AddInputBlocker(blockingMechanism);
            else if (newState == MovementState.Normal) HeroController.instance.RemoveInputBlocker(blockingMechanism);
        }
    }
    class InputBlockingMechanism
    {

    }
}
