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
    }
}

