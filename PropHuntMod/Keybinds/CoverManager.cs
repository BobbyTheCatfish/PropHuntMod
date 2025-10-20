using HutongGames.PlayMaker.Actions;
using System.Collections.Generic;
using UnityEngine;
using static PropHuntMod.Logging.Logging;

namespace PropHuntMod.Keybinds
{
    enum Direction { Left, Right, Up, Down }; 
    internal class CoverManager
    {
        GameObject cover;
        public void MoveProp(Direction direction, float distance = .1f)
        {
            if (!cover)
            {
                TempLog("No cover");
                return;
            }

            var x = cover.transform.position.x;
            var y = cover.transform.position.y;
            if (direction == Direction.Left) x -= distance;
            else if (direction == Direction.Right) x += distance;
            else if (direction == Direction.Up) y += distance;
            else if (direction == Direction.Down) y -= distance;
            else
            {
                TempLog("Invalid direction");
                return;
            }

            cover.transform.position = new Vector3(x, y, cover.transform.position.z);
        }

        public void DisableProp(HornetManager manager)
        {
            GameObject.Destroy(cover);
            cover = null;
            manager.ToggleHornet(true);
        }

        public void EnableProp(HornetManager hornet)
        {
            if (cover)
            {
                GameObject.Destroy(cover);
                cover = null;
            }

            var breakables = GetAllBreakables();
            var transform = hornet.hornet.transform;
            var existing = breakables.GetRandomElement();
            cover = GameObject.Instantiate(existing, transform.position, transform.rotation, transform);

            var script = GetBreakableScript(cover);
            script.enabled = false;

            cover.transform.SetPositionZ(existing.transform.position.z);
            hornet.ToggleHornet(false);
        }
        private List<GameObject> GetAllBreakables()
        {
            GameObject[] allGameObjects = Resources.FindObjectsOfTypeAll<GameObject>();
            List<GameObject> breakableObjects = new List<GameObject>();

            foreach (GameObject gameObject in allGameObjects)
            {
                var script = GetBreakableScript(gameObject);
                if (script != null)
                {
                    breakableObjects.Add(gameObject);
                }
            }

            return breakableObjects;
        }

        private Breakable GetBreakableScript(GameObject obj)
        {
            return obj.GetComponent<Breakable>();
        }
    }
}
