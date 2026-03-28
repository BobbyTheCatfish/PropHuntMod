using HarmonyLib;
using PropHuntMod.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using TeamCherry.Localization;
using UnityEngine;

namespace PropHuntMod.Utils
{
    internal static class EffectsManager
    {
        static AudioClip victorySound;
        static AudioClip otherRevealSound;
        static AudioClip selfRevealSound;
        static AudioClip gameOverSound;
        static ParticleSystem confetti;
        public static Shader SweepShader;
        public static void PlayFoundSound(bool isSelf)
        {
            if (selfRevealSound == null) Init();

            PlaySound(isSelf ? selfRevealSound : otherRevealSound);
        }

        public static void PlayGameOverSound(bool isWinner)
        {
            if (selfRevealSound == null) Init();

            PlaySound(gameOverSound);

            if (isWinner)
            {
                PlaySound(victorySound);
            }
        }

        static void PlaySound(AudioClip audio)
        {
            Log.LogDebug($"Playing {audio.name}");

            var source = HeroController.instance.GetComponent<AudioSource>();
            source.PlayOneShot(audio);
        }

        public static void SetTitle(string main, string sub = "", string super = "Prop Hunt", bool large = true, float time = 4.75f)
        {
            // Set the FSM settings
            var title = GameCameras.instance.hudCamera.transform.Find("In-game/Area Title");
            var fsm = title.GetComponent<PlayMakerFSM>();
            var eventName = "PROPHUNT_TITLE";
            
            FSMUtility.SetBool(fsm, "NPC Title", false);
            FSMUtility.SetBool(fsm, "Visited", !large);
            FSMUtility.SetString(fsm, "Area Event", eventName);
            FSMUtility.SetFloat(fsm, "Unvisited Wait", time);

            // Add to the language
            var sheets = Traverse.Create(typeof(Language)).Field("_currentEntrySheets");
            var sheetContent = sheets.GetValue<Dictionary<string, Dictionary<string, string>>>();

            sheetContent["Titles"][$"{eventName}_MAIN"] = main;
            sheetContent["Titles"][$"{eventName}_SUB"] = sub;
            sheetContent["Titles"][$"{eventName}_SUPER"] = super;

            // Enable title
            title.Find("Title Large/Title_Fleur_Bot").localPosition = new Vector3(0, -5.92f, 0);

            // toggle off then on in order to clear current title
            title.gameObject.SetActive(false);
            PropHuntMod.nextFrameActions.Add(() => title.gameObject.SetActive(true));
        }

        public static void PlayConfetti()
        {
            if (confetti == null) Init();
            confetti.Play();
        }

        public static void SweepGameObject(GameObject go, Action onFinish)
        {
            if (SweepShader == null) Init();

            var sweep = go.AddComponent<EffectVerticalSweep>();
            sweep.onComplete = onFinish;
        }

        // Not tested or implemented yet
        static void ActionOnBundle(string bundleName, string bundleFile, Action<AssetBundle> action)
        {
            var loadedBundles = AssetBundle.GetAllLoadedAssetBundles();
            var bundle = loadedBundles.FirstOrDefault(b => b.name == bundleName);

            bool unload = false;
            if (bundle == null)
            {
                Log.LogDebug("Bundle not loaded. Loading now.");
                var bundlePath = Path.Combine(bundleFile);
                bundle = AssetBundle.LoadFromFile(bundlePath);
                unload = true;
            }

            if (bundle == null)
            {
                Log.LogError($"Unable to load bundle {bundleName}");
                return;
            }

            action.Invoke(bundle);

            if (unload) bundle.Unload(false);
        }

        static void Init()
        {
            /****************
             *  LOAD AUDIO  *
             ****************/
            var loadedBundles = AssetBundle.GetAllLoadedAssetBundles();
            var bundle = loadedBundles.First(b => b.name == "7aa551f2bad7d3e8e7893d04de5ef978.bundle");

            if (bundle == null)
            {
                Log.LogWarning("Couldn't load audio bundle");
                return;
            }

            var audioClips = bundle.LoadAllAssets<AudioClip>();
            victorySound = audioClips.FirstOrDefault(a => a.name == "sl3");
            otherRevealSound = audioClips.FirstOrDefault(a => a.name == "Garama_weak_collapse");
            selfRevealSound = audioClips.FirstOrDefault(a => a.name == "d3");

            bundle = loadedBundles.First(b => b.name == "48a0f4259d782cbbf6fb20cdcc4f4e5f.bundle");
            gameOverSound = bundle.LoadAllAssets<AudioClip>().FirstOrDefault(a => a.name == "slow_motion_effect_tone_with_texture");

            Log.LogDebug("Audio loaded");

            //Alternate game over sounds
            //audioBundle = loadedBundles.First(b => b.name == "45160b0885b9207aade8da6c49b4c729.bundle");
            //gameOverSound = audioBundle.LoadAllAssets<AudioClip>().FirstOrDefault(a => a.name == "unravelled_boss_bg_head_dissapear");

            //audioBundle = loadedBundles.First(b => b.name == "9ebdb0e6cfbf616e44feed59c02848ad.bundle");
            //gameOverSound = audioBundle.LoadAllAssets<AudioClip>().FirstOrDefault(a => a.name ==  "dream_enter_pt_2");


            /******************************
             *  LOAD CUSTOM ASSET BUNDLE  *
             ******************************/
            var dir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            
            bundle = AssetBundle.LoadFromFile(Path.Combine(dir, "prophunt.bundle"));
            var assets = bundle.LoadAllAssets();

            //foreach (var  asset in assets)
            //{
            //    Log.LogInfo(asset.name, asset);
            //}

            /******************************
             *  LOAD SWEEP EFFECT SHADER  *
             ******************************/
            var shader = assets.First(a => a is Shader && a.name == "Unlit/SweeperNew") as Shader;
            if (shader == null)
            {
                Log.LogError("SweepShader shader is null");
                return;
            }
            SweepShader = shader;

            Log.LogDebug("Sweep shader loaded");


            /**************************
             *  LOAD CONFETTI PREFAB  *
             **************************/

            GameObject confettiGO = assets.First(a => a is GameObject && a.name == "Confetti") as GameObject;
            var main = confettiGO.GetComponent<ParticleSystem>().main;
            main.playOnAwake = false;

            confettiGO = GameObject.Instantiate(confettiGO, GameCameras.instance.tk2dCam.transform);
            confettiGO.transform.localPosition = new Vector3(0, -7.5f, 25);

            confetti = confettiGO.GetComponent<ParticleSystem>();

            bundle.Unload(false);


            bundle = loadedBundles.FirstOrDefault(b => b.name == "c3803556c9d1f4d00ecf06fd8fbe45f0.bundle");

            bool unload = false;
            if (bundle == null)
            {
                Log.LogDebug("Bundle not loaded. Loading now.");
                var bundlePath = Path.Combine(BepInEx.Paths.ManagedPath, "../", "StreamingAssets", "aa", "StandaloneWindows64", "materials_assets_areaaqueductsprintmaster.bundle");
                bundle = AssetBundle.LoadFromFile(bundlePath);
                unload = true;
            }

            Log.LogDebug("bundle loaded");

            var mat = bundle.LoadAsset<Material>("Assets/Materials/Particles/Confetti Particle.mat");
            //var mat = materials.First(a => a.name.ToLower() == "confetti particle");
            confettiGO.GetComponent<ParticleSystemRenderer>().material = mat;

            if (unload) bundle.Unload(false);
            Log.LogDebug("Confetti loaded");
        }
    }
}
