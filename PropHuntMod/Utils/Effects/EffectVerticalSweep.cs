using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Events;

namespace PropHuntMod.Utils
{
    internal class EffectVerticalSweep : MonoBehaviour
    {
        public int sweepCount = 3;
        public float duration = 1f;
        public float delayBetweenSweeps = 0.3f;
        public Action onComplete;

        SpriteRenderer[] renderers;
        MaterialPropertyBlock block;

        static int MinY;
        static int MaxY;
        static int SweepID;

        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Awake()
        {
            renderers = GetComponentsInChildren<SpriteRenderer>();
            block = new MaterialPropertyBlock();

            foreach (var r in renderers)
            {
                r.material = new Material(EffectsManager.SweepShader);
            }

            MinY = Shader.PropertyToID("_BoundsMinY");
            MaxY = Shader.PropertyToID("_BoundsMaxY");
            SweepID = Shader.PropertyToID("_SweepPosition");

            ComputeAndApplyBounds();
        }

        void ComputeAndApplyBounds()
        {
            float minY = float.PositiveInfinity;
            float maxY = float.NegativeInfinity;

            foreach (var r in renderers)
            {
                Bounds b = r.bounds;
                minY = Mathf.Min(minY, b.min.y);
                maxY = Mathf.Max(maxY, b.max.y);
            }

            foreach (var r in renderers)
            {
                r.GetPropertyBlock(block);
                block.SetFloat(MinY, minY);
                block.SetFloat(MaxY, maxY);
                r.SetPropertyBlock(block);
            }
        }

        void Start()
        {
            Play();
        }

        void Play()
        {
            StopAllCoroutines();
            StartCoroutine(Sweep());
        }

        IEnumerator Sweep()
        {

            for (int i = 0; i < sweepCount; i++)
            {
                float t = 0f;

                while (t < duration)
                {
                    t += Time.deltaTime;
                    float value = Mathf.Clamp01(t / duration);
                    SetSweep(value);
                    yield return null;
                }

                SetSweep(1);

                if (i < sweepCount - 1)
                {
                    yield return new WaitForSeconds(delayBetweenSweeps);
                    SetSweep(0);
                }

            }

            onComplete?.Invoke();

            Debug.Log("Sweep finished");
        }

        // Update is called once per frame
        void SetSweep(float value)
        {
            foreach (var r in renderers)
            {
                r.GetPropertyBlock(block);
                block.SetFloat(SweepID, value);
                r.SetPropertyBlock(block);
            }
        }
    }
}
