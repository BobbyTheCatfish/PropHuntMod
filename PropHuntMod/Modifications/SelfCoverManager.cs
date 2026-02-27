using NoRepeat;
using PropHuntMod.Utils;
using PropHuntMod.Utils.Networking;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace PropHuntMod.Modifications
{
    enum Direction { Left, Right, Up, Down, Front, Back, Reset, RotateLeft, RotateRight, ScaleUp, ScaleDown };
    internal class SelfCoverManager : BaseCoverManager
    {
        public static SelfCoverManager instance;
        internal bool movedRecently = false;
        internal SelfCoverManager()
        {
            isRemote = false;
            instance = this;
        }
        public void MoveProp(Direction direction, KeyCode key, ref bool moved, bool onlyOnce = false)
        {
            if (!IsHiding) return;
            if (onlyOnce && !Input.GetKeyDown(key)) return;
            else if (!onlyOnce && !Input.GetKey(key)) return;

            moved = true;
            bool slowDown = Input.GetKey(KeyCode.LeftAlt) || Input.GetKey(KeyCode.RightAlt);
            float distance = slowDown ? 0.01f : 0.1f;

            MoveProp(direction, distance);
        }

        public void MoveProp(Direction direction, float distance = 0.1f)
        {
            if (!IsHiding)
            {
                Log.LogError("No cover, can't move");
                return;
            }

            var x = Position.x;
            var y = Position.y;
            var z = Position.z;
            var rotation = Rotation;
            var scale = Scale;

            SelfHornetManager hornet = SelfHornetManager.instance;
            if (!hornet.HornetExists()) return;

            var flipped = hornet.hornet.transform.GetScaleX() < 1;
            if (direction == Direction.Left)
            {
                if (flipped) x += distance;
                else x -= distance;
            }
            else if (direction == Direction.Right)
            {
                if (flipped) x -= distance;
                else x += distance;
            }
            else if (direction == Direction.Up) y += distance;
            else if (direction == Direction.Down) y -= distance;
            else if (direction == Direction.Front) z -= distance;
            else if (direction == Direction.Back) z += distance;
            else if (direction == Direction.ScaleUp) scale *= (1 + distance / 5);
            else if (direction == Direction.ScaleDown) scale /= (1 + distance / 5);
            else if (direction == Direction.RotateLeft)
            {
                if (flipped) rotation -= distance * 10;
                else rotation += distance * 10;
            }
            else if (direction == Direction.RotateRight)
            {
                if (flipped) rotation += distance * 10;
                else rotation -= distance * 10;
            }
            else if (direction == Direction.Reset)
            {
                x = 0;
                y = 0;
                z = 0;
                rotation = 0;
                scale = 1;
            }
            else
            {
                Log.LogError($"Invalid movement direction {direction} ({(int)direction})");
                return;
            }


            //Log.LogInfo($"Hornet position: {hornet.hornet.transform.position}");
            Vector3 newLocation = new Vector3(x, y, z);
            SetPropLocation(newLocation, rotation, scale);
            movedRecently = true;
        }

        public void SendPropPosition()
        {
            if (!movedRecently) return;
            if (!IsHiding)
            {
                Log.LogError("No cover, can't send prop position");
                return;
            }

            movedRecently = false;
            ClientNetwork.SendPropLocation(Position, Rotation, Scale);
        }

        public void EnableRandomProp()
        {
            if (PropValidation.currentSceneObjects == null)
            {
                PropValidation.GetAllProps();
            }

            if (IsSeeker()) return;

            if (PropHuntClient.maxPropSwaps > 0 && PropHuntClient.propSwaps >= PropHuntClient.maxPropSwaps)
            {
                PropHuntClient.LocalMessage($"You've reached the max number of prop swaps (${PropHuntClient.maxPropSwaps}).");
                return;
            }

            var newCover = PropValidation.currentSceneObjects.GetRandom();
            EnableProp(newCover);

            Log.LogInfo(newCover.path);
            Log.LogInfo(PropValidation.FindGameObject(newCover.path));
        }

        public bool EnableProp(Prop prop, int ticket = -1)
        {
            if (IsSeeker()) return false;

            Log.LogInfo("Enabling prop");
            var success = base.EnableProp(SelfHornetManager.instance, prop);
            if (success)
            {
                ClientNetwork.SendPropSwap(prop, ticket);
                PropHuntClient.propSwaps++;
            }

            return success;
        }

        public bool DisableProp(bool logOnFail = true, bool isSceneChange = false, int ticket = -1)
        {
            var success = base.DisableProp(SelfHornetManager.instance, logOnFail);
            if (success && !isSceneChange)
            {
                PropMovementControls.Instance.SetMovementState(MovementState.Normal);
                ClientNetwork.SendPropSwap(null, ticket);
                PropHuntMod.showHitboxes = false;
            }

            return success;
        }

        public override void OnHit(TriggerHandler handler)
        {
            Log.LogError("UH OH! ON HIT IS SUPPOSED TO BE A REMOTE PLAYER!");
        }

        bool IsSeeker()
        {
            if (PropHuntClient.isSeeker)
            {
                PropHuntClient.LocalMessage("You're a seeker! You can't enable props right now.");
                return true;
            }
            return false;
        }

        // To hide the original methods
        public override bool EnableProp(BaseHornetManager hornet, Prop prop) { return EnableProp(prop); }
        public override bool DisableProp(BaseHornetManager hornet, bool logOnFail = true) { return DisableProp(logOnFail); }
    }
}
