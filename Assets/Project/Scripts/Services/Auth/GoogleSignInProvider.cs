using System.Threading.Tasks;
using DMZ.Legacy.LoginScreen;
using Unity.Services.Authentication;
using UnityEngine;

namespace Chang.Services.Auth
{
    /// <summary>
    /// Sign in with Google: the platform part gets a Google ID token, Unity Authentication signs in or links with it.
    /// - WebGL: a Google OpenID Connect popup (<see cref="GoogleWebSignIn"/>, Plugins/WebGL/ChangGoogleSignIn.jslib);
    /// - Android: Credential Manager, "Sign in with Google" (<see cref="GoogleAndroidSignIn"/>, Plugins/Android/ChangGoogleSignIn.androidlib);
    /// - Editor, iOS (later, with the iOS release) and other platforms: not supported, the buttons are hidden.
    /// See Docs/save-and-auth.md#google.
    /// </summary>
    public class GoogleSignInProvider : IExternalSignInProvider
    {
        public const string ProviderId = "google";

        private readonly string _clientId;

        public GoogleSignInProvider(GoogleSignInConfig config)
        {
            _clientId = config != null ? config.WebClientId : null;
        }

        public string Id => ProviderId;

        public bool IsSupported => IsPlatformSupported && !string.IsNullOrEmpty(_clientId);

        private static bool IsPlatformSupported =>
#if UNITY_EDITOR
            false;
#elif UNITY_WEBGL || UNITY_ANDROID
            true;
#else
            false;
#endif

        public Task<string> RequestCredentialAsync()
        {
#if UNITY_WEBGL && !UNITY_EDITOR
            return GoogleWebSignIn.RequestIdTokenAsync(_clientId);
#elif UNITY_ANDROID && !UNITY_EDITOR
            return GoogleAndroidSignIn.RequestIdTokenAsync(_clientId);
#else
            Debug.LogWarning($"[{nameof(GoogleSignInProvider)}] Google sign-in is not supported on {Application.platform}");
            return Task.FromException<string>(new GoogleSignInException("not supported on this platform"));
#endif
        }

        public Task SignInAsync(string idToken)
        {
            return AuthenticationService.Instance.SignInWithGoogleAsync(idToken);
        }

        public Task LinkAsync(string idToken)
        {
            return AuthenticationService.Instance.LinkWithGoogleAsync(idToken);
        }
    }

    public class GoogleSignInException : System.Exception
    {
        public GoogleSignInException(string message) : base($"Google sign-in failed: {message}")
        {
        }
    }
}
