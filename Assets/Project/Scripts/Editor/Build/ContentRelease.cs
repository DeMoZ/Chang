using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Chang.Resources;
using UnityEditor;
using UnityEditor.AddressableAssets.Build;
using UnityEditor.AddressableAssets.Settings;
using UnityEngine;
using UnityEngine.Networking;

namespace Chang.Editor.Build
{
    /// <summary>
    /// Chang/Content: CCD content release flow for the active build target (bucket name = BuildTarget name).
    /// - Build & Release: Addressables Build & Release to the dev environment (the CCD Automatic profile), badge "latest";
    /// - Promote: copies the release a badge points to into the next environment (dev → staging → production)
    ///   and, for production, puts the "live" badge on it. Bucket ids and badges come from <see cref="AppEnvironment"/>.
    /// Uses the Editor's Unity account (Services must be linked to the project). See Docs/build-and-deploy.md.
    /// </summary>
    public static class ContentRelease
    {
        private const string Root = "Chang/Content/";
        private const string ServicesUrl = "https://services.unity.com";

        [MenuItem(Root + "Build & Release to dev")]
        private static async void BuildAndRelease()
        {
            string platform = EditorUserBuildSettings.activeBuildTarget.ToString();
            Debug.Log($"{nameof(ContentRelease)}: Build & Release {platform} content to dev");

            AddressableAssetBuildResult result = await AddressableAssetSettings.BuildAndReleasePlayerContent();
            // the Addressables CCD upload leaves its "Postbuild" progress bar open
            EditorUtility.ClearProgressBar();

            if (!string.IsNullOrEmpty(result.Error))
            {
                Debug.LogError($"{nameof(ContentRelease)}: Build & Release {platform} failed: {result.Error}");
                return;
            }

            Debug.Log($"{nameof(ContentRelease)}: Build & Release {platform} done");
        }

        [MenuItem(Root + "Promote dev to staging")]
        private static void PromoteToStaging() => Promote(AppEnvironment.Dev, AppEnvironment.Staging);

        [MenuItem(Root + "Promote staging to production (live)")]
        private static void PromoteToProduction() => Promote(AppEnvironment.Staging, AppEnvironment.Production);

        private static async void Promote(string from, string to)
        {
            string platform = EditorUserBuildSettings.activeBuildTarget.ToString();
            string log = $"{nameof(ContentRelease)}: promote {platform} {from} → {to}";

            try
            {
                if (!AppEnvironment.TryGetCcd(from, platform, out string fromBucket, out string fromBadge)
                    || !AppEnvironment.TryGetCcd(to, platform, out string toBucket, out string toBadge))
                {
                    Debug.LogError($"{log}: no {platform} bucket in {nameof(AppEnvironment)}");
                    return;
                }

                string projectId = CloudProjectSettings.projectId;
                string token = await GetServiceToken();
                EnvironmentList environments = await Send<EnvironmentList>("GET",
                    $"/api/unity/v1/projects/{projectId}/environments", null, token);
                string fromEnvironmentId = GetEnvironmentId(environments, from);
                string toEnvironmentId = GetEnvironmentId(environments, to);

                string ccd = $"/api/ccd/management/v1/projects/{projectId}/environments";
                Badge badge = await Send<Badge>("GET",
                    $"{ccd}/{fromEnvironmentId}/buckets/{fromBucket}/badges/{fromBadge}", null, token);

                var promoteBody = new PromoteBody
                {
                    from_release = badge.releaseid,
                    to_bucket = toBucket,
                    to_environment = toEnvironmentId,
                };
                Release release = await Send<Release>("POST",
                    $"{ccd}/{fromEnvironmentId}/buckets/{fromBucket}/promote", JsonUtility.ToJson(promoteBody), token);

                // "latest" moves to the newest release by itself; other badges are put by hand
                if (toBadge != "latest")
                {
                    var badgeBody = new BadgeAssign { name = toBadge, releaseid = release.releaseid };
                    await Send<Badge>("PUT", $"{ccd}/{toEnvironmentId}/buckets/{toBucket}/badges",
                        JsonUtility.ToJson(badgeBody), token);
                }

                Debug.Log($"{log}: done, release {fromBadge} {badge.releaseid} (#{badge.releasenum}) → " +
                          $"{release.releaseid} (#{release.releasenum}), badge {toBadge}");
            }
            catch (Exception e)
            {
                Debug.LogError($"{log} failed: {e.Message}");
            }
        }

        private static string GetEnvironmentId(EnvironmentList environments, string name)
        {
            return environments.results?.FirstOrDefault(e => e.name == name)?.id
                   ?? throw new Exception($"Unity Cloud environment '{name}' not found");
        }

        /// <summary>
        /// Exchanges the Editor's Unity account token for a Unity Services token (the same as the CCD Management package does).
        /// </summary>
        private static async Task<string> GetServiceToken()
        {
            string accessToken = CloudProjectSettings.accessToken;
            if (string.IsNullOrEmpty(accessToken) || string.IsNullOrEmpty(CloudProjectSettings.projectId))
            {
                throw new Exception("sign in to Unity in the Editor and link the project to Unity Cloud");
            }

            var body = new Token { token = accessToken };
            Token result = await Send<Token>("POST", "/api/auth/v1/genesis-token-exchange/unity/",
                JsonUtility.ToJson(body), null);
            return result.token;
        }

        private static async Task<T> Send<T>(string method, string path, string json, string token)
        {
            using var request = new UnityWebRequest(ServicesUrl + path, method);
            request.downloadHandler = new DownloadHandlerBuffer();

            if (json != null)
            {
                request.uploadHandler = new UploadHandlerRaw(Encoding.UTF8.GetBytes(json));
                request.SetRequestHeader("Content-Type", "application/json");
            }

            if (token != null)
            {
                request.SetRequestHeader("Authorization", "Bearer " + token);
            }

            var completion = new TaskCompletionSource<bool>();
            request.SendWebRequest().completed += _ => completion.SetResult(true);
            await completion.Task;

            if (request.result != UnityWebRequest.Result.Success)
            {
                throw new Exception($"{method} {path}: {request.responseCode} {request.error} {request.downloadHandler.text}");
            }

            return JsonUtility.FromJson<T>(request.downloadHandler.text);
        }

        [Serializable]
        private class Token
        {
            public string token;
        }

        [Serializable]
        private class EnvironmentList
        {
            public EnvironmentInfo[] results;
        }

        [Serializable]
        private class EnvironmentInfo
        {
            public string id;
            public string name;
        }

        [Serializable]
        private class Badge
        {
            public string name;
            public string releaseid;
            public long releasenum;
        }

        [Serializable]
        private class BadgeAssign
        {
            public string name;
            public string releaseid;
        }

        [Serializable]
        private class Release
        {
            public string releaseid;
            public long releasenum;
        }

        [Serializable]
        private class PromoteBody
        {
            public string from_release;
            public string to_bucket;
            public string to_environment;
        }
    }
}
