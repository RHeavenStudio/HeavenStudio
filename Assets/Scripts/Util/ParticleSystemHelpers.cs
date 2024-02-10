using UnityEngine;

namespace HeavenStudio.Util
{
    public static class ParticleSystemHelpers
    {
        public static void PlayScaledAsync(this ParticleSystem particleSystem, float timeScale)
        {
            ParticleSystem.MainModule main = particleSystem.main;
            main.simulationSpeed = (1 / Conductor.instance.pitchedSecPerBeat) * timeScale;
            particleSystem.Play();
        }

        public static void SetAsyncScaling(this ParticleSystem particleSystem, float timeScale)
        {
            ParticleSystem.MainModule main = particleSystem.main;
            main.simulationSpeed = (1 / Conductor.instance.pitchedSecPerBeat) * timeScale;
        }

        public static void PlayScaledAsyncAllChildren(this ParticleSystem particleSystem, float timeScale)
        {
            particleSystem.PlayScaledAsync(timeScale);
            
            foreach (var p in particleSystem.GetComponentsInChildren<ParticleSystem>())
            {
                if (p == particleSystem) continue;
                p.PlayScaledAsyncAllChildren(timeScale);
            }
        }
    }
}

