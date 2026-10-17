using System;
using System.Collections.Generic;
using UnityEngine;
using Debug = DMZ.DebugSystem.DMZLogger;

namespace Chang.Resources
{
    /// <summary>
    /// Picks the Unity CCD environment (dev / staging / production) at runtime, so one WebGL build can be promoted
    /// between environments without a rebuild. Remote.LoadPath uses {CcdManager.EnvironmentName}, {CcdManager.BucketId}
    /// and {CcdManager.Badge}; Addressables fills them from the build only when they are not set yet,
    /// so they are set here before the first scene loads.
    /// The environment comes from the page hostname: "dev.*" and "staging.*" pick those environments,
    /// a host from <see cref="ProductionHosts"/> picks production. Anything else (localhost, Unity Play, the Editor,
    /// non-WebGL platforms) keeps the environment the content was built with (Addressables profile, "dev").
    /// See Docs/build-and-deploy.md.
    /// </summary>
    public static class CcdEnvironments
    {
        public const string Dev = "dev";
        public const string Staging = "staging";
        public const string Production = "production";

        private const string DevBadge = "latest";
        private const string ReleaseBadge = "live";

        private static readonly string[] ProductionHosts =
        {
            "kruchaang.pages.dev",
        };

        private class Environment
        {
            public string Badge;

            /// <summary>Bucket id by platform (bucket name = BuildTarget name). Empty id = bucket not created yet.</summary>
            public Dictionary<string, string> Buckets;
        }

        private static readonly Dictionary<string, Environment> Environments = new()
        {
            [Dev] = new Environment
            {
                Badge = DevBadge,
                Buckets = new Dictionary<string, string>
                {
                    ["WebGL"] = "33801ee2-bd90-4c9c-a184-e9f447d0a552",
                    ["Android"] = "2712cac0-6b8b-4186-a4aa-ee3276437039",
                    ["StandaloneOSX"] = "cc9f9bd4-b9e6-414a-8399-054e16869b86",
                },
            },
            [Staging] = new Environment
            {
                Badge = ReleaseBadge,
                Buckets = new Dictionary<string, string>
                {
                    // todo #83: fill in after the buckets are created in Unity Cloud
                    ["WebGL"] = "",
                    ["Android"] = "",
                },
            },
            [Production] = new Environment
            {
                Badge = ReleaseBadge,
                Buckets = new Dictionary<string, string>
                {
                    // todo #83: fill in after the bucket is created in Unity Cloud
                    ["WebGL"] = "",
                    ["Android"] = "a5ec98ca-6494-44e7-b9c4-d018c0c79086",
                },
            },
        };

#if UNITY_WEBGL && !UNITY_EDITOR
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void ConfigureFromPageUrl()
        {
            string environmentName = GetEnvironmentByUrl(Application.absoluteURL);

            if (environmentName == null)
            {
                Debug.Log($"{nameof(CcdEnvironments)}: no environment for {Application.absoluteURL}, using the build one");
                return;
            }

            Apply(environmentName, "WebGL");
        }
#endif

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
        /// Points Addressables to the environment. Must be called before Addressables initialization.
        /// </summary>
        public static bool Apply(string environmentName, string platform)
        {
            if (!Environments.TryGetValue(environmentName, out Environment environment)
                || !environment.Buckets.TryGetValue(platform, out string bucketId)
                || string.IsNullOrEmpty(bucketId))
            {
                Debug.LogWarning($"{nameof(CcdEnvironments)}: no {platform} bucket in {environmentName}, using the build environment");
                return false;
            }

            CcdManager.EnvironmentName = environmentName;
            CcdManager.BucketId = bucketId;
            CcdManager.Badge = environment.Badge;

            Debug.Log($"{nameof(CcdEnvironments)}: {environmentName}, bucket {platform} {bucketId}, badge {environment.Badge}");
            return true;
        }
    }
}
