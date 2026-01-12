using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;
using NoRepeat;
using PropHuntMod.Utils.Networking;
using Steamworks;
using SilksongMultiplayer.NetworkData;
using GlobalEnums;
using System.IO;
using System;

namespace PropHuntMod.Modifications
{
    enum Direction { Left, Right, Up, Down, Front, Back, Reset };
    internal class CoverManager
    {
        GameObject cover;
        public string currentScene;
        public CSteamID steamID;
        public NoRepeat<GameObject> applicableCovers;
        bool movedRecently = false;
        public void MoveOwnProp(Direction direction, KeyCode key)
        {
            float distance = Input.GetKey(KeyCode.LeftAlt) ? 0.01f : 0.1f;
            if (!Input.GetKey(key)) return;

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

        public void SetPropLocation(Vector3 location)
        {
            if (cover == null)
            {
                PropHuntMod.Log.LogError("No cover, can't set prop position");
                return;
            }

            cover.transform.position = location;
        }

        public void SendPropPosition()
        {
            if (!movedRecently) return;
            if (cover == null)
            {
                PropHuntMod.Log.LogError("No cover, can't send prop position");
                return;
            }

            movedRecently = false;
            PropHuntMod.Log.LogInfo($"Sending prop position {cover.transform.position}");
            PacketSend.SendPropLocation(cover.transform.position);
        }

        public bool IsCovered()
        {
            return cover != null;
        }

        public void DisableProp(HornetManager manager, bool logOnFail = true)
        {
            if (cover == null)
            {
                if (logOnFail) PropHuntMod.Log.LogError("No cover to disable");
                return;
            }
            GameObject.Destroy(cover);
            cover = null;
            manager.ToggleHornet(true);

            string username = SteamFriends.GetPersonaName();
            NetworkDataSender.SendGlobalSystemChatMessage($"{username} was revealed!");

            if (!PlayerManager.IsRemotePlayer(steamID)) PacketSend.SendPropSwap("");
        }
        public void EnableProp(HornetManager hornet, GameObject cover)
        {
            if (cover == null)
            {
                PropHuntMod.Log.LogError("No valid cover found");
                return;
            }
            
            if (this.cover != null)
            {
                PropHuntMod.Log.LogWarning("Destroying cover...");
                GameObject.Destroy(this.cover);
                this.cover = null;
            }
            else if (!PlayerManager.IsRemotePlayer(steamID))
            {
                PropHuntMod.Log.LogInfo("Sending hiding message");
                string username = SteamFriends.GetPersonaName();
                NetworkDataSender.SendGlobalSystemChatMessage($"{username} has hidden!");
            }

            if (hornet.hornet == null) hornet.SetHornet();
            if (hornet.hornet == null)
            {
                PropHuntMod.Log.LogError($"No hornet found for {steamID}");
            }

            var transform = hornet.hornet.transform;

            try
            {
                PropHuntMod.Log.LogInfo("Creating prop");
                this.cover = GameObject.Instantiate(cover, transform.position, transform.rotation, transform);
                //this.cover.transform.SetScaleX(-1 * this.cover.transform.GetScaleX());
                this.cover.layer = (int)PhysLayers.HERO_BOX;
                hornet.ToggleHornet(false);
            }
            catch
            {
                PropHuntMod.Log.LogError("Ran into an error instantiating cover. Was the cover a bench lol?");
            }
            PropHuntMod.Log.LogInfo($"{this.cover.name} - {this.cover.layer} - {this.cover.activeInHierarchy}");

            System.Type[] allowedTypes =
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
                //typeof(AudioSource)
            };

            var scripts = this.cover.GetComponentsInChildren<MonoBehaviour>();
            foreach (var component in scripts)
            {
                System.Type type = component.GetType();
                if (!allowedTypes.Any((t) => type == t))
                {
                    PropHuntMod.Log.LogInfo(component + " removed");
                    Component.Destroy(component);
                }
            }

            void RemoveComponents<T>(bool keepOnParent = false) where T : Component
            {
                var components = this.cover.GetComponentsInChildren<T>();
                foreach (var component in components)
                {
                    PropHuntMod.Log.LogInfo(component + " removed");
                    if (keepOnParent && component.gameObject != this.cover.gameObject) Component.Destroy(component);
                }
            }

            RemoveComponents<Collider2D>();
            RemoveComponents<Rigidbody2D>(true);

            var children = this.cover.GetComponentsInChildren<Transform>();
            foreach (var component in children)
            {
                if (!component.GetComponentInChildren<Renderer>() && !component.GetComponentInChildren<ParticleSystem>())
                {
                    PropHuntMod.Log.LogInfo(component.gameObject + " GO removed");

                    component.gameObject.GetComponents<Component>().ToList().ForEach((Component c) =>
                    {
                        PropHuntMod.Log.LogInfo(c);
                    });
                    GameObject.Destroy(component.gameObject);

                }
            }


            Renderer[] renderers = this.cover.GetComponentsInChildren<Renderer>();
            if (renderers == null || renderers.Length == 0)
            {
                PropHuntMod.Log.LogError("No renderers found");
            }
            else
            {
                Bounds combinedBounds = renderers[0].bounds;

                for (int i = 1; i < renderers.Length; i++)
                {
                    combinedBounds.Encapsulate(renderers[i].bounds);
                }

                var collider = this.cover.AddComponent<BoxCollider2D>();
                PropHuntMod.Log.LogInfo($"Added collider {collider}");
                collider.isTrigger = true;

                collider.offset = this.cover.transform.InverseTransformPoint(combinedBounds.center);
                collider.size = combinedBounds.size;

                var body = this.cover.AddComponentIfNotPresent<Rigidbody2D>();
                body.bodyType = RigidbodyType2D.Kinematic;

                var t = this.cover.AddComponent<TriggerHandler>();
                t.steamID = steamID;
            }

            if (!PlayerManager.IsRemotePlayer(steamID)) PacketSend.SendPropSwap(cover.name);

            PropHuntMod.Log.LogInfo(cover.transform.localScale);
        }

