using System.IO;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace Chang.Editor.Build
{
    /// <summary>
    /// WebGL player build for Cloudflare Pages. The same build is deployed to dev, staging and production:
    /// the CCD environment is picked at runtime by the page hostname (see Chang.Resources.CcdEnvironments).
    /// See Docs/build-and-deploy.md.
    /// </summary>
    public static class WebGLBuild
    {
        public const string OutputPath = "Builds/WebGL/Chang";
        private const string Template = "PROJECT:ChangPWA";

        [MenuItem("Chang/Build/WebGL Release")]
        private static void BuildRelease() => Build(BuildOptions.None);

        [MenuItem("Chang/Build/WebGL Development")]
        private static void BuildDevelopment() => Build(BuildOptions.Development);

        public static bool Build(BuildOptions options)
        {
            ApplyPlayerSettings();

            if (Directory.Exists(OutputPath))
            {
                Directory.Delete(OutputPath, true);
            }

            var buildOptions = new BuildPlayerOptions
            {
                scenes = EditorBuildSettingsScene.GetActiveSceneList(EditorBuildSettings.scenes),
                locationPathName = OutputPath,
                target = BuildTarget.WebGL,
                options = options,
            };

            BuildReport report = BuildPipeline.BuildPlayer(buildOptions);

            if (report.summary.result != BuildResult.Succeeded)
            {
                Debug.LogError($"WebGL build failed: {report.summary.result}, errors: {report.summary.totalErrors}");
                return false;
            }

            Debug.Log($"WebGL build {PlayerSettings.bundleVersion} done: {Path.GetFullPath(OutputPath)}\n" +
                      $"Deploy to dev: npx wrangler pages deploy {OutputPath} --project-name kruchaang --branch dev");
            return true;
        }

        /// <summary>
        /// Settings the Cloudflare deploy relies on: the template with the fixed Service Worker and _headers,
        /// Brotli without decompression fallback (served with Content-Encoding from _headers),
        /// file names as content hashes (so Build/* can be cached forever).
        /// </summary>
        private static void ApplyPlayerSettings()
        {
            PlayerSettings.WebGL.template = Template;
            PlayerSettings.WebGL.compressionFormat = WebGLCompressionFormat.Brotli;
            PlayerSettings.WebGL.decompressionFallback = false;
            PlayerSettings.WebGL.nameFilesAsHashes = true;
        }
    }
}
