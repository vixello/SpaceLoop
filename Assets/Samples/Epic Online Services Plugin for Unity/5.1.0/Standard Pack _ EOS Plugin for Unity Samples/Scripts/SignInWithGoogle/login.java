/*
 * Copyright (c) 2024 PlayEveryWare
 *
 * Permission is hereby granted, free of charge, to any person obtaining a copy
 * of this software and associated documentation files (the "Software"), to deal
 * in the Software without restriction, including without limitation the rights
 * to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
 * copies of the Software, and to permit persons to whom the Software is
 * furnished to do so, subject to the following conditions:
 *
 * The above copyright notice and this permission notice shall be included in all
 * copies or substantial portions of the Software.
 *
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
 * IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
 * FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
 * AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
 * LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
 * OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
 * SOFTWARE.
 */

package com.playeveryware.googlelogin;

import com.google.android.libraries.identity.googleid.GetSignInWithGoogleOption;
import com.google.android.libraries.identity.googleid.GoogleIdTokenCredential;
import com.google.android.libraries.identity.googleid.GoogleIdTokenParsingException;

import androidx.credentials.GetCredentialRequest;
import androidx.credentials.GetCredentialResponse;
import androidx.credentials.exceptions.GetCredentialException;
import androidx.credentials.Credential;
import androidx.credentials.CustomCredential;
import androidx.credentials.CredentialManager;
import androidx.credentials.CredentialManagerCallback;

import android.app.Activity;
import android.content.Context;
import android.os.CancellationSignal;
import android.util.Log;

import java.util.concurrent.Executors;
import java.util.concurrent.Executor;


public class login //extends Activity
{
    private String name;
    private String token;
    private static login instance;

    public login()
    {
        instance = this;
    }

    public static login instance()
    {
        if(instance == null)
        {
            instance = new login();
        }
        return instance;
    }

    public String getResultName()
    {
        return name;
    }
    public String getResultIdToken()
    {
        return token;
    }

    public interface GoogleSignInCallback {
        void onSuccess(String token, String name);
        void onFailure(String error);
    }

    public void SignInWithGoogle(String WEB_CLIENT_ID, String nonce, Context context,
                                 CredentialManagerCallback<GetCredentialResponse, GetCredentialException> callback)
    {
        Log.d("Unity", "JAVA: SignInWithGoogle() called");
        Log.d("Unity", "JAVA: WEB_CLIENT_ID = " + WEB_CLIENT_ID);
        Log.d("Unity", "JAVA: nonce = " + nonce);

        GetSignInWithGoogleOption signInWithGoogleOption = new GetSignInWithGoogleOption.Builder(WEB_CLIENT_ID)
                .setNonce(nonce)
                .build();

        GetCredentialRequest request = new GetCredentialRequest.Builder()
                .addCredentialOption(signInWithGoogleOption)
                .build();

        Log.d("Unity", "JAVA: running CredentialManager.getCredentialAsync");

        CredentialManager credentialManager = CredentialManager.create(context);
        credentialManager.getCredentialAsync(context,request,new CancellationSignal(),Executors.newSingleThreadExecutor(),callback);
    }

    public void handleFailure(GetCredentialException e)
    {
        Log.e("Unity", "Received an invalid google id token response", e);
    }

    public void handleSignIn(GetCredentialResponse result)
    {
        Log.d("Unity", "JAVA: handleSignIn() fired");
        Credential credential = result.getCredential();

        if (credential instanceof CustomCredential)
        {
            Log.d("Unity", "JAVA: Got CustomCredential type = " + credential.getType());
            if (GoogleIdTokenCredential.TYPE_GOOGLE_ID_TOKEN_CREDENTIAL.equals(credential.getType()))
            {
                try
                {
                    GoogleIdTokenCredential googleIdTokenCredential = GoogleIdTokenCredential.createFrom(credential.getData());
                    name = googleIdTokenCredential.getDisplayName();
                    token = googleIdTokenCredential.getIdToken();

                    Log.d("Unity", "Google Sign-In SUCCESS");
                    Log.d("Unity", "DisplayName = " + name);
                    Log.d("Unity", "Token (first 20 chars) = " + token.substring(0,20));
                }
                catch (Exception e) {
                    Log.e("Unity", "Failed to parse Google ID token", e);
                }
            }
        }
    }
}

