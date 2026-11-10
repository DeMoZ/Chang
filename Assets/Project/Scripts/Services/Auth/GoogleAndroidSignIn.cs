using System.Threading.Tasks;
using UnityEngine;

namespace Chang.Services.Auth
{
    /// <summary>
    /// Android: Credential Manager, "Sign in with Google" (Plugins/Android/ChangGoogleSignIn.androidlib).
    /// The ID token is requested for the Web client ID (serverClientId), the Android OAuth client (package name + SHA-1 of the
    /// signing key) only has to exist in the same Google Cloud project.
    /// </summary>
    internal static class GoogleAndroidSignIn
    {
        private const string BridgeClass = "com.roman.kolychev.chang.googlesignin.GoogleSignInBridge";

        /// <returns>the Google ID token, null if the player cancelled</returns>
        public static Task<string> RequestIdTokenAsync(string clientId)
        {
            var tcs = new TaskCompletionSource<string>(TaskCreationOptions.RunContinuationsAsynchronously);

            try
            {
                using var unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
                using var activity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");
                using var bridge = new AndroidJavaClass(BridgeClass);
                bridge.CallStatic("signIn", activity, clientId, new Callback(tcs));
            }
            catch (System.Exception e)
            {
                tcs.TrySetException(e);
            }

            return tcs.Task;
        }

        /// <summary>Called on the Android main thread; the awaiting code continues on the Unity thread (its synchronization context).</summary>
        private class Callback : AndroidJavaProxy
        {
            private readonly TaskCompletionSource<string> _tcs;

            public Callback(TaskCompletionSource<string> tcs) : base(BridgeClass + "$Callback")
            {
                _tcs = tcs;
            }

            // ReSharper disable InconsistentNaming: called from Java
            public void onSuccess(string idToken) => _tcs.TrySetResult(idToken);

            public void onCanceled() => _tcs.TrySetResult(null);

            public void onError(string message) => _tcs.TrySetException(new GoogleSignInException(message));
            // ReSharper restore InconsistentNaming
        }
    }
}
