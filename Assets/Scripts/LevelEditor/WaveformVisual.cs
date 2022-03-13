using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace HeavenStudio.Editor
{
    public class WaveformVisual : MonoBehaviour
    {
        public new AudioSource audio;

        public RawImage image;

        public int width;

        public Color col;

        int resolution = 60;

        float[] waveForm;
        float[] samples;

        Texture2D texture;

        private void Start()
        {
            audio = Conductor.instance.musicSource;
            GetComponent<RectTransform>().sizeDelta = new Vector2(Conductor.instance.SongLengthInBeats(), GetComponent<RectTransform>().sizeDelta.y);
            texture = new Texture2D(width, 100, TextureFormat.RGBA32, false);
            CreateWaveForm();
        }

        // This two are from unity answer (I mixed up)
        public void CreateWaveForm()
        {
            resolution = audio.clip.frequency / resolution;

            samples = new float[audio.clip.samples * audio.clip.channels];
            audio.clip.GetData(samples, 0);

            int s = 0;
            while (s < samples.Length)
            {
                samples[s] = samples[s] * 0.5F;
                ++s;
            }
            audio.clip.SetData(samples, 0);

            waveForm = new float[(samples.Length / resolution)];

            for (int i = 0; i < waveForm.Length; i++)
            {
                waveForm[i] = 0;

                for (int ii = 0; ii < resolution; ii++)
                {
                    waveForm[i] += Mathf.Abs(samples[(i * resolution) + ii]);
                }

                waveForm[i] /= resolution;
            }

            MakeTexture(width, 100, waveForm, col);
        }

        public void MakeTexture(int width, int height, float[] waveform, Color col)
        {
            texture = new Texture2D(width, height, TextureFormat.RGBA32, false);

            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    texture.SetPixel(x, y, Color.black);
                }
            }

            for (int x = 0; x < waveform.Length; x++)
            {
                for (int y = 0; y <= waveform[x] * ((float)height * .75f); y++)
                {
                    texture.SetPixel(x, (height / 2) + y, col);
                    texture.SetPixel(x, (height / 2) - y, col);
                }
            }

            texture.Apply();

            image.texture = texture;
        }

        void Update()
        {
            //script from unity doc.
            float[] spectrum = new float[1024];

            AudioListener.GetSpectrumData(spectrum, 0, FFTWindow.Rectangular);

            /*for (int i = 1; i < spectrum.Length - 1; i++)
            {
                Debug.DrawLine(new Vector3(i - 1, spectrum[i] + 10, 0), new Vector3(i, spectrum[i + 1] + 10, 0), Color.red);
                Debug.DrawLine(new Vector3(i - 1, Mathf.Log(spectrum[i - 1]) + 10, 2), new Vector3(i, Mathf.Log(spectrum[i]) + 10, 2), Color.cyan);
                Debug.DrawLine(new Vector3(Mathf.Log(i - 1), spectrum[i - 1] - 10, 1), new Vector3(Mathf.Log(i), spectrum[i] - 10, 1), Color.green);
                Debug.DrawLine(new Vector3(Mathf.Log(i - 1), Mathf.Log(spectrum[i - 1]), 3), new Vector3(Mathf.Log(i), Mathf.Log(spectrum[i]), 3), Color.blue);
            }

            //script from unity answer
            for (int i = 0; i < waveForm.Length - 1; i++)
            {
                Vector3 sv = new Vector3(i * .01f, waveForm[i] * 10, 0);
                Vector3 ev = new Vector3(i * .01f, -waveForm[i] * 10, 0);

                Debug.DrawLine(sv, ev, Color.yellow);
            }*/

            int current = audio.timeSamples / resolution;
            current *= 2;

            Vector3 c = new Vector3(current * .01f, 0, 0);

            Debug.DrawLine(c, c + Vector3.up * 10, Color.white);
        }
    }
}