        public void ViewCoverComponents()
        {
            if (this.cover == null)
            {
                PropHuntMod.Log.LogInfo("No cover found");
            }
            foreach (var component in this.cover.GetComponentsInChildren<Component>())
            {
                PropHuntMod.Log.LogInfo(component);
            }
        }
        public void EnableProp(HornetManager hornet)
        {
            if (applicableCovers == null)
            {
                applicableCovers = new NoRepeat<GameObject>(Utils.PropValidation.GetAllProps(currentScene));
            }

            var newCover = applicableCovers.GetRandom();
            EnableProp(hornet, newCover);
        }

        //private int[] invalidLayers = { 11, 17 };
        //private List<GameObject> GetAllProps()
        //{
        //    GameObject[] allGameObjects = Resources.FindObjectsOfTypeAll<GameObject>();
        //    List<GameObject> props = new List<GameObject>();
        //    foreach (GameObject gameObject in allGameObjects)
        //    {
        //        if (
        //            HasScript(gameObject)
        //            //|| (gameObject.name.ToLower().Contains("pilgrim"))
        //            || (gameObject.name.ToLower().Contains("corpse"))
        //            // || gameObject.layer == 19
        //            || gameObject.tag == "RespawnPoint"
        //        )
        //        {
        //            SpriteRenderer GetRenderer(GameObject go)
        //            {
        //                var ren = go.GetComponent<SpriteRenderer>();
        //                if (ren != null && ren.sprite == null)
        //                {
        //                    ren = go.GetComponentsInChildren<SpriteRenderer>().Where(s => s.sprite != null).FirstOrDefault();
        //                }
        //                return ren;
        //            }
        //            //PropHuntMod.Log.LogInfo($"{gameObject.name} - {gameObject.layer} MAYBE");
                    
