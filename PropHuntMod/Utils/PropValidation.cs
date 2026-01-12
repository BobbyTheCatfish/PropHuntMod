using GlobalEnums;
using NoRepeat;
using PropHuntMod.Modifications;
using Steamworks;
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
        public static NoRepeat<GameObject> currentSceneObjects;

        static Type[] allowedTypes =
        {
            typeof(Transform),
            typeof(MeshFilter),
            typeof(Renderer),
            typeof(MeshRenderer),
            typeof(SpriteRenderer),
            typeof(tk2dSprite),
            typeof(tk2dSpriteAnimator),
            typeof(tk2dSpriteAnimation),
            typeof(tk2dSpriteAnimationClip),
            typeof(tk2dSpriteAnimationFrame),
            typeof(tk2dLookAnimNPC),
            typeof(CurveRotationAnimation)
        };
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
        static bool IsExtraProp(string scene, GameObject gameObject)
        {
                if (string.IsNullOrEmpty(scene)) return false;
                if (ExtraProps.props == null) ExtraProps.Init();
                var props = ExtraProps.props.FirstOrDefault(k => scene.StartsWith(k.Key)).Value;

                if (props == null) return false;
                Log.LogError("success");
                var renderer = gameObject.GetComponent<SpriteRenderer>();
                if (renderer == null || renderer.sprite == null) return false;
                if (renderer.color != Color.white) return false;

                return props.Any(p => gameObject.name.StartsWith(p) || renderer.sprite.name.StartsWith(p));
        }
        static void LogSpecificObj(string objectName, string condition)
        {
            if (objectName == "Bonechurch_shop") Log.LogInfo(condition);
        }
        static bool IsValidProp(string scene, GameObject gameObject)
        {
            // Filter negatives first to avoid letting them through
            if (!gameObject.activeInHierarchy)
            {
                LogSpecificObj(gameObject.name, "inactive");
                return false;
            }

            if (Regex.IsMatch(gameObject.name, "^pebble$|^junk_push|^Small_bell_push|^weaver_corpse_shrine", RegexOptions.IgnoreCase))
            {
                Log.LogInfo($"{gameObject.name} - banned");
                return false;
            }
            if (gameObject.name.Contains("(Clone)"))
            {
                LogSpecificObj(gameObject.name, "clone");
                return false;
            }
            if (invalidLayers.Contains((PhysLayers)gameObject.layer))
            {
                LogSpecificObj(gameObject.name, "layer " + (PhysLayers)gameObject.layer);
                return false;
            }
            var renderer = gameObject.GetComponent<SpriteRenderer>();
            if (renderer == null && gameObject.GetComponent<tk2dSprite>() == null)
            {
                LogSpecificObj(gameObject.name, "no renderer");
                return false;
            }

            LogSpecificObj(gameObject.name, "passed negative");

            //if (gameObject.name.StartsWith("CC_metal__"))
            //{
            //    Log.LogInfo(gameObject.name);
            //}

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
                //Log.LogInfo(oldObjRenderer.sprite);
                
                if (oldObjRenderer == null || oldObjRenderer.sprite == null || newObjRenderer == null || newObjRenderer.sprite == null) return false;

                return oldObjRenderer.sprite.name == newObjRenderer.sprite.name;
            }
            catch (Exception e)
            {
                Log.LogError($"{existingGameObject.name} failed, no sprite on one of the objects?");
                Log.LogError(e);
                return true;
            }
        }
        
        static SpriteRenderer GetRenderer(GameObject gameObject)
        {
            var renderer = gameObject.GetComponent<SpriteRenderer>();
            if (renderer == null || renderer.sprite == null)
            {
                renderer = gameObject.GetComponentsInChildren<SpriteRenderer>().FirstOrDefault(s => s.sprite != null);
            }
            return renderer;
        }
        public static void GetAllProps(string scene)
        {
            GameObject[] allGameObjects = Resources.FindObjectsOfTypeAll<GameObject>();
            List<GameObject> props = new List<GameObject>();

            foreach (var gameObject in allGameObjects)
            {
                if (!IsValidProp(scene, gameObject)) continue;
                //Log.LogInfo($"{gameObject.name} - {gameObject.layer} MAYBE");

                if (gameObject.name == "Bonechurch_Shop") Log.LogInfo("valid prop");
                var renderer = GetRenderer(gameObject);
                if (renderer == null) continue;

                if (props.Any(o => IsGameObjectDuplicate(renderer, o))) continue;
                Log.LogInfo($"{gameObject.name} - {gameObject.layer} YES");
                props.Add(gameObject);
            }

            PrepareAllProps(props);
            //AddPropsToOutput(scene, props);
        }
        static void AddPropsToOutput(string scene, List<GameObject> props)
        {
            string row = $"{scene},{props.Count},\"{string.Join("\n", props.Select(x => x.name))}\"";

            var file = File.AppendText("H:\\SteamLibrary\\steamapps\\common\\Hollow Knight Silksong\\BepInEx\\plugins\\prophunt\\PropReport.csv");
            file.WriteLine(row);
            file.Close();
        }
        static void StripProp(GameObject gameObject)
        {
            // Remove scripts
            var scripts = gameObject.GetComponentsInChildren<MonoBehaviour>();
            foreach (var component in scripts)
            {
                Type type = component.GetType();
                if (!allowedTypes.Any((t) => type == t))
                {
                    Component.Destroy(component);
                }
            }

            // Remove physics objects
            void RemoveComponents<T>(bool keepOnParent = false) where T : Component
            {
                var components = gameObject.GetComponentsInChildren<T>();
                foreach (var component in components)
                {
                    //Log.LogInfo(component + " removed");
                    if (keepOnParent && component.gameObject != gameObject.gameObject) Component.Destroy(component);
                }
            }

            RemoveComponents<Collider2D>();
            RemoveComponents<Rigidbody2D>(true);

            // Remove empty children such as detectors
            var children = gameObject.GetComponentsInChildren<Transform>();
            foreach (var component in children)
            {
                if (!component.GetComponentInChildren<Renderer>() && !component.GetComponentInChildren<ParticleSystem>())
                {
                    GameObject.Destroy(component.gameObject);
                }
            }
        }
        static void PrepareAllProps(List<GameObject> props)
        {
            var parent = new GameObject("PROP PARENT");
            parent.SetActive(false);

            List<GameObject> allProps = new List<GameObject>();

            foreach (var prop in props)
            {
                GameObject cover = null;
                try
                {
                    cover = GameObject.Instantiate(prop, parent.transform.position, parent.transform.rotation, parent.transform);
                    cover.layer = (int)PhysLayers.HERO_BOX;
                }
                catch (Exception e)
                {
                    Log.LogError("Ran into an error instantiating cover.");
                    Log.LogError(e);
                }

                if (cover == null) continue;
                Log.LogInfo(cover.name);

                StripProp(cover);

                // Add hit detection
                Renderer[] renderers = parent.GetComponentsInChildren<Renderer>();
                if (renderers.Length == 0)
                {
                    Log.LogError("No renderers found");
                }
                else
                {
                    Bounds combinedBounds = renderers[0].bounds;

                    for (int i = 1; i < renderers.Length; i++)
                    {
                        combinedBounds.Encapsulate(renderers[i].bounds);
                    }

                    var collider = cover.AddComponent<BoxCollider2D>();
                    collider.isTrigger = true;
                    collider.offset = cover.transform.InverseTransformPoint(combinedBounds.center);
                    collider.size = combinedBounds.size;

                    var body = cover.AddComponentIfNotPresent<Rigidbody2D>();
                    body.bodyType = RigidbodyType2D.Kinematic;

                    cover.AddComponent<TriggerHandler>();
                }

                allProps.Add(cover);
            }

            currentSceneObjects = new NoRepeat<GameObject>(allProps);
        }
        public static void ResetProps()
        {
            currentSceneObjects = null;
        }
    }
}
