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
    enum Direction { Left, Right, Up, Down, Front, Back, Reset, RotateLeft, RotateRight };
    internal class SelfCoverManager : BaseCoverManager
    {
        public static SelfCoverManager instance;
        internal bool movedRecently = false;
        internal SelfCoverManager()
        {
            isRemote = false;
            instance = this;
        }
        public void MoveProp(Direction direction, KeyCode key, bool onlyOnce = false)
        {
            if (onlyOnce && !Input.GetKeyDown(key)) return;
            else if (!onlyOnce && !Input.GetKey(key)) return;

            bool slowDown = Input.GetKey(KeyCode.LeftAlt) || Input.GetKey(KeyCode.RightAlt);
            float distance = slowDown ? 0.01f : 0.1f;

            if (!IsHiding)
            {
                Log.LogError("No cover, can't move");
                return;
            }

            var x = position.x;
            var y = position.y;
            var z = position.z;
            var rotation = this.rotation;

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
            else if (direction == Direction.RotateLeft) rotation += distance * 10;
            else if (direction == Direction.RotateRight) rotation -= distance * 10;
            else if (direction == Direction.Reset)
            {
                x = 0;
                y = 0;
                z = 0;
                rotation = 0;
            }
            else
            {
                Log.LogError($"Invalid movement direction {direction} ({(int)direction})");
                return;
            }


            //Log.LogInfo($"Hornet position: {hornet.hornet.transform.position}");
            Vector3 newLocation = new Vector3(x, y, z);
            SetPropLocation(newLocation, rotation);
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
            ClientNetwork.SendPropLocation(position, rotation);
        }

        public void EnableProp()
        {
            if (PropValidation.currentSceneObjects == null)
            {
                PropValidation.GetAllProps();
            }

            var newCover = PropValidation.currentSceneObjects.GetRandom();
            EnableProp(newCover);
        }

        public bool EnableProp(GameObject cover)
        {
            var success = base.EnableProp(SelfHornetManager.instance, cover);
            if (success) ClientNetwork.SendPropSwap(cover.name);

            return success;
        }

        public bool DisableProp(bool logOnFail = true)
        {
            var success = base.DisableProp(SelfHornetManager.instance, logOnFail);
            if (success) ClientNetwork.SendPropSwap("");

            return success;
        }

        public override void OnHit()
        {
            Log.LogError("UH OH! ON HIT IS SUPPOSED TO BE A REMOTE PLAYER!");
        }

        // To hide the original methods
        public override bool EnableProp(BaseHornetManager hornet, GameObject cover) { return EnableProp(cover); }
        public override bool DisableProp(BaseHornetManager hornet, bool logOnFail = true) { return DisableProp(logOnFail); }
    }
}
