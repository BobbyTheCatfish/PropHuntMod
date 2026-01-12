using NoRepeat;
using PropHuntMod.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace PropHuntMod.Modifications
{
    internal class SelfCoverManager : BaseCoverManager
    {
        
        public void MoveProp(Direction direction, KeyCode key, bool onlyOnce = false)
        {
            if (onlyOnce && !Input.GetKeyDown(key)) return;
            else if (!onlyOnce && !Input.GetKey(key)) return;

            bool slowDown = Input.GetKey(KeyCode.LeftAlt) || Input.GetKey(KeyCode.RightAlt);
            float distance = slowDown ? 0.01f : 0.1f;

            if (!cover)
            {
                PropHuntMod.Log.LogError("No cover");
                return;
            }

            var x = cover.transform.position.x;
            var y = cover.transform.position.y;
            var z = cover.transform.position.z;

            HornetManager hornet = PropHuntMod.hornet;
            var pos = hornet.hornet.transform.position;

            if (direction == Direction.Left) x -= distance;
            else if (direction == Direction.Right) x += distance;
            else if (direction == Direction.Up) y += distance;
            else if (direction == Direction.Down) y -= distance;
            else if (direction == Direction.Front) z -= distance;
            else if (direction == Direction.Back) z += distance;
            else if (direction == Direction.Reset)
            {
                x = pos.x;
                y = pos.y;
                z = pos.z;
            }
            else
            {
                PropHuntMod.Log.LogError("Invalid direction");
                return;
            }

            x = Mathf.Clamp(x, pos.x - 2, pos.x + 2);
            y = Mathf.Clamp(y, pos.y - 4, pos.y + 4);
            z = Mathf.Clamp(z, pos.z - 4, pos.z + 4);

            PropHuntMod.Log.LogInfo($"Hornet position: {hornet.hornet.transform.position}");

            SetPropLocation(new Vector3(x, y, z));
            movedRecently = true;
        }

        public void EnableProp()
        {
            if (PropValidation.currentSceneObjects == null)
            {
                PropValidation.GetAllProps(currentScene);
            }

            var newCover = PropValidation.currentSceneObjects.GetRandom();
            EnableProp(PropHuntMod.hornet, newCover);
        }

        public new void OnHit()
        {
            PropHuntMod.Log.LogError("UH OH! ON HIT IS SUPPOSED TO BE A REMOTE PLAYER!");
        }
    }
}
