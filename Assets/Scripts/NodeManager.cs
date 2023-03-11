using UnityEngine;
using UnityEngine.Rendering.PostProcessing;

using Kino;
using System.Collections.Generic;

namespace HeavenStudio
{
    public class NodeManager : MonoBehaviour
    {
        public PostProcessVolume volume;
        public ChromaticAberration chromaticAberration;
        public Vignette vignette;
        public Bloom bloom;
        public Grain grain;
        public LensDistortion lensDistortion;

        public DigitalGlitch digitalGlitch;
        public AnalogGlitch analogGlitch;
        public Tube crt;

        private void Start()
        {
            digitalGlitch = GameCamera.GetCamera().GetComponent<DigitalGlitch>();
            analogGlitch = GameCamera.GetCamera().GetComponent<AnalogGlitch>();
            crt = GameCamera.GetCamera().GetComponent<Tube>();

            volume = GameCamera.GetCamera().GetComponent<PostProcessVolume>();

            volume.profile.TryGetSettings(out chromaticAberration);
            volume.profile.TryGetSettings(out vignette);
            volume.profile.TryGetSettings(out bloom);
            volume.profile.TryGetSettings(out grain);
            volume.profile.TryGetSettings(out lensDistortion);
        }

        private void Update()
        {
            chromaticAberration.intensity.value = GetIntensityFromNodes(Nodes.NodeType.Chromatic_Aberration) * 0.25f;
            vignette.intensity.value = GetIntensityFromNodes(Nodes.NodeType.Vignette) * 0.01f;
            bloom.intensity.value = GetIntensityFromNodes(Nodes.NodeType.Bloom) * 0.5f;
            grain.intensity.value = GetIntensityFromNodes(Nodes.NodeType.Grain) * 0.25f;
            lensDistortion.intensity.value = GetIntensityFromNodes(Nodes.NodeType.Lens_Distortion);

            digitalGlitch.intensity = GetIntensityFromNodes(Nodes.NodeType.Glitch) * 0.01f;
            analogGlitch.scanLineJitter = GetIntensityFromNodes(Nodes.NodeType.Jitter) * 0.01f;

            crt.bleeding = GetIntensityFromNodes(Nodes.NodeType.CRT) * 0.01f;
            crt.fringing = crt.bleeding;
            crt.scanline = crt.bleeding;
        }

        private float GetIntensityFromNodes(Nodes.NodeType type)
        {
            var intensity = 0.0f;
            var keyframeBeat = 0.0f;

            var nextKeyframeBeat = 0.0f;
            var nextKeyframeIntensity = 0.0f;

            var allNodes = GameManager.instance.Beatmap.nodes.FindAll(c => c.Type == type);
            for (int i = 0; i < allNodes.Count; i++)
            {
                var node = allNodes[i];
                var nodeBeat = node.Beat;
                var nodeIntensity = node.Intensity;

                if (allNodes.Count > 0)
                {
                    if (Conductor.instance.songPositionInBeats < allNodes[0].Beat)
                    {
                        keyframeBeat = 0.0f;
                        intensity = 0.0f;

                        nextKeyframeBeat = allNodes[0].Beat;
                        nextKeyframeIntensity = allNodes[0].Intensity;
                    }
                    else
                    {
                        if (Conductor.instance.songPositionInBeats >= nodeBeat)
                        {
                            keyframeBeat = nodeBeat;
                            intensity = nodeIntensity;

                            if (i + 1 < allNodes.Count)
                            {
                                nextKeyframeBeat = allNodes[i + 1].Beat;
                                nextKeyframeIntensity = allNodes[i + 1].Intensity;
                            }
                            else
                            {
                                nextKeyframeBeat = 320;
                                nextKeyframeIntensity = 0.0f;
                            }
                        }
                    }
                }
                else
                {
                    keyframeBeat = 0.0f;
                    intensity = 0.0f;
                    nextKeyframeBeat = 0.0f;
                    nextKeyframeIntensity = 0.0f;
                }
            }

            var ret = Mathf.Lerp(intensity, nextKeyframeIntensity, Conductor.instance.GetPositionFromBeat(keyframeBeat, nextKeyframeBeat - keyframeBeat));
            return (float.IsNaN(ret)) ? 0.0f : ret;
        }
    }
}
