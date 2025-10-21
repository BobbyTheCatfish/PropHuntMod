using HutongGames.PlayMaker.Actions;
using System;
using System.Collections.Generic;
using UnityEngine;
using static PropHuntMod.Logging.Logging;
using PropHuntMod.PropManager;
using System.Linq;

namespace PropHuntMod.Keybinds
{
    enum Direction { Left, Right, Up, Down, Front, Back };
    internal class CoverManager
    {
        Props props = new Props();
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
            var z = cover.transform.position.z;
            if (direction == Direction.Left) x -= distance;
            else if (direction == Direction.Right) x += distance;
            else if (direction == Direction.Up) y += distance;
            else if (direction == Direction.Down) y -= distance;
            else if (direction == Direction.Front) z -= distance;
            else if (direction == Direction.Back) z += distance;
            else
            {
                TempLog("Invalid direction");
                return;
            }

            cover.transform.position = new Vector3(x, y, z);
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
            if (script) Component.Destroy(script);
            else TempLog("Breakable script not found");

            //cover.transform.SetPositionZ(existing.transform.position.z);
            hornet.ToggleHornet(false);
        }
        private List<GameObject> GetAllBreakables()
        {
            GameObject[] allGameObjects = Resources.FindObjectsOfTypeAll<GameObject>();
            List<GameObject> breakableObjects = new List<GameObject>();

            foreach (GameObject gameObject in allGameObjects)
            {
                if (GetBreakableScript(gameObject))
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

        private List<MonoBehaviour> GetBreakableScripts(GameObject obj)
        {

            List<MonoBehaviour> breakables = new List<MonoBehaviour>();
            //var baseBreak = obj.GetComponent<Breakable>();
            //var breakObj = obj.GetComponent<BreakableObject>();
            //var breakPole = obj.GetComponent<BreakablePole>();
            //var breakPoleSimple = obj.GetComponent<BreakablePoleSimple>();
            //var breakPoleTopLand = obj.GetComponent<BreakablePoleTopLand>();


            List<string> uniqueStrings = new List<string>();
            Console.WriteLine($"Scripts: {obj.GetComponentsInChildren<MonoBehaviour>().Length}");
            foreach (MonoBehaviour script in obj.GetComponentsInChildren<MonoBehaviour>())
            {
                var name = script.GetType().Name;
                if (props.breakableScriptNames.Contains(name))
                {
                    breakables.Add(script);
                    if (!uniqueStrings.Contains(name))
                    {
                        uniqueStrings.Add(name);
                        TempLog($"{name} - {obj.name}");
                    }
                }
                else
                {
                    TempLog($"Script {name} - N/A");
                }
            }
            return breakables;
            //obj.GetComponent<BreakOnHazard>();
            //return obj.GetComponent<Breakable>();
        }
    }
}
