using System.IO;
using Unity.VectorGraphics;
using UnityEngine;
using UnityEngine.Rendering;

namespace Chang.Mascot
{
    /// <summary>
    /// Renders SVG markup into a texture at runtime (Unity's built-in vector graphics module):
    /// parse, tessellate, draw the mesh into an antialiased render texture and read it back.
    /// </summary>
    public static class MascotRenderer
    {
        private const int AntiAliasing = 4;

        private static readonly VectorUtils.TessellationOptions Tessellation = new()
        {
            // In SVG units (the mascot is ~150 wide). Finer sampling costs a lot more time for no visible gain.
            StepDistance = 100f,
            MaxCordDeviation = 0.1f,
            MaxTanAngleDeviation = 0.05f,
            SamplingStepSize = 0.1f
        };

        private static Material _material;
        private static Mesh _mesh;
        private static CommandBuffer _commands;

        /// <summary>A new texture (the caller destroys it) with the SVG viewBox stretched to <paramref name="width"/>×<paramref name="height"/>.</summary>
        public static Texture2D Render(string svg, int width, int height)
        {
            var scene = SVGParser.ImportSVG(new StringReader(svg), ViewportOptions.PreserveViewport);
            var geometry = VectorUtils.TessellateScene(scene.Scene, Tessellation, scene.NodeOpacity);

            _mesh ??= new Mesh { name = "Mascot" };
            VectorUtils.FillMesh(_mesh, geometry, 1f, false);

            // Vertex colors only, any sprite shader draws it (this one is always included in builds).
            _material ??= new Material(Shader.Find("Sprites/Default")) { name = "Mascot" };
            _commands ??= new CommandBuffer { name = "Mascot" };

            var msaa = RenderTexture.GetTemporary(width, height, 0, RenderTextureFormat.ARGB32, RenderTextureReadWrite.Default, AntiAliasing);
            var resolved = RenderTexture.GetTemporary(width, height, 0, RenderTextureFormat.ARGB32);

            var viewport = scene.SceneViewport;
            _commands.Clear();
            _commands.SetRenderTarget(msaa);
            _commands.ClearRenderTarget(true, true, Color.clear);
            // SVG y goes down: top of the projection is the top of the viewBox.
            _commands.SetViewProjectionMatrices(Matrix4x4.identity, Matrix4x4.Ortho(viewport.xMin, viewport.xMax, viewport.yMax, viewport.yMin, -1f, 1f));
            _commands.DrawMesh(_mesh, Matrix4x4.identity, _material);
            Graphics.ExecuteCommandBuffer(_commands);
            Graphics.Blit(msaa, resolved);

            var texture = new Texture2D(width, height, TextureFormat.RGBA32, false)
            {
                name = "Mascot",
                wrapMode = TextureWrapMode.Clamp,
                filterMode = FilterMode.Bilinear
            };

            var previous = RenderTexture.active;
            RenderTexture.active = resolved;
            texture.ReadPixels(new Rect(0, 0, width, height), 0, 0, false);
            RenderTexture.active = previous;

            // The sprite shader blends premultiplied colors, UI images expect straight alpha.
            var pixels = texture.GetPixels32();
            for (var i = 0; i < pixels.Length; i++)
            {
                var p = pixels[i];
                if (p.a is > 0 and < 255)
                {
                    pixels[i] = new Color32((byte)Mathf.Min(255, p.r * 255 / p.a), (byte)Mathf.Min(255, p.g * 255 / p.a), (byte)Mathf.Min(255, p.b * 255 / p.a), p.a);
                }
            }

            texture.SetPixels32(pixels);
            texture.Apply(false, true);

            RenderTexture.ReleaseTemporary(msaa);
            RenderTexture.ReleaseTemporary(resolved);
            return texture;
        }
    }
}
