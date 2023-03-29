// KinoTube - Old TV/video artifacts simulation
// https://github.com/keijiro/KinoTube

using UnityEngine;

namespace Kino
{
    [ExecuteInEditMode]
    public class Tube : MonoBehaviour
    {
        #region Public

        public float bleeding { get { return _bleeding; } set { _bleeding = value; } }
        public float fringing { get { return _fringing; } set { _fringing = value; } }
        public float scanline { get { return _scanline; } set { _scanline = value; } }

        #endregion

        #region Editable attributes

        [SerializeField, Range(0, 1)] float _bleeding = 0.5f;
        [SerializeField, Range(0, 1)] float _fringing = 0.5f;
        [SerializeField, Range(0, 1)] float _scanline = 0.5f;

        #endregion

        #region Private resources

        [SerializeField] Shader _shader;
        Material _material;

        #endregion

        #region MonoBehaviour methods

        void OnDestroy()
        {
            if (_material == null)
            {
                if (Application.isPlaying)
                    Destroy(_material);
                else
                    DestroyImmediate(_material);
            }
        }

        void OnRenderImage(RenderTexture source, RenderTexture dest)
        {
            if (_material == null)
            {
                _material = new Material(_shader);
                _material.hideFlags = HideFlags.DontSave;
            }

            var bleedWidth = 0.04f * _bleeding;  // width of bleeding
            var bleedStep = 2.5f / source.width; // max interval of taps
            var bleedTaps = Mathf.CeilToInt(bleedWidth / bleedStep);
            var bleedDelta = bleedWidth / bleedTaps;
            var fringeWidth = 0.0025f * _fringing; // width of fringing

            _material.SetInt("_BleedTaps", bleedTaps);
            _material.SetFloat("_BleedDelta", bleedDelta);
            _material.SetFloat("_FringeDelta", fringeWidth);
            _material.SetFloat("_Scanline", _scanline);

            Graphics.Blit(source, dest, _material, 0);
        }

        #endregion
    }
}
