using BepInEx;
using BepInEx.Logging;
using GenericVariableExtension;
using HarmonyLib;
using HarmonyLib.Tools;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;
using UnityEditor;
using UnityEngine;



[BepInPlugin("com.bobbythecatfish.prophunt", "Prop Hunt", "0.1.0")]
[BepInProcess("Hollow Knight Silksong.exe")]
public class PropHuntMod : BaseUnityPlugin
{
    
    GameObject hornet;
    GameObject cover;
    Props props = new Props();
    bool show = true;
    Vector2 ogHitboxSize;
    Vector2 currentHitboxSize;
    BoxCollider2D hitbox;

    private void ToggleHide()
    {
        tempLog("Toggling Hornet");
        setHornet();
        var render = hornet.GetComponent<MeshRenderer>();
        render.enabled = !show;
        show = !show;
    }

    private void Awake()
    {
        tempLog("Prop Hunt Loaded.");
        props.Initialize();
        Harmony.CreateAndPatchAll(typeof(PropHuntMod), null);
    }

    private void setHornet()
    {
        hornet = GameObject.FindGameObjectWithTag("Player");
        tempLog(hornet.name);
        if (hitbox == null)
        {
            //hitbox = hornet.GetComponent<BoxCollider2D>();
            var hitboxObj = GameObject.FindGameObjectWithTag("HeroBox");
            tempLog(hitboxObj.name);
            hitbox = hitboxObj.GetComponent<BoxCollider2D>();
            Console.WriteLine(hitbox.size);
            ogHitboxSize = hitbox.size;
        }
    }

    private void Update()
    {
        // For some reason, the game updates the collision size automatically. not sure if theres a better way (or if i even want to do this at all)
        if (hitbox != null && currentHitboxSize != null && hitbox.size.x != currentHitboxSize.x)
        {
            hitbox.size = new Vector3(currentHitboxSize.x, currentHitboxSize.y);
        }

        // TOGGLE VISIBILITY
        if (Input.GetKeyDown(KeyCode.H))
        {
            ToggleHide();
            return;
        }
        if (Input.GetKeyDown(KeyCode.P))
        {

            setHornet();
            var position = hornet.transform;
            Console.WriteLine(position.localPosition);

            if (cover != null)
            {
                GameObject.Destroy(cover);
            }

            var prop = props.boneBottom.GetRandomElement();
            var existing = GameObject.Find(prop.name);
            if (existing == null)
            {
                tempLog($"Couldn't get GameObject {prop.name}");
                return;
            }
            cover = GameObject.Instantiate(existing, hornet.transform.position, hornet.transform.rotation, hornet.transform);
            cover.transform.SetPositionX(cover.transform.position.x + prop.positionOffset.x);
            cover.transform.SetPositionY(cover.transform.position.y + prop.positionOffset.y);
            cover.transform.SetPositionZ(existing.transform.position.z);


            var size = GetTotalSize(cover);
            //var sprite = cover.GetComponent<SpriteRenderer>();
            //if (!sprite)
            //{
            //    tempLog($"Couldn't get sprite for {cover.name}");
            //    return;
            //}
            //var size = sprite.sprite.rect.size;
            currentHitboxSize = new Vector3(size.x, size.y);
            hitbox.size = currentHitboxSize;


            //GameObject[] allGameObjects = Resources.FindObjectsOfTypeAll<GameObject>();

            //tempLog("--- Tags ---");
            //if (allGameObjects.Length > 0)
            //{
            //    foreach (GameObject obj in allGameObjects)
            //    {
            //        tempLog($"- {obj.name} ({obj.tag})");
            //    }
            //}
        }
        if (Input.GetKey(KeyCode.Keypad2)) MoveProp(Direction.Down);
        if (Input.GetKey(KeyCode.Keypad4)) MoveProp(Direction.Left);
        if (Input.GetKey(KeyCode.Keypad6)) MoveProp(Direction.Right);
        if (Input.GetKey(KeyCode.Keypad8)) MoveProp(Direction.Up);
    }

    private void tempLog(string msg)
    {
        Console.WriteLine(msg);
    }

    private Vector2 GetTotalSize(GameObject cover)
    {
        var sprites = cover.GetComponentsInChildren<SpriteRenderer>();
        if (sprites.Length == 0)
        {
            tempLog($"No renderers on {cover.name}");
            return Vector2.zero;
        }

        var totalBounds = sprites[0].bounds;
        foreach (var sprite in sprites)
        {
            tempLog(sprite.name);
            totalBounds.Encapsulate(sprite.bounds);
        }
        return (Vector2)totalBounds.size;
    }
    enum Direction { Left, Right, Up, Down };
    private void MoveProp(Direction direction, float distance = .1f)
    {
        if (!cover)
        {
            tempLog("No cover");
            return;
        }

        var x = cover.transform.position.x;
        var y = cover.transform.position.y;
        if (direction == Direction.Left) x -= distance;
        else if (direction == Direction.Right) x += distance;
        else if (direction == Direction.Up) y += distance;
        else if (direction == Direction.Down) y -= distance;
        else
        {
            tempLog("Invalid direction");
            return;
        }

        cover.transform.position = new Vector3(x, y, cover.transform.position.z);
    }
}