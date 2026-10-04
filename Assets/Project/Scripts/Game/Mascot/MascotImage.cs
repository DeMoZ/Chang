using Chang.Profile;
using UnityEngine;
using UnityEngine.UI;

namespace Chang.Mascot
{
    /// <summary>
    /// Shows a mascot look (or a color swatch) on a RawImage. The picture is rendered at the pixel size of the rect
    /// and again only when the look or the size changes. Renders are spread over frames, a few per frame.
    /// </summary>
    [RequireComponent(typeof(RawImage))]
    public class MascotImage : MonoBehaviour
    {
        private const int MaxRendersPerFrame = 3;
        private const int MaxTextureSize = 1024;

        [SerializeField] private MascotFraming framing = MascotFraming.Face;

        private static int _budgetFrame = -1;
        private static int _rendersThisFrame;

        private RawImage _image;
        private Texture2D _texture;
        private MascotLook _look;
        private string _swatch;
        private string _renderedSvg;
        private Vector2Int _renderedSize;

        private RawImage Image => _image != null ? _image : _image = GetComponent<RawImage>();

        public void Show(MascotLook look)
        {
            _look = look?.Clone();
            _swatch = null;
        }

        public void ShowSwatch(string color)
        {
            _swatch = color;
            _look = null;
        }

        private void Awake()
        {
            Image.enabled = _texture != null;
        }

        private void LateUpdate()
        {
            if (_look == null && _swatch == null)
            {
                return;
            }

            var size = PixelSize();
            if (size.x <= 0 || size.y <= 0)
            {
                return;
            }

            var aspect = size.x / (float)size.y;
            var svg = _swatch != null ? MascotSvg.Swatch(_swatch, aspect) : MascotSvg.Build(_look, framing, aspect);
            if (svg == _renderedSvg && size == _renderedSize)
            {
                return;
            }

            if (_budgetFrame != Time.frameCount)
            {
                _budgetFrame = Time.frameCount;
                _rendersThisFrame = 0;
            }

            if (_rendersThisFrame >= MaxRendersPerFrame)
            {
                return;
            }

            _rendersThisFrame++;
            var texture = MascotRenderer.Render(svg, size.x, size.y);
            DestroyTexture();
            _texture = texture;
            _renderedSvg = svg;
            _renderedSize = size;
            Image.texture = _texture;
            Image.enabled = true;
        }

        private Vector2Int PixelSize()
        {
            var rect = ((RectTransform)transform).rect;
            var canvas = Image.canvas;
            var scale = canvas != null ? canvas.rootCanvas.scaleFactor : 1f;
            var width = Mathf.Min(MaxTextureSize, Mathf.CeilToInt(rect.width * scale));
            var height = Mathf.Min(MaxTextureSize, Mathf.CeilToInt(rect.height * scale));
            return new Vector2Int(width, height);
        }

        private void OnDestroy()
        {
            DestroyTexture();
        }

        private void DestroyTexture()
        {
            if (_texture != null)
            {
                Destroy(_texture);
                _texture = null;
            }
        }
    }
}
