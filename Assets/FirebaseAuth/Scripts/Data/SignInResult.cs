using System;

namespace FirebaseAuthHandler
{
    [Serializable]
    public class SignInResult
    {
        public bool IsSuccessful;
        public string Message;
    }
}