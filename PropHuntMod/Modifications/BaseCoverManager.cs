using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using PropHuntMod.Utils.Networking;
using PropHuntMod.Utils;
//using SilksongMultiplayer.NetworkData;
using GlobalEnums;
using System;
using SSMP.Api.Client;

namespace PropHuntMod.Modifications
{
    using PlayerID = CSteamID;
    internal class BaseCoverManager
    {
        internal GameObject cover;
        internal string coverOGName = "";
        //public string currentScene;
        public PlayerID playerID;
        internal bool isRemote = true;
        public void SetPropLocation(Vector3 location)
        {
            if (cover == null)
            {
                Log.LogError("No cover, can't set prop position");
                return;
            }

            cover.transform.position = location;
        }

        public void SetPropLocation(float rotation)
        {
            if (cover == null)
            {
                Log.LogError("No cover, can't set prop rotation");
                return;
            }

            var r = cover.transform.rotation;
            cover.transform.rotation = new Quaternion(rotation, r.y, r.z, r.w);
        }

        public void SetPropLocation(Vector3 location, float rotation)
        {
            SetPropLocation(location);
            SetPropLocation(rotation);
        }

        public bool IsCovered()
        {
            return cover != null;
        }

        public void DisableProp(HornetManager manager, bool logOnFail = true)
        {
            if (cover == null)
            {
                if (logOnFail) Log.LogError("No cover to disable");
                return;
            }
            GameObject.Destroy(cover);
            cover = null;
            coverOGName = "";
            manager.ToggleHornet(true);


            if (!isRemote)
            {
                Network.SendPropSwap("");
            }
        }

        public void EnableProp(HornetManager hornet, GameObject cover)
        {
            // Can't do anything
            if (cover == null)
            {
                Log.LogError("No valid cover found");
                return;
            }
            
            if (this.cover != null)
            {
                Log.LogWarning("Destroying cover...");
                GameObject.Destroy(this.cover);
                this.cover = null;
            }
            
            // Create prop, parent to hornet, and hide hornet
            if (hornet.hornet == null) hornet.SetHornet();
            var transform = hornet.hornet.transform;

            try
            {
                Log.LogInfo("Creating prop");
                this.cover = GameObject.Instantiate(cover, transform.position, transform.rotation, transform);
                cover.SetActive(true);
                coverOGName = cover.name;

                this.cover.layer = (int)PhysLayers.HERO_BOX;
                hornet.ToggleHornet(false);

                var handler = this.cover.GetComponent<TriggerHandler>();
                handler.playerID = playerID;
                handler.isRemote = isRemote;
            }
            catch (Exception e)
            {
                Log.LogError("Ran into an error instantiating cover.");
                Log.LogError(e);
            }

            Log.LogInfo($"{this.cover.name} - {this.cover.layer} - {this.cover.activeInHierarchy}");

            if (!isRemote) Network.SendPropSwap(cover.name);
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
        //            //Log.LogInfo($"{gameObject.name} - {gameObject.layer} MAYBE");
                    
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
        //                    Log.LogInfo("Getting sprite renderer");
        //                    try
        //                    {
        //                        //Log.LogInfo(renderer);
        //                        Log.LogInfo(renderer.sprite);
        //                        //Log.LogInfo(renderer.sprite.texture);
        //                        //Log.LogInfo(renderer.sprite.texture.name);
        //                        var ren = GetRenderer(o);
        //                        Log.LogInfo(ren.sprite);
        //                        if (ren == null || ren.sprite == null || renderer == null) return false;
        //                        return ren.sprite.name == renderer.sprite.name;
        //                    }
        //                    catch (Exception e)
        //                    {
        //                        Log.LogError($"{gameObject.name} failed, not a real object?");
        //                        Log.LogError(e);
        //                        return true;
        //                    }
        //                }))
        //                {
        //                    continue;
        //                }

        //                Log.LogInfo($"{gameObject.name} - {gameObject.layer} YES");
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
            DisableProp(PlayerManager.GetPlayerManager(playerID).hornetManager);
            Log.LogInfo($"Found {playerID}");
            Network.SendPropFound(playerID);
            return;
        }
    }

    class TriggerHandler : MonoBehaviour
    {
        public PlayerID playerID;
        public bool isRemote;
        void Awake()
        {
            Debug.Log("Hey, i'm on!");
        }

        void OnTriggerEnter2D(Collider2D other)
        {
            if (!isRemote)
            {
                //Log.LogInfo($"own collider");
                //Log.LogInfo($"{other.name} - {other.tag}");
                return;
            }
            //Log.LogInfo($"{other.name} - {other.tag}");
            if (other.tag == "Nail Attack" && !PropHuntMod.cover.IsCovered())
            {
                PlayerManager.GetPlayerManager(playerID).coverManager.OnHit();
            }
        }
    }
}
