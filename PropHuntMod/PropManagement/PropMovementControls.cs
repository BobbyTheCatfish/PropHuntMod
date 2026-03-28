using InControl;
using PropHuntMod.Utils;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace PropHuntMod.Props
{
    internal class PropMovementControls
    {
        static float StickThreshold = 0.02f;

        static SpriteRenderer StateIndicator;
        static Sprite MoveXY;
        static Sprite MoveZ;
        static Sprite Rotate;
        static Sprite Scale;

        bool moved = false;

        public static PropMovementControls Instance;
        internal static MovementState MovementState = MovementState.Normal;

        InputBlockingMechanism blocker = new InputBlockingMechanism();

        public PropMovementControls()
        {
            if (Instance == null) Instance = this;
        }

        void ChangeModes()
        {
            if (!SelfCoverManager.instance.IsHiding)
            {
                SetMovementState(MovementState.Normal);
                return;
            }

            if (MovementState == MovementState.Normal) SetMovementState(MovementState.Move2D);
            else if (MovementState == MovementState.Move2D) SetMovementState(MovementState.MoveZ);
            else if (MovementState == MovementState.MoveZ) SetMovementState(MovementState.Scale);
            else if (MovementState == MovementState.Scale) SetMovementState(MovementState.Rotate);
            else SetMovementState(MovementState.Normal);

            return;
        }

        bool ResetPosition()
        {
            if (Input.GetKeyDown(Config.PropPositionReset))
            {
                MoveProp(Direction.Reset);
                SetMovementState(MovementState.Normal);
                moved = true;
                return true;
            }

            return false;
        }

        bool ModeSwitchPressed = false;
        bool CheckStick(OneAxisInputControl input, Direction direction, bool useValue = true)
        {
            if (input.IsPressed && input.Value > StickThreshold)
            {
                var magnitude = 0.1f;
                if (useValue) magnitude = input.Value / 5;
                if (InputHandler.Instance.inputActions.Dash.IsPressed) magnitude /= 2;
                MoveProp(direction, magnitude);
                return true;
            }

            return false;
        }

        void NonKeypadUpdate(OneAxisInputControl switchModeButton, OneAxisInputControl left, OneAxisInputControl right, OneAxisInputControl up, OneAxisInputControl down, bool useValue)
        {
            left.Enabled = true;
            right.Enabled = true;
            up.Enabled = true;
            down.Enabled = true;
            switchModeButton.Enabled = true;
            InputHandler.Instance.inputActions.Dash.Enabled = true;

            if (switchModeButton.IsPressed && !ModeSwitchPressed)
            {
                ChangeModes();
                ModeSwitchPressed = true;
            }
            else if (!switchModeButton.IsPressed)
            {
                ModeSwitchPressed = false;
            }


            if (MovementState == MovementState.Move2D)
            {
                // move left or right, not both
                if (!CheckStick(left, Direction.Left, useValue)) CheckStick(right, Direction.Right, useValue);
                // move up or down, not both
                if (!CheckStick(up, Direction.Up, useValue)) CheckStick(down, Direction.Down, useValue);
            }
            else if (MovementState == MovementState.MoveZ)
            {
                if (!CheckStick(left, Direction.Back, useValue)) CheckStick(right, Direction.Front, useValue);
                if (!CheckStick(up, Direction.Back, useValue)) CheckStick(down, Direction.Front, useValue);
            }
            else if (MovementState == MovementState.Scale)
            {
                if (!CheckStick(left, Direction.ScaleUp, useValue)) CheckStick(right, Direction.ScaleDown, useValue);
                if (!CheckStick(up, Direction.ScaleUp, useValue)) CheckStick(down, Direction.ScaleDown, useValue);
            }
            else if (MovementState == MovementState.Rotate)
            {
                // rotate left or right, not both
                if (!CheckStick(left, Direction.RotateLeft, useValue)) CheckStick(right, Direction.RotateRight, useValue);
            }
        }

        void ControllerUpdate(bool rightStick = true)
        {
            var stick = rightStick ? InputManager.ActiveDevice.RightStick : InputManager.ActiveDevice.LeftStick;
            if (stick == null) return;

            var modeButton = rightStick ? InputManager.ActiveDevice.RightStickButton : InputManager.ActiveDevice.LeftStickButton;
            if (modeButton == null) return;

            NonKeypadUpdate(modeButton, stick.Left, stick.Right, stick.Up, stick.Down, true);
        }

        void NumpadUpdate()
        {
            var cover = SelfCoverManager.instance;
            cover.MoveProp(Direction.Left, KeyCode.Keypad4, ref moved);
            cover.MoveProp(Direction.Right, KeyCode.Keypad6, ref moved);
            cover.MoveProp(Direction.Up, KeyCode.Keypad8, ref moved);
            cover.MoveProp(Direction.Down, KeyCode.Keypad2, ref moved);

            cover.MoveProp(Direction.Front, KeyCode.Keypad7, ref moved);
            cover.MoveProp(Direction.Back, KeyCode.Keypad9, ref moved);

            cover.MoveProp(Direction.RotateLeft, KeyCode.Keypad1, ref moved);
            cover.MoveProp(Direction.RotateRight, KeyCode.Keypad3, ref moved);

            cover.MoveProp(Direction.ScaleUp, KeyCode.KeypadPlus, ref moved);
            cover.MoveProp(Direction.ScaleDown, KeyCode.KeypadMinus, ref moved);
            //cover.MoveProp(Direction.Reset, KeyCode.Keypad5, ref moved, true); // Probably handled by ResetPosition()
        }

        void KeyboardUpdate()
        {
            var actions = InputHandler.Instance.inputActions;
            NonKeypadUpdate(actions.Taunt, actions.Left, actions.Right, actions.Up, actions.Down, false);
        }

        public void Update()
        {
            if (ResetPosition()) return;

            switch(Config.MovementMethod)
            {
                case MovementMethods.ControllerRightStick:
                    ControllerUpdate(true);
                    break;
                case MovementMethods.ControllerLeftStick:
                    ControllerUpdate(false);
                    break;
                case MovementMethods.Numpad:
                    NumpadUpdate();
                    break;
                case MovementMethods.KeyboardMovement:
                    KeyboardUpdate();
                    break;
            }

            if (!moved) SelfCoverManager.instance.SendPropPosition();
            moved = false;
        }

        void MoveProp(Direction direction, float distance = 0.1f)
        {
            moved = true;
            SelfCoverManager.instance.MoveProp(direction, distance);
        }

        public void SetMovementState(MovementState newState)
        {
            Log.LogDebug($"Movement State: {MovementState} -> {newState}");
            MovementState = newState;

            // Block or unblock inputs
            if (newState == MovementState.Move2D)
            {
                HeroController.instance.AddInputBlocker(blocker);
            }
            else if (newState == MovementState.Normal)
            {
                // Unblock on the next frame to prevent accidental taunt inputs
                PropHuntMod.nextFrameActions.Add(() =>
                {
                    HeroController.instance.RemoveInputBlocker(blocker);
                });
            }

            if (StateIndicator == null) CreateMovementIndicator();

            switch (newState)
            {
                case MovementState.Normal:
                    StateIndicator.sprite = null;
                    break;
                case MovementState.Move2D:
                    StateIndicator.sprite = MoveXY;
                    break;
                case MovementState.MoveZ:
                    StateIndicator.sprite = MoveZ;
                    break;
                case MovementState.Scale:
                    StateIndicator.sprite = Scale;
                    break;
                case MovementState.Rotate:
                    StateIndicator.sprite = Rotate;
                    break;
            }
        }

        void CreateMovementIndicator()
        {
            var dir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

            var bundle = AssetBundle.LoadFromFile(Path.Combine(dir, "prophunt.bundle"));
            var assets = bundle.LoadAllAssets<Sprite>();

            //foreach (var asset in assets)
            //{
            //    Log.LogInfo(asset.name, asset);
            //}


            MoveXY = assets.First(a => a.name == "MoveXY");
            MoveZ = assets.First(a => a.name == "MoveZ");
            Rotate = assets.First(a => a.name == "Rotate");
            Scale = assets.First(a => a.name == "Scale");

            bundle.Unload(false);

            var canvas = GameCameras.instance.hudCamera.transform.Find("In-game/Anchor TL/Hud Canvas Offset/Hud Canvas");
            var iconPosition = canvas.Find("Delivery Icon/Parent/Delivery Icon Sprite");
            
            var indicator = GameObject.Instantiate(iconPosition, canvas);
            indicator.transform.position = iconPosition.position;

            indicator.gameObject.SetActive(true);

            StateIndicator = indicator.GetComponent<SpriteRenderer>();
            StateIndicator.sprite = null;
        }
    }
    class InputBlockingMechanism
    {

    }
}
