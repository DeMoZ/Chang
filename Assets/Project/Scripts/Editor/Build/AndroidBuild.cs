using System.IO;
using Chang.Resources;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace Chang.Editor.Build
{
    /// <summary>
    /// Android APK build for one environment: an app has no page hostname, so the environment (CCD content, Cloud Save)
    /// is written into the build (see <see cref="PlayerBuildEnvironment"/>). Release the Android content
    /// to that environment first (Chang/Content with the Android build target). See Docs/build-and-deploy.md.
    /// </summary>
    public static class AndroidBuild
    {
        public const string OutputFolder = "Builds/Android";
        private const string Root = "Chang/Build/Android APK ";

        [MenuItem(Root + "(dev)")]
        private static void BuildDev() => Build(AppEnvironment.Dev, BuildOptions.Development);

        [MenuItem(Root + "(staging)")]
        private static void BuildStaging() => Build(AppEnvironment.Staging, BuildOptions.None);

        [MenuItem(Root + "(production)")]
        private static void BuildProduction() => Build(AppEnvironment.Production, BuildOptions.None);

        public static bool Build(string environment, BuildOptions options)
        {
            string path = $"{OutputFolder}/Chang-{PlayerSettings.bundleVersion}-{environment}.apk";
            Directory.CreateDirectory(OutputFolder);

            EditorUserBuildSettings.buildAppBundle = false;

            var buildOptions = new BuildPlayerOptions
            {
                scenes = EditorBuildSettingsScene.GetActiveSceneList(EditorBuildSettings.scenes),
                locationPathName = path,
                target = BuildTarget.Android,
                options = options,
            };

            BuildReport report;
            try
            {
                PlayerBuildEnvironment.Next = environment;
                report = BuildPipeline.BuildPlayer(buildOptions);
            }
            finally
            {
                PlayerBuildEnvironment.Next = null;
            }

            if (report.summary.result != BuildResult.Succeeded)
            {
                Debug.LogError($"Android build failed: {report.summary.result}, errors: {report.summary.totalErrors}");
                return false;
            }

            Debug.Log($"Android build {PlayerSettings.bundleVersion} for {environment} done: {Path.GetFullPath(path)}\n" +
                      $"Install: adb install -r \"{path}\"");
            return true;
        }
    }
}
