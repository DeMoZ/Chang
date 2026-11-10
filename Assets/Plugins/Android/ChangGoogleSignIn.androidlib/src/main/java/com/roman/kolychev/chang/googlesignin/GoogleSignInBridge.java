package com.roman.kolychev.chang.googlesignin;

import android.app.Activity;
import android.os.CancellationSignal;
import android.os.Handler;
import android.os.Looper;
import android.util.Log;

import androidx.credentials.Credential;
import androidx.credentials.CredentialManager;
import androidx.credentials.CredentialManagerCallback;
import androidx.credentials.CustomCredential;
import androidx.credentials.GetCredentialRequest;
import androidx.credentials.GetCredentialResponse;
import androidx.credentials.exceptions.GetCredentialCancellationException;
import androidx.credentials.exceptions.GetCredentialException;

import com.google.android.libraries.identity.googleid.GetSignInWithGoogleOption;
import com.google.android.libraries.identity.googleid.GoogleIdTokenCredential;

import java.util.concurrent.Executor;

/**
 * Chang: "Sign in with Google" with Credential Manager, called from C# (GoogleAndroidSignIn.cs).
 * The ID token is requested for the Web OAuth client ID (serverClientId); Unity Authentication checks it.
 * The Android OAuth client (package name + SHA-1) only has to exist in the same Google Cloud project.
 */
public final class GoogleSignInBridge {
    private static final String TAG = "ChangGoogleSignIn";

    public interface Callback {
        void onSuccess(String idToken);

        void onCanceled();

        void onError(String message);
    }

    private GoogleSignInBridge() {
    }

    public static void signIn(final Activity activity, final String serverClientId, final Callback callback) {
        final Handler mainHandler = new Handler(Looper.getMainLooper());
        final Executor mainExecutor = mainHandler::post;

        mainHandler.post(() -> {
            try {
                // the "Sign in with Google" button flow: always shows the account picker, also with no account signed in before
                GetSignInWithGoogleOption option = new GetSignInWithGoogleOption.Builder(serverClientId).build();
                GetCredentialRequest request = new GetCredentialRequest.Builder().addCredentialOption(option).build();

                CredentialManager.create(activity).getCredentialAsync(activity, request, new CancellationSignal(), mainExecutor,
                        new CredentialManagerCallback<GetCredentialResponse, GetCredentialException>() {
                            @Override
                            public void onResult(GetCredentialResponse response) {
                                handleResult(response, callback);
                            }

                            @Override
                            public void onError(GetCredentialException e) {
                                if (e instanceof GetCredentialCancellationException) {
                                    callback.onCanceled();
                                    return;
                                }

                                Log.w(TAG, "getCredential failed", e);
                                callback.onError(e.getType() + ": " + e.getMessage());
                            }
                        });
            } catch (Exception e) {
                Log.e(TAG, "signIn failed", e);
                callback.onError(e.getClass().getSimpleName() + ": " + e.getMessage());
            }
        });
    }

    private static void handleResult(GetCredentialResponse response, Callback callback) {
        Credential credential = response.getCredential();
        if (!(credential instanceof CustomCredential)
                || !GoogleIdTokenCredential.TYPE_GOOGLE_ID_TOKEN_CREDENTIAL.equals(credential.getType())) {
            callback.onError("unexpected credential type " + credential.getType());
            return;
        }

        try {
            GoogleIdTokenCredential google = GoogleIdTokenCredential.createFrom(credential.getData());
            callback.onSuccess(google.getIdToken());
        } catch (Exception e) {
            Log.e(TAG, "bad Google ID token credential", e);
            callback.onError(e.getClass().getSimpleName() + ": " + e.getMessage());
        }
    }
}
