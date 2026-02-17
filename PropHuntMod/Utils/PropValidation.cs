using GlobalEnums;
using NoRepeat;
using PropHuntMod.Modifications;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace PropHuntMod.Utils
{
    public class Prop
    {
        public string name;
        public string path;
        public GameObject go;
    }

    internal static class PropValidation
    {
        static readonly string[] extraNames = { "corpse", "quest_board" };
        static readonly PhysLayers[] invalidLayers = { PhysLayers.ENEMIES, PhysLayers.HERO_ATTACK };
        public static NoRepeat<Prop> currentSceneObjects { get; private set; }
        static GameObject PropParent;

        static readonly Type[] allowedTypes =
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
                //Log.LogInfo($"{gameObject.name} - banned");
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

            if (renderer?.sprite?.name == "black_fader_moon")
            {
                LogSpecificObj(gameObject.name, "black fader");
                return false;
            }

            LogSpecificObj(gameObject.name, "passed negative");

            //if (gameObject.name.StartsWith("CC_metal__"))
            //{
            //    Log.LogInfo(gameObject.name);
            //}

            // Then filter positives
            if (HasScript(gameObject)) return true;
            //if (gameObject.tag == "RespawnPoint") return true; // some benches have null sprites, leave off until thats fixed

            string name = gameObject.name.ToLower();
            if (extraNames.Any(n => name.Contains(n))) return true;

            if (ExtraProps.IsExtraProp(scene, gameObject)) return true;

            return false;
        }
        static bool IsGameObjectDuplicate(SpriteRenderer newObjRenderer, GameObject existingGameObject)
        {
            try
            {
                var oldObjRenderer = GetRenderer(existingGameObject);
                //Log.LogInfo(oldObjRenderer.sprite);

                if (oldObjRenderer?.sprite == null || newObjRenderer?.sprite == null)
                {
                    Log.LogInfo($"{newObjRenderer.name} not duplicate, a sprite was null");
                    return false;
                }

                bool result = oldObjRenderer.sprite.name == newObjRenderer.sprite.name;
                //if (result)
                //{
                //    Log.LogInfo(
                //        $"{newObjRenderer.gameObject.name} is duplicate of {existingGameObject.name}",
                //        $"Old Sprite: {oldObjRenderer.sprite.name}",
                //        $"New Sprite: {newObjRenderer.sprite.name}"
                //    );
                //}

                return result;
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
            if (renderer?.sprite == null)
            {
                renderer = gameObject.GetComponentsInChildren<SpriteRenderer>().FirstOrDefault(s => s.sprite != null);
            }
            return renderer;
        }
        public static void GetAllProps()
        {
            string scene = SceneManager.GetActiveScene().name;

            Log.LogInfo($"Preparing all props for scene {scene}");

            GameObject[] allGameObjects = Resources.FindObjectsOfTypeAll<GameObject>();
            List<GameObject> props = new List<GameObject>();

            ExtraProps.Init();

            foreach (var gameObject in allGameObjects)
            {
                if (!IsValidProp(scene, gameObject)) continue;
                //Log.LogInfo($"{gameObject.name} - {gameObject.layer} MAYBE");

                LogSpecificObj(gameObject.name, "valid prop");
                var renderer = GetRenderer(gameObject);
                if (renderer == null)
                {
                    LogSpecificObj(gameObject.name, "no renderer");
                    continue;
                }

                if (props.Any(o => IsGameObjectDuplicate(renderer, o)))
                {
                    LogSpecificObj(gameObject.name, "duplicate object");
                    continue;
                }
                //Log.LogInfo($"{gameObject.name} - {gameObject.layer} YES");
                props.Add(gameObject);
            }

            PrepareAllProps(props);
            //AddPropsToOutput(scene);
        }
        static void AddPropsToOutput(string scene)
        {
            var props = currentSceneObjects.inputValues;
            string row = $"{scene},{props.Count},\"{string.Join("\n", props.Select(x => x.name))}\"";

            var filepath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "PropReport.csv");
            var file = File.AppendText(filepath);
            file.WriteLine(row);
            file.Close();
        }

        static void RemoveComponents<T>(GameObject gameObject, bool keepOnParent = false) where T : Component
        {
            var components = gameObject.GetComponentsInChildren<T>();
            foreach (var component in components)
            {
                //Log.LogInfo(component + " removed");
                if (!keepOnParent || component.gameObject != gameObject.gameObject) Component.Destroy(component);
            }
        }
        static void StripProp(GameObject gameObject)
        {
            // Remove scripts
            var scripts = gameObject.GetComponentsInChildren<MonoBehaviour>(true);
            foreach (var component in scripts)
            {
                Type type = component.GetType();
                if (!allowedTypes.Any((t) => type == t))
                {
                    Component.Destroy(component);
                }
            }

            RemoveComponents<Collider2D>(gameObject);
            RemoveComponents<Rigidbody2D>(gameObject, true);

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

        static string GeneratePropPath(GameObject prop)
        {
            string path = prop.name;
            
            var parent = prop.transform.parent;
            while (prop.transform.parent != null)
            {
                prop = prop.transform.parent.gameObject;
                path = $"{prop.name}/{path}";
            }    

            return path;
        }
        public static Prop PrepareProp(GameObject prop, bool assignParent = true)
        {
            if (prop == null)
            {
                Log.LogError("Unknown prop to prepare");
                return null;
            }

            GameObject cover;
            try
            {
                if (assignParent) cover = GameObject.Instantiate(prop, PropParent.transform);
                else
                {
                    cover = GameObject.Instantiate(prop);
                    cover.SetActive(false);
                }

                cover.layer = (int)PhysLayers.HERO_BOX;
                cover.name = prop.name;

                var breakable = cover.GetComponent<Breakable>();
                if (breakable != null && breakable.IsBroken)
                {
                    var renderer = cover.GetComponent<SpriteRenderer>();
                    if (renderer != null) renderer.enabled = true;
                }
            }
            catch (Exception e)
            {
                Log.LogError($"Ran into an error instantiating cover {prop.name}.");
                Log.LogError(e);
                return null;
            }

            StripProp(cover);

            // reset rotation before determining collider bounds
            var rot = cover.transform.rotation;
            cover.transform.rotation = Quaternion.identity;

            if (!AddPropHitbox(cover))
            {
                Log.LogInfo($"Couldn't add hitbox for {prop.name}");
                GameObject.Destroy(cover);
                return null;
            }

            // restore rotation
            cover.transform.rotation = rot;


            //Log.LogInfo(cover.name);
            return new Prop
            {
                name = cover.name,
                go = cover,
                path = GeneratePropPath(prop)
            };
        }

        static bool AddPropHitbox(GameObject prop)
        {
            Renderer[] renderers = prop.GetComponentsInChildren<Renderer>(false).Where(r => !(r is LineRenderer) && !(r is ParticleSystemRenderer)).ToArray();

            if (renderers.Length == 0)
            {
                Log.LogError("No renderers found");
                return false;
            }

            Bounds combinedBounds = renderers[0].bounds;

            foreach (var r in renderers)
            {
                var name = r.name.ToLower();
                if (prop.name == "Active") Log.LogInfo(name, r.bounds.size);
                if (
                    name.StartsWith("haze") || name.StartsWith("light") || name.StartsWith("vignette") ||
                    name.EndsWith("fader") || name.EndsWith("glow") || name.EndsWith("cutout") ||
                    name == "lit"
                ) continue;
                //Log.LogInfo(r, r.name, r.bounds);
                combinedBounds.Encapsulate(r.bounds);
            }
            //Log.LogInfo(renderers[0].bounds);

            var size = prop.transform.InverseTransformVector(combinedBounds.size);

            var collider = prop.AddComponent<BoxCollider2D>();
            collider.isTrigger = true;
            collider.offset = prop.transform.InverseTransformPoint(combinedBounds.center);
            collider.size = new Vector2(
                Mathf.Abs(size.x),
                Mathf.Abs(size.y)
            );

            var body = prop.AddComponentIfNotPresent<Rigidbody2D>();
            body.bodyType = RigidbodyType2D.Kinematic;

            prop.AddComponent<LineRenderer>();
            prop.AddComponent<DebugViewCollider>();

            prop.AddComponent<TriggerHandler>();

            foreach (Renderer renderer in renderers)
            {
                if (renderer.gameObject == prop) continue;
                renderer.gameObject.AddComponentIfNotPresent<LineRenderer>();
                renderer.gameObject.AddComponent<DebugViewBounds>();
            }

            return true;
        }

        static void PrepareAllProps(List<GameObject> props)
        {
            if (PropParent != null) GameObject.Destroy(PropParent);

            PropParent = new GameObject("PROP PARENT");
            PropParent.SetActive(false);

            if (!SelfHornetManager.instance.HornetExists()) return;

            //var hornetTransform = SelfHornetManager.instance.hornet.transform;
            //parent.transform.SetParentReset(hornetTransform);

            List<Prop> allProps = new List<Prop>();

            foreach (var prop in props)
            {
                var preppedProp = PrepareProp(prop);
                if (preppedProp != null)
                {
                    allProps.Add(preppedProp);
                }
            }

            //PropParent.SetActive(false);
            currentSceneObjects = new NoRepeat<Prop>(allProps);
        }
        public static void ResetProps()
        {
            Log.LogInfo("Resetting props");
            currentSceneObjects = null;
        }

        public static GameObject FindGameObject(string path)
        {
            string[] names = path.Split('/');

            var parent = SceneManager.GetActiveScene().GetRootGameObjects().FirstOrDefault(go => go.name == names[0]);
            if (parent == null) return null;

            for (int i = 1; i < names.Length; i++)
            {
                var name = names[i];
                
                var nextObject = FindGameObjectLayer(name, parent);
                if (nextObject == null) return null;
                
                parent = nextObject;
            }

            return parent;
        }

        static GameObject FindGameObjectLayer(string name, GameObject parent)
        {
            var childCount = parent.transform.childCount;
            for (int i = 0; i < childCount; i++)
            {
                var child = parent.transform.GetChild(i);
                if (child.name == name) return child.gameObject;
            }

            return null;
        }
    }

    class DebugViewCollider : MonoBehaviour
    {
        Color borderColor = Color.white;
        readonly float lineWidth = 0.05f;

        LineRenderer lineRenderer;
        BoxCollider2D collider;
        bool Show => PropHuntMod.showHitboxes;

        void Awake()
        {
            collider = GetComponent<BoxCollider2D>();
            lineRenderer = GetComponent<LineRenderer>();

            lineRenderer.loop = true;
            lineRenderer.useWorldSpace = true;
            lineRenderer.startWidth = lineWidth;
            lineRenderer.endWidth = lineWidth;
            lineRenderer.material = new Material(Shader.Find("Sprites/Default"));
            lineRenderer.startColor = borderColor;
            lineRenderer.endColor = borderColor;
        }

        void LateUpdate()
        {
            if (Show) DrawBox(collider);
            else lineRenderer.positionCount = 0;

        }

        void DrawBox(BoxCollider2D box)
        {
            Vector2 size = box.size * 0.5f;
            Vector2 offset = box.offset;

            Vector3[] points = new Vector3[4];
            points[0] = offset + new Vector2(-size.x, -size.y);
            points[1] = offset + new Vector2(size.x, -size.y);
            points[2] = offset + new Vector2(size.x, size.y);
            points[3] = offset + new Vector2(-size.x, size.y);

            SetPositions(points);
        }

        void SetPositions(Vector3[] localPoints)
        {
            lineRenderer.positionCount = localPoints.Length;

            for (int i = 0; i < localPoints.Length; i++)
            {
                lineRenderer.SetPosition(i, transform.TransformPoint(localPoints[i]));
            }
        }
    }

    class DebugViewBounds : MonoBehaviour
    {
        LineRenderer lineRenderer;
        Renderer[] renderers;
        Color borderColor = Color.red;
        readonly float lineWidth = 0.05f;
        bool Show => PropHuntMod.showHitboxes;

        void Awake()
        {
            lineRenderer = GetComponent<LineRenderer>();

            lineRenderer.loop = true;
            lineRenderer.useWorldSpace = true;
            lineRenderer.startWidth = lineWidth;
            lineRenderer.endWidth = lineWidth;
            lineRenderer.material = new Material(Shader.Find("Sprites/Default"));
            lineRenderer.startColor = borderColor;
            lineRenderer.endColor = borderColor;

            renderers = GetComponents<Renderer>();
        }

        void LateUpdate()
        {
            if (!Show) return;

            foreach (Renderer renderer in renderers)
            {
                DrawBox(renderer);
            }
        }

        void DrawBox(Renderer renderer)
        {
            Vector2 size = renderer.localBounds.size.DivideElements(2.1f, 2.1f);
            Vector2 offset = renderer.localBounds.center;

            Vector3[] points = new Vector3[4];
            points[0] = offset + new Vector2(-size.x, -size.y);
            points[1] = offset + new Vector2(size.x, -size.y);
            points[2] = offset + new Vector2(size.x, size.y);
            points[3] = offset + new Vector2(-size.x, size.y);

            SetPositions(points);
        }

        void SetPositions(Vector3[] localPoints)
        {
            lineRenderer.positionCount = localPoints.Length;

            for (int i = 0; i < localPoints.Length; i++)
            {
                lineRenderer.SetPosition(i, transform.TransformPoint(localPoints[i]));
            }
        }
    }
}
