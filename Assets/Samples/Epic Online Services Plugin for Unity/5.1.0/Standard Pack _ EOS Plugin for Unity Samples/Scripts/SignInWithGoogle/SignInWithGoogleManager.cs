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

using UnityEngine;

namespace PlayEveryWare.EpicOnlineServices.Samples
{
    public interface IGoogleSignInManager
    {
        public void GetGoogleIdToken(System.Action<string, string> callback);
    }

    public class SignInWithGoogleManager : MonoBehaviour, IGoogleSignInManager
    {
        AndroidJavaObject loginObject;

        public void GetGoogleIdToken(System.Action<string, string> callback)
        {
            using AndroidJavaClass unityPlayer = new("com.unity3d.player.UnityPlayer");
            using AndroidJavaObject activity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");
            if (activity == null)
            {
                Debug.LogError("EOSAndroid: activity context is null!");
                print("EOSAndroid: activity context is null!");
                return;
            }
            print("C#: Activity valid");

            using AndroidJavaClass loginClass = new AndroidJavaClass("com.playeveryware.googlelogin.LoginKT");
            if (loginClass == null)
            {
                Debug.LogError("Java Login Class is null!");
                print("Java Login Class is null!");
                return;
            }

            print("C#: Java Login Class loaded");
            loginObject = loginClass.CallStatic<AndroidJavaObject>("instance");

            if (loginObject == null)
            {
                print("ERROR: loginObject is NULL from Java");
                return;
            }

            print("C#: loginObject instance created");

            /// Create the proxy class and pass instances to be used by the callback
            EOSCredentialManagerCallback javaCallback = new EOSCredentialManagerCallback();
            javaCallback.loginObject = loginObject;
            javaCallback.callback = callback;

            AndroidConfig config = AndroidConfig.Get<AndroidConfig>();
            config.GoogleLoginNonce = System.Guid.NewGuid().ToString("N");

            print("C#: Using WEB_CLIENT_ID = " + config.GoogleLoginClientID);
            print("C#: Using nonce = " + config.GoogleLoginNonce);

            if (string.IsNullOrEmpty(config.GoogleLoginClientID))
            {
                Debug.LogError("Client ID is null, needs to be configured for Google ID connect login");
                print("Client ID is null, needs to be configured for Google ID connect login");
                return;
            }

            print("C#: Calling Java SignInWithGoogle...");
            /// SignInWithGoogle(String WEB_CLIENT_ID, String nonce, Context context, CredentialManagerCallback callback)
            loginObject.Call("SignInWithGoogle", config.GoogleLoginClientID, config.GoogleLoginNonce, activity, javaCallback);
            print("C#: Call to Java SignInWithGoogle COMPLETED");
        }

        class EOSCredentialManagerCallback : AndroidJavaProxy
        {
            public AndroidJavaObject loginObject;
            public System.Action<string, string> callback;

            /// <summary>
            /// Proxy class to receive Android callbacks in C#
            /// </summary>
            public EOSCredentialManagerCallback() : base("androidx.credentials.CredentialManagerCallback")
            {
                print("UNITY PROXY: EOSCredentialManagerCallback constructor called");
            }

            /// <summary>
            /// Succeeding Callback of GetCredentialAsync  
            /// GetCredentialAsync is called in com.playeveryware.googlelogin.login.SignInWithGoogle)
            /// </summary>
            /// <param name="credentialResponseResult"></param>
            public void onResult(AndroidJavaObject credentialResponseResult)
            {
                print("UNITY CALLBACK: Google onResult fired");
                /// Parses the response resilt into google credentials
                loginObject.Call("handleSignIn", credentialResponseResult);

                string token = loginObject.Call<string>("getResultIdToken");
                string name = loginObject.Call<string>("getResultName");

                print("UNITY CALLBACK: token = " + (token != null ? token.Substring(0, 20) : "NULL"));
                print("UNITY CALLBACK: name = " + name);

                /// Invoke Connect Login with fetched Google ID
                callback.Invoke(token, name);
            }

            /// <summary>
            /// Failing Callback of GetCredentialAsync  
            /// GetCredentialAsync is called in com.playeveryware.googlelogin.login.SignInWithGoogle)
            /// </summary>
            /// <param name="credentialException"></param>
            public void onError(AndroidJavaObject credentialException)
            {
                print("UNITY CALLBACK: Google SIGN-IN ERROR");
                loginObject.Call("handleFailure", credentialException);
                callback.Invoke(null, null);
            }
        }
    }
}