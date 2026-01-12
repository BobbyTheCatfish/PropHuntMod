using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using NoRepeat;
using PropHuntMod.Utils.Networking;
using Steamworks;
using SilksongMultiplayer.NetworkData;
using GlobalEnums;
using System;

namespace PropHuntMod.Modifications
{
    enum Direction { Left, Right, Up, Down, Front, Back, Reset };
    internal class BaseCoverManager
    {
        internal GameObject cover;
        internal string coverOGName = "";
        public string currentScene;
        public CSteamID steamID;
        internal bool movedRecently = false;
        

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
            coverOGName = "";
            manager.ToggleHornet(true);

            string username = SteamFriends.GetPersonaName();
            NetworkDataSender.SendGlobalSystemChatMessage($"{username} was revealed!");

            if (!PlayerManager.IsRemotePlayer(steamID)) PacketSend.SendPropSwap("");
        }
        public void EnableProp(HornetManager hornet, GameObject cover)
        {
            // Can't do anything
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

            // Create prop, parent to hornet, and hide hornet
            if (hornet.hornet == null) hornet.SetHornet();
            var transform = hornet.hornet.transform;

            try
            {
                PropHuntMod.Log.LogInfo("Creating prop");
                this.cover = GameObject.Instantiate(cover, transform.position, transform.rotation, transform);
                cover.SetActive(true);
                coverOGName = cover.name;

                this.cover.layer = (int)PhysLayers.HERO_BOX;
                hornet.ToggleHornet(false);
            }
            catch (Exception e)
            {
                PropHuntMod.Log.LogError("Ran into an error instantiating cover.");
                PropHuntMod.Log.LogError(e);
            }

            PropHuntMod.Log.LogInfo($"{this.cover.name} - {this.cover.layer} - {this.cover.activeInHierarchy}");

            if (!PlayerManager.IsRemotePlayer(steamID)) PacketSend.SendPropSwap(cover.name);
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
            DisableProp(PlayerManager.GetPlayerManager(steamID).hornetManager);
            PropHuntMod.Log.LogInfo($"Found {steamID}");
            PacketSend.SendPropFound(steamID);
            return;
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
                //PropHuntMod.Log.LogInfo($"own collider");
                //PropHuntMod.Log.LogInfo($"{other.name} - {other.tag}");
                return;
            }
            //PropHuntMod.Log.LogInfo($"{other.name} - {other.tag}");
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
