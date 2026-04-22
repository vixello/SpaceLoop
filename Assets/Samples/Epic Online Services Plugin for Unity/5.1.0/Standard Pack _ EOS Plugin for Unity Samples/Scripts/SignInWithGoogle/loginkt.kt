
package com.playeveryware.googlelogin

import com.google.android.libraries.identity.googleid.GetSignInWithGoogleOption
import com.google.android.libraries.identity.googleid.GoogleIdTokenCredential
import com.google.android.libraries.identity.googleid.GoogleIdTokenParsingException
//import com.google.android.libraries.identity.googleid.GetGoogleIdOption

import androidx.credentials.GetCredentialRequest
import androidx.credentials.GetCredentialResponse
import androidx.credentials.exceptions.GetCredentialException
import androidx.credentials.Credential
import androidx.credentials.CustomCredential
import androidx.credentials.CredentialManager
import androidx.credentials.CredentialManagerCallback

import java.util.concurrent.Executors
import java.util.concurrent.Executor

import android.content.Context
import android.util.Log
import android.os.CancellationSignal


class LoginKT
{
    public var name : String? = null
    public var token : String? = null

    companion object {
        private var _instance: LoginKT? = null

        @JvmStatic
        fun instance(): LoginKT {
            if (_instance == null) {
                _instance = LoginKT()
            }
            return _instance!!
        }
    }

    public fun getResultName() : String?
    {
        return name
    }

    public fun getResultIdToken() : String?
    {
        return token
    }

    public fun SignInWithGoogle(
        webClientId: String,
        nonce: String,
        context: Context,
        callback: CredentialManagerCallback<GetCredentialResponse, GetCredentialException>
    ) {
        val credentialManager : CredentialManager = CredentialManager.create(context)

        val option : GetSignInWithGoogleOption = GetSignInWithGoogleOption.Builder(webClientId)
            .setNonce(nonce)
            .build()

        val request : GetCredentialRequest = GetCredentialRequest.Builder()
            .addCredentialOption(option)
            .build()

        credentialManager.getCredentialAsync(
            context = context,
            request = request,
            cancellationSignal = CancellationSignal(),
            executor = Executors.newSingleThreadExecutor(),
            callback = callback
        )
    }


    public fun handleFailure(e : GetCredentialException)
    {
        Log.e("Unity", "Received an invalid google id token response", e);
    }

    public fun handleSignIn(result : GetCredentialResponse)
    {
        Log.d("Unity", "KOTLIN: handleSignIn() fired")
        val credential : Credential = result.credential

        if (credential is CustomCredential)
        {
            Log.d("Unity", "KOTLIN: Got CustomCredential type = " + credential.type)
            if (credential.type == GoogleIdTokenCredential.TYPE_GOOGLE_ID_TOKEN_CREDENTIAL)
            {
                try
                {
                    val googleIdTokenCredential : GoogleIdTokenCredential = GoogleIdTokenCredential.createFrom(credential.data)
                    name = googleIdTokenCredential.displayName
                    token = googleIdTokenCredential.idToken

                    Log.d("Unity", "Google Sign-In SUCCESS");
                    Log.d("Unity", "DisplayName = " + name);
                    val preview = token?.take(20) ?: "no_token"
                    Log.d("Unity", "Token (first chars) = $preview")
                }
                catch (e : Exception) {
                    Log.e("Unity", "Failed to parse Google ID token", e)
                }
            }
        }
    }
}

