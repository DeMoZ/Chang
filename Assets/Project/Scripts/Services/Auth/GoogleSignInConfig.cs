using UnityEngine;

namespace Chang.Services.Auth
{
    /// <summary>
    /// Google sign-in settings, Assets/Project/Resources/GoogleSignInConfig.asset.
    /// The OAuth client ID is public (every page with Google sign-in shows it), it is not a secret; there is no client secret here.
    /// See Docs/save-and-auth.md#google.
    /// </summary>
    [CreateAssetMenu(fileName = nameof(GoogleSignInConfig), menuName = "Chang/Google Sign-In Config")]
    public class GoogleSignInConfig : ScriptableObject
    {
        public const string ResourcePath = nameof(GoogleSignInConfig);

        [Tooltip("Google Cloud → APIs & Services → Credentials → OAuth client ID of type Web application " +
                 "(xxxx.apps.googleusercontent.com). WebGL signs in with it, Android asks for an ID token for it " +
                 "(the Android client ID is only registered in Google Cloud). The same ID is set in Unity Dashboard → Authentication → Google. " +
                 "Empty: the Google buttons are hidden.")]
        [SerializeField] private string _webClientId;

        public string WebClientId => _webClientId?.Trim();

        public static GoogleSignInConfig Load()
        {
            var config = UnityEngine.Resources.Load<GoogleSignInConfig>(ResourcePath);
            if (config == null)
            {
                Debug.LogWarning($"[{nameof(GoogleSignInConfig)}] Resources/{ResourcePath} not found, Google sign-in is off");
            }

            return config;
        }
    }
}
