using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PropHuntMod.Modifications;
using QuickWarp;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace PropHuntMod.Utils
{
    class WarpPoint
    {
        public string scene;
        public string transition;
    }

    internal static class PropTesting
    {
        static int propIndex = -1;
        static int sceneIndex = 0;
        static bool warned = false;


        static List<Prop> props => PropValidation.currentSceneObjects.inputValues;

        static List<WarpPoint> transitions = new List<WarpPoint>();

        static List<string> bannedScenes = new List<string>
        {
            "Diving_Bell_Abyss_Fixed",
            "Room_Diving_Bell_Abyss",
            "Last_Dive_Return",
            "Abyss_Cocoon",
            "Last_Dive"
        };

        public static void Init()
        {
            transitions = new List<WarpPoint>();
            var areas = Warp.GetAreaNames();
            foreach (var area in areas)
            {
                var scenes = Warp.GetSceneNames(area);
                foreach (var scene in scenes)
                {

                    if (bannedScenes.Contains(scene)) continue;

                    var transition = Warp.GetTransitionNames(scene).FirstOrDefault();
                    if (transition == null)
                    {
                        Log.LogInfo($"No transition for {area} {scene}");
                        continue;
                    }

                    transitions.Add(new WarpPoint { scene = scene, transition = transition });
                }
            }

            transitions.Sort((a, b) => a.scene.CompareTo(b.scene));
        }

        public static void PropPrevious()
        {
            if (propIndex <= 0)
            {
                GoToScene(sceneIndex - 1);
                return;
            }

            propIndex--;
            SetProp();
            
        }

        public static void PropNext()
        {
            if (propIndex >= props.Count - 1)
            {
                GoToScene(sceneIndex + 1);
                return;
            }

            propIndex++;
            SetProp();
        }

        static void SetProp()
        {
            var prop = props[propIndex];
            var scene = transitions[sceneIndex].scene;

            var isExtra = ExtraProps.IsExtraProp(scene, prop.go);
            if (isExtra)
            {
                var sprite = prop.go.GetComponent<SpriteRenderer>()?.sprite?.name;
                Log.LogWarning(prop.name, $"Is extra, sprite is {sprite}");
            }

            SelfCoverManager.instance.EnableProp(prop);
            warned = false;
        }

        static void GoToScene(int index)
        {
            if (transitions.Count == 0)
            {
                Init();
            }

            if (index < 0)
            {
                Log.LogError("Ran out of scenes. Cannot go back.");
                return;
            }

            if (index >= transitions.Count)
            {
                Log.LogError("Ran out of scenes. Cannot go forward.");
                return;
            }

            if (!warned)
            {
                warned = true;
                Log.LogInfo("No more props in this room. Press again to continue.");

                return;
            }

            propIndex = 0;
            sceneIndex = index;
            var transition = transitions[index];

            Log.LogInfo($"Going to {transition.scene}");
            Warp.TryWarp(transition.scene, transition.transition);
        }

        public static void OnSceneChange()
        {
            if (transitions.Count == 0)
            {
                Init();
            }

            warned = false;
            var scene = SceneManager.GetActiveScene().name;
            var index = transitions.FindIndex(t => t.scene == scene);

            if (index == -1)
            {
                Log.LogError($"Couldn't find transition for current scene {scene}");
                return;
            }

            sceneIndex = index;
        }
    }
}
