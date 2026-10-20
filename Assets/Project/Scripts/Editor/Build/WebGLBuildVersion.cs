using System.IO;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace Chang.Editor.Build
{
    /// <summary>
    /// Writes version.json (<c>{"version": "&lt;bundleVersion&gt;"}</c>) next to index.html of every WebGL build.
    /// The ChangPWA page compares it with its own build version and reloads an old build after a deploy (#104).
    /// See Docs/build-and-deploy.md.
    /// </summary>
    public class WebGLBuildVersion : IPostprocessBuildWithReport
    {
        public const string FileName = "version.json";

        public int callbackOrder => 0;

        public void OnPostprocessBuild(BuildReport report)
        {
            if (report.summary.platform != BuildTarget.WebGL)
            {
                return;
            }

            string path = Path.Combine(report.summary.outputPath, FileName);
            File.WriteAllText(path, JsonUtility.ToJson(new VersionFile { version = PlayerSettings.bundleVersion }));
            Debug.Log($"{nameof(WebGLBuildVersion)}: {path} {PlayerSettings.bundleVersion}");
        }

        [System.Serializable]
        private class VersionFile
        {
            public string version;
        }
    }
}
