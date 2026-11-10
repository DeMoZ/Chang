using System;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using AOT;

namespace Chang.Services.Auth
{
    /// <summary>
    /// WebGL: Google sign-in in a popup (Plugins/WebGL/ChangGoogleSignIn.jslib). The popup opens Google's OpenID Connect page
    /// (response_type=id_token), Google returns to google-auth/ of the same site (WebGLTemplates/ChangPWA/google-auth/index.html),
    /// which sends the ID token back to the game and closes.
    /// </summary>
    internal static class GoogleWebSignIn
    {
        // the values are the same in ChangGoogleSignIn.jslib
        private const int StatusSuccess = 0;
        private const int StatusCanceled = 1;

        private delegate void ResultCallback(int status, string value);

        private static TaskCompletionSource<string> _tcs;

#if UNITY_WEBGL && !UNITY_EDITOR
        [DllImport("__Internal")]
        private static extern void ChangGoogleSignIn_Start(string clientId, ResultCallback callback);
#else
        private static void ChangGoogleSignIn_Start(string clientId, ResultCallback callback)
        {
            callback(2, "the jslib works only in a WebGL player");
        }
#endif

        /// <returns>the Google ID token, null if the player closed the popup</returns>
        public static Task<string> RequestIdTokenAsync(string clientId)
        {
            // one sign-in at a time: a new one cancels the previous one (the jslib does the same)
            _tcs?.TrySetResult(null);
            var tcs = new TaskCompletionSource<string>(TaskCreationOptions.RunContinuationsAsynchronously);
            _tcs = tcs;
            ChangGoogleSignIn_Start(clientId, OnResult);
            return tcs.Task;
        }

        [MonoPInvokeCallback(typeof(ResultCallback))]
        private static void OnResult(int status, string value)
        {
            var tcs = _tcs;
            _tcs = null;
            if (tcs == null)
            {
                return;
            }

            switch (status)
            {
                case StatusSuccess:
                    tcs.TrySetResult(value);
                    break;
                case StatusCanceled:
                    tcs.TrySetResult(null);
                    break;
                default:
                    tcs.TrySetException(new GoogleSignInException(value));
                    break;
            }
        }
    }
}
