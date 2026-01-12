using GlobalEnums;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;

namespace PropHuntMod.Utils
{

    internal static class PropValidation
    {
        static readonly string[] extraNames = { "corpse", "quest_board" };
        static PhysLayers[] invalidLayers = { PhysLayers.ENEMIES, PhysLayers.HERO_ATTACK };


        static bool IsExtraProp(string scene, GameObject gameObject)
        {
            if (ExtraProps.props == null) ExtraProps.Init();
            var props = ExtraProps.props.FirstOrDefault(k => scene.StartsWith(k.Key)).Value;
            if (props == null) return false;

            var renderer = gameObject.GetComponent<SpriteRenderer>();
            if (renderer == null || renderer.sprite == null) return false;
            if (renderer.color != Color.white) return false;

            return props.Any(p => gameObject.name.StartsWith(p) || renderer.sprite.name.StartsWith(p));
        }

        static bool HasScript(GameObject obj)
        {
            return (
                obj.GetComponent<Breakable>()
                || obj.GetComponent<PlayMakerNPC>()
                || obj.GetComponent<BasicNPC>()
                || obj.GetComponent<QuestBoardInteractable>()
                || obj.GetComponent<PushableRubble>()
                //|| obj.GetComponent<GrassCut>()
            );
        }
        static bool IsValidProp(string scene, GameObject gameObject)
        {
            // Filter negatives first to avoid letting them through
            if (!gameObject.activeInHierarchy) return false;

            if (Regex.IsMatch(gameObject.name, "^pebble$|^junk_push|^Small_bell_push|^weaver_corpse_shrine", RegexOptions.IgnoreCase))
            {
                PropHuntMod.Log.LogInfo($"{gameObject.name} - banned");
                return false;
            }
            if (gameObject.name.Contains("(Clone)"))
            {
                //PropHuntMod.Log.LogInfo($"{gameObject.name} - clone");
                return false;
            }
            if (invalidLayers.Contains((PhysLayers)gameObject.layer))
            {
                //PropHuntMod.Log.LogInfo($"{gameObject.name} - bad layer");
                return false;
            }
            var renderer = gameObject.GetComponent<SpriteRenderer>();
            if (renderer == null)
            {
                //PropHuntMod.Log.LogInfo($"{gameObject.name} - no renderer");
                return false;
            }

            if (gameObject.name.StartsWith("CC_metal__"))
            {
                PropHuntMod.Log.LogInfo(gameObject.name);
            }

            // Then filter positives
            if (HasScript(gameObject)) return true;
            if (gameObject.tag == "RespawnPoint") return true;

            string name = gameObject.name.ToLower();
            if (extraNames.Any(n => name.Contains(n))) return true;

            if (IsExtraProp(scene, gameObject)) return true;

            return false;
        }

        static bool IsGameObjectDuplicate(SpriteRenderer newObjRenderer, GameObject existingGameObject)
        {
            try
            {
                var oldObjRenderer = GetRenderer(existingGameObject);
                //PropHuntMod.Log.LogInfo(oldObjRenderer.sprite);
                
                if (oldObjRenderer == null || oldObjRenderer.sprite == null || newObjRenderer == null || newObjRenderer.sprite == null) return false;

                return oldObjRenderer.sprite.name == newObjRenderer.sprite.name;
            }
            catch (Exception e)
            {
                PropHuntMod.Log.LogError($"{existingGameObject.name} failed, no sprite on one of the objects?");
                PropHuntMod.Log.LogError(e);
                return true;
            }
        }

        static SpriteRenderer GetRenderer(GameObject gameObject)
        {
            var renderer = gameObject.GetComponent<SpriteRenderer>();
            if (renderer != null && renderer.sprite == null)
            {
                renderer = gameObject.GetComponentsInChildren<SpriteRenderer>().FirstOrDefault(s => s.sprite != null);
            }
            return renderer;
        }
        static void AddPropsToOutput(string scene, List<GameObject> props)
        {
            string row = $"{scene},{props.Count},\"{string.Join("\n", props.Select(x => x.name))}\"";

            var file = File.AppendText("H:\\SteamLibrary\\steamapps\\common\\Hollow Knight Silksong\\BepInEx\\plugins\\prophunt\\PropReport.csv");
            file.WriteLine(row);
            file.Close();
        }
        public static List<GameObject> GetAllProps(string scene)
        {
            GameObject[] allGameObjects = Resources.FindObjectsOfTypeAll<GameObject>();
            List<GameObject> props = new List<GameObject>();

            foreach (var gameObject in allGameObjects)
            {
                if (!IsValidProp(scene, gameObject)) continue;
                PropHuntMod.Log.LogInfo($"{gameObject.name} - {gameObject.layer} MAYBE");

                var renderer = GetRenderer(gameObject);
                if (renderer == null) continue;

                if (props.Any(o => IsGameObjectDuplicate(renderer, o))) continue;
                PropHuntMod.Log.LogInfo($"{gameObject.name} - {gameObject.layer} YES");
                props.Add(gameObject);
            }

            //AddPropsToOutput(scene, props);
            return props;
        }
    }
}
