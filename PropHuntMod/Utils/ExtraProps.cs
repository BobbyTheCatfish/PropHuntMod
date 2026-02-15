

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
            if (!string.IsNullOrEmpty(scenePrefix) && scene.StartsWith(scenePrefix)) return;

            // not in the prefix list
            if (scenePrefixes != null && !scenePrefixes.Any(s => scene.StartsWith(s))) return;

            Log.LogInfo("Proceeding with extra prop loading");

            // load json file
            var path = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "ExtraProps.json");
            var jsonFile = File.ReadAllText(path);
            var json = JsonConvert.DeserializeObject<Dictionary<string, string[]>>(jsonFile);

            scenePrefixes = json.Keys.ToArray();

            var key = json.Keys.FirstOrDefault(s => scene.StartsWith(s));
            if (string.IsNullOrEmpty(key)) return;

            sceneProps = json[key];
            scenePrefix = key;

        }

        public static bool IsExtraProp(string scene, GameObject gameObject)
        {
            if (string.IsNullOrEmpty(scene)) return false;

            
            if (sceneProps == null) return false;
            //Log.LogError("success");

            var renderer = gameObject.GetComponent<SpriteRenderer>();
            if (renderer?.sprite == null) return false;
            if (renderer.color != Color.white) return false;

            return sceneProps.Any(p => gameObject.name.StartsWith(p) || renderer.sprite.name.StartsWith(p));
        }
    }
}