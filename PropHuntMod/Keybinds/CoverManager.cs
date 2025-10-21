using HutongGames.PlayMaker.Actions;
using System;
using System.Collections.Generic;
using UnityEngine;
using static PropHuntMod.Logging.Logging;
using System.Linq;
using UnityEngine.SceneManagement;
using System.Text.RegularExpressions;

namespace PropHuntMod.Keybinds
{
    enum Direction { Left, Right, Up, Down, Front, Back };
    internal class CoverManager
    {
        GameObject cover;
        string currentScene;
        NoRepeat<GameObject> applicableCovers;
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

            var sceneName = SceneManager.GetActiveScene().name;
            if (currentScene != sceneName || applicableCovers == null)
            {
                currentScene = sceneName;
                applicableCovers = new NoRepeat<GameObject>(GetAllBreakables());
            }

            var transform = hornet.hornet.transform;
            var existing = applicableCovers.GetRandom();
            if (existing == null)
            {
                TempLog("No valid cover found");
                return;
            }
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
                if (GetBreakableScript(gameObject) && !Regex.IsMatch(gameObject.name, "\\(\\d+\\)$"))
                {
                    TempLog(gameObject.name);
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
