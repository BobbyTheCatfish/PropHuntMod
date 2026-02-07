using InControl;
using PropHuntMod.Modifications;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using static HutongGames.PlayMaker.Actions.GamepadStickEvents;

namespace PropHuntMod.Utils
{
    internal class PropMovementControls
    {
        static float StickThreshold = 0.02f;

        static SpriteRenderer StateIndicator;
        static Sprite MoveXY;
        static Sprite MoveZ;
        static Sprite Rotate;

        bool moved = false;

        public static PropMovementControls Instance;
        internal static MovementState MovementState = MovementState.Normal;

        InputBlockingMechanism blockingMechanism = new InputBlockingMechanism();

        public PropMovementControls()
        {
            if (Instance == null) Instance = this;
        }

        void ChangeModes()
        {
            if (MovementState == MovementState.Normal) SetMovementState(MovementState.Move2D);
            else if (MovementState == MovementState.Move2D) SetMovementState(MovementState.MoveZ);
            else if (MovementState == MovementState.MoveZ) SetMovementState(MovementState.Rotate);
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
                MoveProp(direction, useValue ? input.Value / 5 : 0.1f);
                return true;
            }

            return false;
        }

        void ControllerUpdate(bool rightStick = true)
        {
            var stick = rightStick ? InputManager.ActiveDevice.RightStick : InputManager.ActiveDevice.LeftStick;
            if (stick == null) return;

            stick.Left.Enabled = true;
            stick.Right.Enabled = true;
            stick.Up.Enabled = true;
            stick.Down.Enabled = true;

            var stickButton = rightStick ? InputManager.ActiveDevice.RightStickButton : InputManager.ActiveDevice.LeftStickButton;
            stickButton.Enabled = true;

            if (stickButton.IsPressed && !ModeSwitchPressed)
            {
                ChangeModes();
                ModeSwitchPressed = true;
            }
            else if (!stickButton.IsPressed)
            {
                ModeSwitchPressed = false;
            }


            if (MovementState == MovementState.Move2D)
            {
                // move left or right, not both
                if (!CheckStick(stick.Left, Direction.Left)) CheckStick(stick.Right, Direction.Right);
                // move up or down, not both
                if (!CheckStick(stick.Up, Direction.Up)) CheckStick(stick.Down, Direction.Down);
            }
            else if (MovementState == MovementState.MoveZ)
            {
                if (!CheckStick(stick.Left, Direction.Back)) CheckStick(stick.Right, Direction.Front);
                if (!CheckStick(stick.Up, Direction.Back)) CheckStick(stick.Down, Direction.Front);
            }
            else if (MovementState == MovementState.Rotate)
            {
                // rotate left or right, not both
                if (!CheckStick(stick.Left, Direction.RotateLeft)) CheckStick(stick.Right, Direction.RotateRight);
            }
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
            cover.MoveProp(Direction.Reset, KeyCode.Keypad4, ref moved, true);
        }

        void KeyboardUpdate()
        {
            var actions = InputHandler.Instance.inputActions;

            actions.Left.Enabled = true;
            actions.Right.Enabled = true;
            actions.Up.Enabled = true;
            actions.Down.Enabled = true;
            actions.Taunt.Enabled = true;

            if (actions.Taunt.IsPressed && !ModeSwitchPressed)
            {
                ChangeModes();
                ModeSwitchPressed = true;
            }
            else if (!actions.Taunt.IsPressed)
            {
                ModeSwitchPressed = false;
            }

            if (MovementState == MovementState.Move2D)
            {
                if (!CheckStick(actions.Left, Direction.Up, false)) CheckStick(actions.Right, Direction.Right, false);
                if (!CheckStick(actions.Up, Direction.Up, false)) CheckStick(actions.Down, Direction.Down, false);
            }
            else if (MovementState == MovementState.MoveZ)
            {
                if (!CheckStick(actions.Left, Direction.Back, false)) CheckStick(actions.Right, Direction.Front, false);
                if (!CheckStick(actions.Up, Direction.Back, false)) CheckStick(actions.Down, Direction.Front, false);
            }
            else if (MovementState == MovementState.Rotate)
            {
                if (!CheckStick(actions.Left, Direction.RotateLeft, false)) CheckStick(actions.Right, Direction.RotateRight, false);
            }
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
            MovementState = newState;

            Log.LogInfo(newState);
            if (newState == MovementState.Move2D)
            {
                HeroController.instance.AddInputBlocker(blockingMechanism);
            }
            else if (newState == MovementState.Normal)
            {
                PropHuntMod.nextFrameActions.Add(() =>
                {
                    HeroController.instance.RemoveInputBlocker(blockingMechanism);
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
                case MovementState.Rotate:
                    StateIndicator.sprite = Rotate;
                    break;
            }
        }

        Sprite CreateSubSprite(Texture2D src, int index)
        {
            Rect rect = new Rect(index * 500, 0, 500, 500);
            return Sprite.Create(src, rect, new Vector2(0.5f, 0.5f), 50);
        }

        void CreateMovementIndicator()
        {
            var iconPath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "movement-icons.png");
            byte[] iconData = File.ReadAllBytes(iconPath);

            var dir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

            var bundle = AssetBundle.LoadFromFile(Path.Combine(dir, "prophunt.bundle"));
            var assets = bundle.LoadAllAssets<Sprite>();

            foreach (var asset in assets)
            {
                Log.LogInfo(asset.name, asset);
            }


            MoveXY = assets.First(a => a.name == "MoveXY");
            MoveZ = assets.First(a => a.name == "MoveZ");
            Rotate = assets.First(a => a.name == "Rotate");

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
