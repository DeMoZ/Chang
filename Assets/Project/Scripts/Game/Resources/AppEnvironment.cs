using System;
using System.Collections.Generic;
using UnityEngine;
using Debug = DMZ.DebugSystem.DMZLogger;

namespace Chang.Resources
{
    /// <summary>
    /// The Unity Cloud environment the game works with: dev, staging or production.
    /// It defines both the CCD content (environment, bucket, badge) and Unity Gaming Services data (accounts, Cloud Save).
    /// One WebGL build is promoted between environments without a rebuild, so the environment is picked at runtime:
    /// - WebGL player: by the page hostname: "dev.*" and "staging.*", a host from <see cref="ProductionHosts"/> is production;
    /// - Editor: the choice in the menu Chang/Environment (saved in EditorPrefs);
    /// - anything else (localhost, Unity Play, other platforms): <see cref="DefaultEnvironment"/>.
    /// Remote.LoadPath uses {CcdManager.EnvironmentName}, {CcdManager.BucketId} and {CcdManager.Badge}; Addressables fills
    /// them from the build only when they are not set yet, so they are set here before the first scene loads.
    /// See Docs/build-and-deploy.md.
    /// </summary>
    public static class AppEnvironment
    {
        public const string Dev = "dev";
        public const string Staging = "staging";
        public const string Production = "production";

        public const string DefaultEnvironment = Dev;
        public const string EditorPrefsKey = "Chang.AppEnvironment";

        public static readonly string[] All = { Dev, Staging, Production };

        private static readonly string[] ProductionHosts =
        {
            "kruchaang.win",
            "www.kruchaang.win",
        };

        private class Environment
        {
            public string Badge;

            /// <summary>Bucket id by platform (bucket name = BuildTarget name).</summary>
            public Dictionary<string, string> Buckets;
        }

        // dev and staging show the latest uploaded / promoted release; production shows the release with the "live" badge
        private static readonly Dictionary<string, Environment> Environments = new()
        {
            [Dev] = new Environment
            {
                Badge = "latest",
                Buckets = new Dictionary<string, string>
                {
                    ["WebGL"] = "33801ee2-bd90-4c9c-a184-e9f447d0a552",
                    ["Android"] = "2712cac0-6b8b-4186-a4aa-ee3276437039",
                    ["StandaloneOSX"] = "cc9f9bd4-b9e6-414a-8399-054e16869b86",
                    ["iOS"] = "0f5bf913-31ee-424c-8e48-d0b9bad693bb",
                },
            },
            [Staging] = new Environment
            {
                Badge = "latest",
                Buckets = new Dictionary<string, string>
                {
                    ["WebGL"] = "ac61c8c3-276d-4076-9f9c-8475213a8958",
                    ["Android"] = "4766bbc6-6b09-446c-9648-478501d7a3a5",
                    ["StandaloneOSX"] = "fb4ddd18-32f4-4f5b-9794-22307beb7192",
                    ["iOS"] = "e60e120d-1108-4041-9405-4cb7aa75ba6e",
                },
            },
            [Production] = new Environment
            {
                Badge = "live",
                Buckets = new Dictionary<string, string>
                {
                    ["WebGL"] = "d1cf58d3-0ac4-41e8-b1f9-da36a6f006cb",
                    ["Android"] = "a5ec98ca-6494-44e7-b9c4-d018c0c79086",
                    ["StandaloneOSX"] = "2f7fea81-479e-49e1-85b0-eca87f26d75d",
                    ["iOS"] = "a472083c-0a92-4038-bcde-1266ef956a31",
                },
            },
        };

        /// <summary>
        /// The current environment name, also used for UnityServices initialization.
        /// </summary>
        public static string Current { get; private set; } = DefaultEnvironment;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Initialize()
        {
            Current = Resolve();
            ApplyCcd(Current, GetPlatform());
        }

        private static string Resolve()
        {
#if UNITY_EDITOR
            string selected = UnityEditor.EditorPrefs.GetString(EditorPrefsKey, DefaultEnvironment);
            return Environments.ContainsKey(selected) ? selected : DefaultEnvironment;
#elif UNITY_WEBGL
            return GetEnvironmentByUrl(Application.absoluteURL) ?? DefaultEnvironment;
#else
            return DefaultEnvironment;
#endif
        }

        private static string GetPlatform()
        {
#if UNITY_EDITOR
            return UnityEditor.EditorUserBuildSettings.activeBuildTarget.ToString();
#else
            return Application.platform switch
            {
                RuntimePlatform.WebGLPlayer => "WebGL",
                RuntimePlatform.Android => "Android",
                RuntimePlatform.IPhonePlayer => "iOS",
                RuntimePlatform.OSXPlayer => "StandaloneOSX",
                _ => Application.platform.ToString(),
            };
#endif
        }

        /// <summary>
        /// Returns the environment for a page url, or null when the url does not define one.
        /// </summary>
        public static string GetEnvironmentByUrl(string url)
        {
            if (!Uri.TryCreate(url, UriKind.Absolute, out Uri uri))
            {
                return null;
            }

            string host = uri.Host.ToLowerInvariant();

            if (host.StartsWith(Dev + "."))
            {
                return Dev;
            }

            if (host.StartsWith(Staging + "."))
            {
                return Staging;
            }

            return Array.IndexOf(ProductionHosts, host) >= 0 ? Production : null;
        }

        /// <summary>
        /// The CCD bucket id and badge the game loads for an environment and platform (bucket name = BuildTarget name).
        /// </summary>
        public static bool TryGetCcd(string environmentName, string platform, out string bucketId, out string badge)
        {
            bucketId = null;
            badge = null;

            if (!Environments.TryGetValue(environmentName, out Environment environment)
                || !environment.Buckets.TryGetValue(platform, out bucketId))
            {
                return false;
            }

            badge = environment.Badge;
            return true;
        }

        /// <summary>
        /// Points Addressables to the environment's CCD bucket and badge. Must be called before Addressables initialization.
        /// </summary>
        public static bool ApplyCcd(string environmentName, string platform)
        {
            if (!TryGetCcd(environmentName, platform, out string bucketId, out string badge))
            {
                Debug.LogWarning($"{nameof(AppEnvironment)}: no {platform} bucket in {environmentName}, using the content build settings");
                return false;
            }

            CcdManager.EnvironmentName = environmentName;
            CcdManager.BucketId = bucketId;
            CcdManager.Badge = badge;

            Debug.Log($"{nameof(AppEnvironment)}: {environmentName}, bucket {platform} {bucketId}, badge {badge}");
            return true;
        }
    }
}
