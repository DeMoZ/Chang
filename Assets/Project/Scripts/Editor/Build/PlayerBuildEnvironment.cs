using System.IO;
using Chang.Resources;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace Chang.Editor.Build
{
    /// <summary>
    /// Writes the environment of a player build into Resources (<see cref="AppEnvironment.BuildResourcePath"/>), where
    /// <see cref="AppEnvironment"/> reads it on platforms without a page hostname (Android, iOS, Standalone).
    /// The environment is <see cref="Next"/> when a Chang/Build menu set it, otherwise dev
    /// (e.g. a build from the Build Profiles window). WebGL picks the environment by the hostname and ignores it there.
    /// </summary>
    public class PlayerBuildEnvironment : IPreprocessBuildWithReport
    {
        private const string AssetPath = "Assets/Resources/" + AppEnvironment.BuildResourcePath + ".txt";

        /// <summary>The environment for the next player build; reset by the build menu after the build.</summary>
        public static string Next { get; set; }

        public int callbackOrder => 0;

        public void OnPreprocessBuild(BuildReport report)
        {
            string environment = Next ?? AppEnvironment.DefaultEnvironment;

            Directory.CreateDirectory(Path.GetDirectoryName(AssetPath));
            File.WriteAllText(AssetPath, environment);
            AssetDatabase.ImportAsset(AssetPath, ImportAssetOptions.ForceSynchronousImport);

            Debug.Log($"{nameof(PlayerBuildEnvironment)}: {report.summary.platform} player build for {environment}");
        }
    }
}
