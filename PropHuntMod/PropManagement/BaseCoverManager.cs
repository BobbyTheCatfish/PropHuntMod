using UnityEngine;
using PropHuntMod.Networking.Client;
using PropHuntMod.Utils;
using GlobalEnums;
using System;
using PropHuntMod.Players;

namespace PropHuntMod.Props
{
    using PlayerID = UInt16;
    internal class BaseCoverManager
    {
        internal GameObject cover;
        internal string CoverPath;
        internal string PrevCoverPath;

        //internal string coverOGName = "";
        //public string currentScene;
        public PlayerID playerID;
        internal bool isRemote = true;
        internal Vector3 Position => cover?.transform.localPosition ?? Vector3.zero;
        internal float Rotation => cover?.transform.GetLocalRotation2D() ?? 0;
        internal float Scale { get; private set; } = 1;
        internal Vector2 OriginalScale { get; private set; } = Vector3.one;
        public bool IsHiding => cover != null;
        public void SetPropLocation(Vector3 location)
        {
            if (!IsHiding)
            {
                Log.LogError("No cover, can't set prop position");
                return;
            }

            cover.transform.localPosition = location;
        }

        public void SetPropRotation(float rotation)
        {
            if (!IsHiding)
            {
                Log.LogError("No cover, can't set prop rotation");
                return;
            }

            cover.transform.SetLocalRotation2D(rotation);
        }

        public void SetPropScale(float scale)
        {
            Scale = scale;
            if (!IsHiding)
            {
                Log.LogError("No cover, can't set prop scale");
                return;
            }

            cover.transform.SetScale2D(OriginalScale * scale);
        }

        public void SetPropLocation(Vector3 location, float rotation, float scale)
        {
            if (!IsHiding)
            {
                Log.LogError("No cover, can't set any prop location");
                return;
            }
            ConstrainPropLocation(ref location, ref rotation, ref scale);

            SetPropLocation(location);
            SetPropRotation(rotation);
            SetPropScale(scale);
        }
        public static void ConstrainPropLocation(ref Vector3 location, ref float rotation, ref float scale)
        {
            location.x = Mathf.Clamp(location.x, Consts.MIN_X, Consts.MAX_X);
            location.y = Mathf.Clamp(location.y, Consts.MIN_Y, Consts.MAX_Y);
            location.z = Mathf.Clamp(location.z, Consts.MIN_Z, Consts.MAX_Z);

            rotation %= 360;

            scale = Mathf.Clamp(scale, Consts.MIN_S, Consts.MAX_S);
        }

        public virtual bool DisableProp(BaseHornetManager manager, bool logOnFail = true)
        {
            if (!IsHiding)
            {
                if (logOnFail) Log.LogError("No cover to disable");
                return false;
            }

            Log.LogInfo($"Destroying prop {cover}");
            GameObject.Destroy(cover);
            cover = null;
            //coverOGName = "";
            manager.ToggleHornet(true);

            return true;
        }

        public void FindProp(BaseHornetManager manager)
        {
            if (!IsHiding)
            {
                Log.LogError("No cover to disable");
                return;
            }

            cover.transform.parent = null;
            manager.ToggleHornet(true);

            var coverCopy = cover.gameObject;
            cover = null;

            Component.Destroy(coverCopy.GetComponent<TriggerHandler>());
            EffectsManager.SweepGameObject(coverCopy, () => GameObject.Destroy(coverCopy));
        }

        public virtual bool EnableProp(BaseHornetManager hornet, Prop prop)
        {
            // Can't do anything
            if (prop == null)
            {
                Client.LocalMessage("There isn't any valid cover here. Try another room.");
                Log.LogError("No valid cover found");
                return false;
            }
            
            // Destroy old prop
            if (IsHiding)
            {
                Log.LogWarning("Destroying cover...");

                PrevCoverPath = CoverPath;

                GameObject.Destroy(cover);
                cover = null;
            }

            // Create prop, parent to hornet, and hide hornet
            if (!hornet.HornetExists()) return false;

            try
            {
                Log.LogInfo("Creating prop");

                if (PrevCoverPath == null)
                {
                    PrevCoverPath = prop.path;
                }

                cover = GameObject.Instantiate(prop.go, hornet.hornet.transform);
                cover.name = prop.name;
                CoverPath = prop.path;

                var sprite = cover.GetComponent<SpriteRenderer>();
                if (sprite != null)
                {
                    sprite.enabled = true;
                    sprite.maskInteraction = SpriteMaskInteraction.None;
                }

                OriginalScale = (Vector2)cover.transform.lossyScale;
                SetPropLocation(Vector3.zero, 0, 1);

                cover.SetActive(true);

                cover.layer = (int)PhysLayers.HERO_BOX;
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
            return true;
        }

        public virtual void OnHit(TriggerHandler handler)
        {
            //DisableProp(PlayerManager.GetPlayerManager(playerID).hornetManager);
            Log.LogInfo($"Found {playerID}");

            Component.Destroy(handler);
            ClientNetwork.SendPropFound(playerID);
            return;
        }
    }

    class TriggerHandler : MonoBehaviour
    {
        public PlayerID playerID;
        public bool isRemote;
        //void Awake()
        //{
            //Debug.Log("Hey, i'm on!");
        //}

        void OnTriggerEnter2D(Collider2D other)
        {
            if (!isRemote)
            {
                //Log.LogInfo($"own collider");
                //Log.LogInfo($"{other.name} - {other.tag}");
                return;
            }
            //Log.LogInfo($"{other.name} - {other.tag}");
            if (other.tag == "Nail Attack" && (Client.GameState == Utils.GameState.Playing ? Client.isSeeker : !SelfCoverManager.instance.IsHiding))
            {
                if (!other.GetComponentInParent<HeroController>()) return;
                PlayerManager.GetPlayerManager(playerID).coverManager.OnHit(this);
            }
        }
    }
}
