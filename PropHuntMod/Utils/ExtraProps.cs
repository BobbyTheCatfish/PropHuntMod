

using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace PropHuntMod.Utils
{
    internal static class ExtraProps
    {
        //public static Dictionary<string, string[]> props;
        static string[] sceneProps;
        static string[] scenePrefixes;
        static string scenePrefix;

        public static void Init()
        {
            var scene = SceneManager.GetActiveScene().name;

            // same prop list
            if (scenePrefixes != null)
            {
                var prefix = scenePrefixes.FirstOrDefault(p => scene.StartsWith(p));
                if (string.IsNullOrEmpty(prefix))
                {
                    Log.LogDebug("EPI: Scene not in extra props list");
                    Log.LogDebug(sceneProps?.Length);
                    sceneProps = null;
                    scenePrefix = null;
                    return;
                }

                if (prefix == scenePrefix)
                {
                    Log.LogDebug("EPI: Scene is same.");
                    Log.LogDebug(sceneProps.Length);
                    return;
                }
            }

            Log.LogInfo("Proceeding with extra prop loading");

            // load json file
            var path = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "ExtraProps.json");
            var jsonFile = File.ReadAllText(path);
            var json = JsonConvert.DeserializeObject<Dictionary<string, string[]>>(jsonFile);

            scenePrefixes = json.Keys.ToArray();
            Log.LogInfo("Prefixes:", string.Join(", ", scenePrefixes));

            var key = json.Keys.FirstOrDefault(s => scene.StartsWith(s));
            Log.LogInfo("Key: " + key);

            if (string.IsNullOrEmpty(key))
            {
                Log.LogError("EPI: key not found");
                sceneProps = null;
                scenePrefix = null;
                return;
            }

            sceneProps = json[key];
            Log.LogInfo("Props:", string.Join(", ", sceneProps));
            scenePrefix = key;

        }

        public static bool IsExtraProp(string scene, GameObject gameObject)
        {
            if (string.IsNullOrEmpty(scene))
            {
                Log.LogDebug("IEP: No scene name");
                return false;
            }

            if (sceneProps == null)
            {
                Log.LogDebug("IEP: No scene props");
                return false;
            }
            //Log.LogError("success");

            var renderer = gameObject.GetComponent<SpriteRenderer>();
            if (renderer?.sprite == null) return false;
            if (renderer.color.grayscale < 0.75f) return false;

            bool result = sceneProps.Any(p => renderer.sprite.name.StartsWith(p));
            //bool result = sceneProps.Any(p => gameObject.name.StartsWith(p) || renderer.sprite.name.StartsWith(p));
            return result;
        }
    }
}