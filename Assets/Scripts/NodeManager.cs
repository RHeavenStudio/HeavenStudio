using Kino;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;

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
        public PixelArtFilter pixelizer;
        public DigitalGlitch digitalGlitch;
        public AnalogGlitch analogGlitch;
        public Tube crt;

        private void Start()
        {
            pixelizer = GameCamera.GetCamera().GetComponent<PixelArtFilter>();
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
            var songPosBeats = Conductor.instance.songPositionInBeats;

            chromaticAberration.intensity.value =   GetIntensityFromNodes(songPosBeats, NodeType.Chromatic_Aberration) * 0.1f;
            vignette.intensity.value =              GetIntensityFromNodes(songPosBeats, NodeType.Vignette) * 0.01f;
            bloom.intensity.value =                 GetIntensityFromNodes(songPosBeats, NodeType.Bloom) * 0.5f;
            grain.intensity.value =                 GetIntensityFromNodes(songPosBeats, NodeType.Grain) * 0.25f;
            lensDistortion.intensity.value =        GetIntensityFromNodes(songPosBeats, NodeType.Lens_Distortion);
            pixelizer.downSamples =                 Mathf.Lerp(0, 8, Mathf.Clamp01(GetIntensityFromNodes(songPosBeats, NodeType.Pixelize) / 100f));

            digitalGlitch.intensity =               GetIntensityFromNodes(songPosBeats, NodeType.Glitch) * 0.01f;
            analogGlitch.scanLineJitter =           GetIntensityFromNodes(songPosBeats, NodeType.Jitter) * 0.01f;

            analogGlitch.enabled = (analogGlitch.scanLineJitter > 0.01f);

            crt.bleeding =                          GetIntensityFromNodes(songPosBeats, NodeType.CRT) * 0.01f;
            crt.fringing =                          crt.bleeding;
            crt.scanline =                          crt.bleeding;
        }

        private float GetIntensityFromNodes(float songPosBeats, NodeType type)
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
                    if (songPosBeats < allNodes[0].Beat)
                    {
                        keyframeBeat = 0.0f;
                        intensity = 0.0f;

                        nextKeyframeBeat = allNodes[0].Beat;
                        nextKeyframeIntensity = allNodes[0].Intensity;
                    }
                    else
                    {
                        if (songPosBeats >= nodeBeat)
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
                                nextKeyframeBeat = Conductor.instance.SongLengthInBeats();
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
