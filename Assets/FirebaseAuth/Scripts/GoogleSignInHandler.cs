using System;
using System.Threading.Tasks;
using Firebase.Auth;
using Firebase.Extensions;
using Google;

namespace FirebaseAuthHandler
{
    public static class GoogleSignInHandler
    {
        private const string WEB_CLIENT_ID = "62974547586-jrt3tf0ubca74kn9mc4o1m6r9bn4q46u.apps.googleusercontent.com";
        private static GoogleSignInConfiguration configuration;

        static GoogleSignInHandler()
        {
            configuration = new GoogleSignInConfiguration { WebClientId = WEB_CLIENT_ID, RequestIdToken = true };
        }

        public static void SignIn(Action<GoogleSignInResult> _callBack)
        {
            GoogleSignIn.Configuration = configuration;
            GoogleSignIn.Configuration.UseGameSignIn = false;
            GoogleSignIn.Configuration.RequestIdToken = true;
            GoogleSignIn.Configuration.RequestEmail = true;

            GoogleSignIn.DefaultInstance.SignIn().ContinueWithOnMainThread(_task => OnGoogleSignInFinished(_task, _callBack));
        }

        private static void OnGoogleSignInFinished(Task<GoogleSignInUser> _task, Action<GoogleSignInResult> _callBack)
        {
            GoogleSignInResult _result = new();
            if (_task.IsFaulted)
            {
                _result.Message = "There was a fault, if this keeps happening please contact support:" + _task.Exception;
            }
            else if (_task.IsCanceled)
            {
                _result.Message = "SignIn was canceled";
            }
            else
            {
                string _token = _task.Result.IdToken;
                Credential _credential = GoogleAuthProvider.GetCredential(_token, null);
                _result.Credential = _credential;
                _result.IsSuccessful = true;
            }

            _callBack?.Invoke(_result);
        }
    }
}