using HarmonyLib;
using HutongGames.PlayMaker;
using HutongGames.PlayMaker.Actions;
using PropHuntMod.Utils;
using SSMP.Util;
using System.Collections.Generic;
using System.Linq;
using TeamCherry.Localization;
using UnityEngine;

namespace PropHuntMod.Modifications
{
    internal static class EffectsManager
    {
        static AudioClip victorySound;
        static AudioClip otherRevealSound;
        static AudioClip selfRevealSound;
        static AudioClip gameOverSound;
        public static void PlayFoundSound(bool isSelf)
        {
            Log.LogInfo("Playing found sound");
            if (selfRevealSound == null) Init();

            PlaySound(isSelf ? selfRevealSound : otherRevealSound);
        }

        public static void PlayGameOverSound(bool isWinner)
        {
            Log.LogInfo("Playing game over sound");
            if (selfRevealSound == null) Init();

            PlaySound(gameOverSound);

            if (isWinner)
            {
                PlaySound(victorySound);
            }
            Log.LogInfo("Playing win sound");

        }

        static void PlaySound(AudioClip audio)
        {
            Log.LogInfo($"Playing {audio.name}");

            var source = HeroController.instance.GetComponent<AudioSource>();
            source.PlayOneShot(audio);
        }

        public static void SetTitle(string main, string sub = "", string super = "Prop Hunt", bool large = true)
        {
            // Set the FSM settings
            var title = GameCameras.instance.hudCamera.transform.Find("In-game/Area Title");
            var fsm = title.GetComponent<PlayMakerFSM>();
            var eventName = "PROPHUNT_TITLE";
            
            FSMUtility.SetBool(fsm, "NPC Title", false);
            FSMUtility.SetBool(fsm, "Visited", !large);
            FSMUtility.SetString(fsm, "Area Event", eventName);

            // Add to the language
            var sheets = Traverse.Create(typeof(Language)).Field("_currentEntrySheets");
            var sheetContent = sheets.GetValue<Dictionary<string, Dictionary<string, string>>>();

            sheetContent["Titles"][$"{eventName}_MAIN"] = main;
            sheetContent["Titles"][$"{eventName}_SUB"] = sub;
            sheetContent["Titles"][$"{eventName}_SUPER"] = super;

            // Enable title
            title.Find("Title Large/Title_Fleur_Bot").localPosition = new Vector3(0, -5.92f, 0);
            title.gameObject.SetActive(true);
        }

        static void Init()
        {
            /****************
             *  LOAD AUDIO  *
             ****************/
            var audioBundle = AssetBundle.GetAllLoadedAssetBundles().First(b => b.name == "7aa551f2bad7d3e8e7893d04de5ef978.bundle");
            //var audioBundle = AssetBundle.LoadFromFile(bundlePath);

            if (audioBundle == null)
            {
                Log.LogInfo("Couldn't load audio bundle");
                return;
            }

            var audioClips = audioBundle.LoadAllAssets<AudioClip>();
            victorySound = audioClips.FirstOrDefault(a => a.name == "sl3");
            otherRevealSound = audioClips.FirstOrDefault(a => a.name == "Garama_weak_collapse");
            selfRevealSound = audioClips.FirstOrDefault(a => a.name == "d3");

            audioBundle = AssetBundle.GetAllLoadedAssetBundles().First(b => b.name == "9ebdb0e6cfbf616e44feed59c02848ad.bundle");
            gameOverSound = audioBundle.LoadAllAssets<AudioClip>().FirstOrDefault(a => a.name ==  "dream_enter_pt_2");
        }
    }
}
