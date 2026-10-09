using System;
using System.IO;
using Newtonsoft.Json.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.Networking;
using Debug = DMZ.DebugSystem.DMZLogger;

namespace Chang.Editor.DesignSystem
{
    /// <summary>
    /// Downloads the Chang Penpot file as JSON into Design/chang-penpot.json through the Penpot API with a personal access token,
    /// so no browser session is needed. The token is kept in Design/penpot-token.txt (git-ignored) or PENPOT_ACCESS_TOKEN;
    /// Tools/Penpot/download.py does the same from the terminal. See Docs/ui-design-system.md#updating-the-design.
    /// </summary>
    public static class PenpotDownloader
    {
        private const string BaseUrl = "https://design.penpot.app";
        private const string FileId = "71b39894-c9c5-81cd-8008-ba09a7fc113c";
        private const string TokenFile = "Design/penpot-token.txt";
        private const string OutFile = "Design/chang-penpot.json";
        private const string Title = "Download from Penpot";

        private static string ProjectRoot => Path.GetDirectoryName(Application.dataPath);

        [MenuItem("Chang/Design System/Download from Penpot", false, -20)]
        private static void DownloadMenu() => Download(false);

        [MenuItem("Chang/Design System/Download and Import from Penpot", false, -19)]
        private static void DownloadAndImportMenu() => Download(true);

        [MenuItem("Chang/Design System/Set Penpot Access Token…", false, -18)]
        private static void SetTokenMenu() => PenpotTokenWindow.Open(null);

        private static void Download(bool import)
        {
            var token = ReadToken();
            if (string.IsNullOrEmpty(token))
            {
                PenpotTokenWindow.Open(() => Download(import));
                return;
            }

            var request = UnityWebRequest.Get($"{BaseUrl}/api/rpc/command/get-file?id={FileId}");
            request.SetRequestHeader("Accept", "application/json");
            request.SetRequestHeader("Authorization", $"Token {token}");
            request.timeout = 300;
            var operation = request.SendWebRequest();

            void Update()
            {
                if (!operation.isDone)
                {
                    var mb = request.downloadedBytes / 1e6f;
                    if (EditorUtility.DisplayCancelableProgressBar(Title, $"{mb:0.0} MB", Mathf.Clamp01(mb / 25f)))
                    {
                        request.Abort();
                    }

                    return;
                }

                EditorApplication.update -= Update;
                EditorUtility.ClearProgressBar();
                try
                {
                    Finish(request, import);
                }
                finally
                {
                    request.Dispose();
                }
            }

            EditorApplication.update += Update;
        }

        private static void Finish(UnityWebRequest request, bool import)
        {
            if (request.result != UnityWebRequest.Result.Success)
            {
                var expired = request.responseCode is 401 or 403 ? " The access token is wrong or expired: set a new one." : "";
                Debug.LogError($"[{nameof(PenpotDownloader)}] [{nameof(Finish)}] {request.responseCode} {request.error}.{expired}\n{Truncate(request.downloadHandler?.text)}");
                if (!string.IsNullOrEmpty(expired) && EditorUtility.DisplayDialog(Title, $"Penpot answered {request.responseCode}.{expired}", "Set token", "Cancel"))
                {
                    PenpotTokenWindow.Open(() => Download(import));
                }

                return;
            }

            var text = request.downloadHandler.text;
            // An error object must not replace the last good export.
            JObject file;
            try
            {
                file = JObject.Parse(text);
            }
            catch (Exception e)
            {
                Debug.LogError($"[{nameof(PenpotDownloader)}] [{nameof(Finish)}] Not JSON: {e.Message}\n{Truncate(text)}");
                return;
            }

            if (file["data"]?["pagesIndex"] is not JObject pages || pages.Count == 0)
            {
                Debug.LogError($"[{nameof(PenpotDownloader)}] [{nameof(Finish)}] Not a Penpot file export\n{Truncate(text)}");
                return;
            }

            var path = Path.Combine(ProjectRoot, OutFile);
            Directory.CreateDirectory(Path.GetDirectoryName(path));
            var tmp = path + ".tmp";
            File.WriteAllText(tmp, text);
            if (File.Exists(path))
            {
                File.Delete(path);
            }

            File.Move(tmp, path);
            EditorPrefs.SetString(PenpotImporter.LastJsonKey, path);
            Debug.Log($"[{nameof(PenpotDownloader)}] [{nameof(Finish)}] {OutFile}: '{file["name"]}' revision {file["revn"]}, {pages.Count} pages, {text.Length / 1e6f:0.0} MB");

            if (import)
            {
                PenpotImporter.Import(path);
            }
        }

        private static string Truncate(string text) => text == null ? "" : text.Length > 300 ? text.Substring(0, 300) : text;

        internal static string ReadToken()
        {
            var token = Environment.GetEnvironmentVariable("PENPOT_ACCESS_TOKEN");
            if (!string.IsNullOrWhiteSpace(token))
            {
                return token.Trim();
            }

            var path = Path.Combine(ProjectRoot, TokenFile);
            return File.Exists(path) ? File.ReadAllText(path).Trim() : null;
        }

        internal static void WriteToken(string token)
        {
            var path = Path.Combine(ProjectRoot, TokenFile);
            Directory.CreateDirectory(Path.GetDirectoryName(path));
            File.WriteAllText(path, token.Trim());
        }
    }

    /// <summary>Asks for the Penpot personal access token and keeps it in Design/penpot-token.txt (git-ignored).</summary>
    public class PenpotTokenWindow : EditorWindow
    {
        private string _token = "";
        private Action _onSaved;

        public static void Open(Action onSaved)
        {
            var window = GetWindow<PenpotTokenWindow>(true, "Penpot Access Token");
            window._onSaved = onSaved;
            window.minSize = window.maxSize = new Vector2(460f, 170f);
            window.ShowUtility();
        }

        private void OnGUI()
        {
            EditorGUILayout.HelpBox(
                "Penpot → your avatar → Your account → Access tokens → Generate new token. " +
                "Paste it here; it is kept in Design/penpot-token.txt, which git ignores.",
                MessageType.Info);
            _token = EditorGUILayout.PasswordField("Token", _token);

            EditorGUILayout.Space();
            using (new EditorGUILayout.HorizontalScope())
            {
                if (GUILayout.Button("Open Penpot"))
                {
                    Application.OpenURL("https://design.penpot.app/#/settings/access-tokens");
                }

                using (new EditorGUI.DisabledScope(string.IsNullOrWhiteSpace(_token)))
                {
                    if (GUILayout.Button("Save"))
                    {
                        PenpotDownloader.WriteToken(_token);
                        var onSaved = _onSaved;
                        Close();
                        onSaved?.Invoke();
                    }
                }
            }
        }
    }
}
