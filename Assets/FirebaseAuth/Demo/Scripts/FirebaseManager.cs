using System;
using Firebase;
using Firebase.Auth;
using Firebase.Extensions;
using FirebaseAuthHandler;

namespace AuthDemo
{
    public static class FirebaseManager
    {
        public static FirebaseAuthentication Authentication = new ();
        
        public static void Init(Action _callBack)
        {
            FirebaseApp.CheckAndFixDependenciesAsync().ContinueWithOnMainThread(_result =>
            {
                if (_result.Result == DependencyStatus.Available)
                {
                    Authentication.Init(FirebaseAuth.DefaultInstance);
                    _callBack?.Invoke();
                }
                else
                {
                    throw new Exception("Couldn't fix dependencies in FirebaseManager.cs");
                }
            });
        }
    }
}