        //            var renderer = GetRenderer(gameObject);
        //            if (
        //                ((renderer && renderer.enabled) || gameObject.GetComponent<tk2dSprite>())
        //                && !invalidLayers.Contains(gameObject.layer)
        //                && gameObject.activeInHierarchy
        //                && !Regex.IsMatch(gameObject.name, "\\(\\d+\\)? ?(\\(Clone\\))?$")
        //                && !Regex.IsMatch(gameObject.name, "pebble|junk_push|Small_bell_push|weaver_corpse_shrine", RegexOptions.IgnoreCase)
        //            )
        //            {
        //                if (props.Any(o => {
        //                    PropHuntMod.Log.LogInfo("Getting sprite renderer");
        //                    try
        //                    {
        //                        //PropHuntMod.Log.LogInfo(renderer);
        //                        PropHuntMod.Log.LogInfo(renderer.sprite);
        //                        //PropHuntMod.Log.LogInfo(renderer.sprite.texture);
        //                        //PropHuntMod.Log.LogInfo(renderer.sprite.texture.name);
        //                        var ren = GetRenderer(o);
        //                        PropHuntMod.Log.LogInfo(ren.sprite);
        //                        if (ren == null || ren.sprite == null || renderer == null) return false;
        //                        return ren.sprite.name == renderer.sprite.name;
        //                    }
        //                    catch (Exception e)
        //                    {
        //                        PropHuntMod.Log.LogError($"{gameObject.name} failed, not a real object?");
        //                        PropHuntMod.Log.LogError(e);
        //                        return true;
        //                    }
        //                }))
        //                {
        //                    continue;
        //                }

        //                PropHuntMod.Log.LogInfo($"{gameObject.name} - {gameObject.layer} YES");
        //                props.Add(gameObject);
        //            }
        //        }
        //    }

        //    AddPropsToOutput(props);
        //    return props;
        //}

        //private bool HasScript(GameObject obj)
        //{
        //    return (
        //        obj.GetComponent<Breakable>()
        //        || obj.GetComponent<PlayMakerNPC>()
        //        || obj.GetComponent<BasicNPC>()
        //        || obj.GetComponent<QuestBoardInteractable>()
        //        || obj.GetComponent<PushableRubble>()
        //        || obj.GetComponent<GrassCut>()
        //    );
        //}

        public void OnHit()
        {
            if (!PlayerManager.IsRemotePlayer(steamID))
            {
                PropHuntMod.Log.LogInfo("UH OH! ON HIT IS SUPPOSED TO BE A REMOTE PLAYER!");
                return;
            }
            else
            {
                DisableProp(PlayerManager.GetPlayerManager(steamID).hornetManager);
                PropHuntMod.Log.LogInfo($"Found {steamID}");
                PacketSend.SendPropFound(steamID);
                return;
            }
            
        }

        
    }

    class TriggerHandler : MonoBehaviour
    {
        public CSteamID steamID;
        void Awake()
        {
            Debug.Log("Hey, i'm on!");
        }
        //void OnCollisionEnter2D(Collision2D other)
        //{
        //    if (!PlayerManager.IsRemotePlayer(steamID))
        //    {
        //        PropHuntMod.Log.LogInfo($"own collider");
        //    }
        //    PropHuntMod.Log.LogInfo($"{other.collider.name} - {other.collider.tag}");
        //    if (other.collider.tag == "Nail Attack")
        //    {
        //        PlayerManager.GetPlayerManager(steamID).coverManager.OnHit();
        //    }
        //}

        void OnTriggerEnter2D(Collider2D other)
        {
            if (!PlayerManager.IsRemotePlayer(steamID))
            {
                PropHuntMod.Log.LogInfo($"own collider");
                PropHuntMod.Log.LogInfo($"{other.name} - {other.tag}");
                return;
            }
            PropHuntMod.Log.LogInfo($"{other.name} - {other.tag}");
            if (other.tag == "Nail Attack")
            {
                PlayerManager.GetPlayerManager(steamID).coverManager.OnHit();
            }
        }

        //void OnTriggerStay2D(Collider2D coll)
        //{
        //    Debug.Log(coll.gameObject.name);
        //}
        //void Update()
        //{


        //    BoxCollider2D col = GetComponent<BoxCollider2D>();
        //    Bounds b = col.bounds;

        //    Vector3 bl = new Vector3(b.min.x, b.min.y);
        //    Vector3 br = new Vector3(b.max.x, b.min.y);
        //    Vector3 tr = new Vector3(b.max.x, b.max.y);
        //    Vector3 tl = new Vector3(b.min.x, b.max.y);

        //    Debug.DrawLine(bl, br, Color.red);
        //    Debug.DrawLine(br, tr, Color.red);
        //    Debug.DrawLine(tr, tl, Color.red);
        //    Debug.DrawLine(tl, bl, Color.red);
        //}
    }
}
