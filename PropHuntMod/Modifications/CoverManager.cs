using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.SceneManagement;
using NoRepeat;
using PropHuntMod.Utils.Networking;
using Steamworks;
using SilksongMultiplayer.NetworkData;
using GlobalEnums;

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

        public void DisableProp(HornetManager manager)
        {
            if (cover == null)
            {
                PropHuntMod.Log.LogError("No cover to disable");
                return;
            }
            GameObject.Destroy(cover);
            cover = null;
            manager.ToggleHornet(true);

            if (!PlayerManager.IsRemotePlayer(steamID)) PacketSend.SendPropSwap("");
        }
        public void EnableProp(HornetManager hornet, GameObject cover)
        {
            if (cover == null)
            {
                PropHuntMod.Log.LogError("No valid cover found");
                return;
            }

            var transform = hornet.hornet.transform;
            this.cover = GameObject.Instantiate(cover, transform.position, transform.rotation, transform);
            PropHuntMod.Log.LogInfo($"{this.cover.name} - {this.cover.layer} - {this.cover.activeInHierarchy}");


            var scripts = this.cover.GetComponentsInChildren<MonoBehaviour>();
            foreach (var component in scripts)
            {
                if (component.GetType() != typeof(tk2dSprite) && component.GetType() != typeof(tk2dSpriteAnimator))
                {
                    //PropHuntMod.Log.LogInfo(component.tag);
                    Component.Destroy(component);
                }
            }

            var colliders = this.cover.GetComponentsInChildren<Collider2D>();
            foreach (var component in colliders)
            {
                Component.Destroy(component);
            }

            var physics = this.cover.GetComponentsInChildren<Rigidbody2D>();
            foreach (var collider in physics)
            {
                Component.Destroy(collider);
            }

            if (!PlayerManager.IsRemotePlayer(steamID)) PacketSend.SendPropSwap(this.cover.name);

            //cover.transform.SetPositionZ(existing.transform.position.z);
            hornet.ToggleHornet(false);
        }
        public void EnableProp(HornetManager hornet)
        {
            if (cover)
            {
                GameObject.Destroy(cover);
                cover = null;
            }

            if (applicableCovers == null)
            {
                applicableCovers = new NoRepeat<GameObject>(GetAllProps());
            }

            var newCover = applicableCovers.GetRandom();
            EnableProp(hornet, newCover);
        }

        private int[] invalidLayers = { 11, 17 };
        private List<GameObject> GetAllProps()
        {
            GameObject[] allGameObjects = Resources.FindObjectsOfTypeAll<GameObject>();
            List<GameObject> props = new List<GameObject>();
            foreach (GameObject gameObject in allGameObjects)
            {
                if (
                    HasScript(gameObject)
                    //|| (gameObject.name.ToLower().Contains("pilgrim"))
                    || (gameObject.name.ToLower().Contains("corpse"))
                    // || gameObject.layer == 19
                    || gameObject.tag == "RespawnPoint"
                )
                {
                    //PropHuntMod.Log.LogInfo($"{gameObject.name} - {gameObject.layer} MAYBE");
                    var renderer = gameObject.GetComponent<SpriteRenderer>();
                    if (
                        ((renderer && renderer.enabled) || gameObject.GetComponent<tk2dSprite>())
                        && !invalidLayers.Contains(gameObject.layer)
                        && gameObject.activeInHierarchy
                        && !Regex.IsMatch(gameObject.name, "\\(\\d+\\) ?(\\(Clone\\))?$")
                    )
                    {
                        //PropHuntMod.Log.LogInfo($"{gameObject.name} - {gameObject.layer} YES");
                        props.Add(gameObject);
                    }
                }
            }

            return props;
        }

        private bool HasScript(GameObject obj)
        {
            return (
                obj.GetComponent<Breakable>()
                || obj.GetComponent<PlayMakerNPC>()
                || obj.GetComponent<BasicNPC>()
                || obj.GetComponent<QuestBoardInteractable>()
                || obj.GetComponent<PushableRubble>()
                || obj.GetComponent<GrassCut>()
            );
        }
    }
}
