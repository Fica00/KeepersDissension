using System;
using Firebase.Auth;
using Firebase.Extensions;
using UnityEngine;

namespace FirebaseAuthHandler
{
    public class FirebaseAuthentication
    {
        private FirebaseAuth auth;
        private FirebaseUser firebaseUser;

        public bool IsSignedIn => firebaseUser != null;
        public FirebaseUser FirebaseUser => firebaseUser;
        public string UserId => firebaseUser.UserId;
        
        private bool isEmailVerificationSent;

        public void Init(FirebaseAuth _auth)
        {
            auth = _auth;
            firebaseUser = auth.CurrentUser;

            if (firebaseUser != null)
            {
                CheckEmailVerification();
            }
        }

        public void SignInAnonymous(Action<SignInResult> _callBack)
        {
            (bool _canSignIn, SignInResult _signInResult) = CanSignIn();

            if (!_canSignIn)
            {
                _callBack?.Invoke(_signInResult);
                return;
            }

            auth.SignInAnonymouslyAsync().ContinueWithOnMainThread(_task =>
            {
                SignInResult _result = new();
                if (_task.IsCanceled)
                {
                    _result.Message = "SignIn was canceled";
                }
                else if (_task.IsFaulted)
                {
                    _result.Message = "SignIn encountered an error: " + _task.Exception;
                }
                else
                {
                    _result.IsSuccessful = true;
                    firebaseUser = _task.Result.User;
                }

                _callBack?.Invoke(_result);
            });
        }

        private void HandleGoogleSignIn(GoogleSignInResult _result, Action<SignInResult> _callBack)
        {
            if (!_result.IsSuccessful)
            {
                _callBack?.Invoke(new SignInResult { IsSuccessful = false, Message = _result.Message });
                return;
            }

            LoginWithCredentials(_result.Credential, _callBack);
        }

        private void LoginWithCredentials(Credential _credential, Action<SignInResult> _callBack)
        {
            auth.SignInWithCredentialAsync(_credential).ContinueWithOnMainThread(_task =>
            {
                SignInResult _result = new();
                if (_task.Exception != null)
                {
                    _result.Message = "Failed to login: " + _task.Exception.Message;
                }
                else
                {
                    _result.IsSuccessful = true;
                    firebaseUser = _task.Result;
                }

                _callBack?.Invoke(_result);
            });
        }

        public void SignInEmail(string _email, string _password, Action<SignInResult> _callBack)
        {
            (bool _canSignIn, SignInResult _signInResult) = CanSignIn();

            if (!_canSignIn)
            {
                _callBack?.Invoke(_signInResult);
                return;
            }

            auth.SignInWithEmailAndPasswordAsync(_email, _password).ContinueWithOnMainThread(_task =>
            {
                SignInResult _result = new();
                if (_task.IsCanceled)
                {
                    _result.Message = "SignIn was canceled";
                }
                else if (_task.IsFaulted)
                {
                    _result.Message = "Email or password incorrect";
                }
                else
                {
                    _result.IsSuccessful = true;
                    firebaseUser = _task.Result.User;
                    isEmailVerificationSent = false;
                    
                    CheckEmailVerification();
                }

                _callBack?.Invoke(_result);
            });
        }

        public void SignUpEmail(string _email, string _password, Action<SignInResult> _callBack)
        {
            (bool _canSignIn, SignInResult _signInResult) = CanSignIn();

            if (!_canSignIn)
            {
                _callBack?.Invoke(_signInResult);
                return;
            }

            auth.CreateUserWithEmailAndPasswordAsync(_email, _password).ContinueWithOnMainThread(_task =>
            {
                SignInResult _result = new();
                if (_task.IsCanceled)
                {
                    _result.Message = "SignUp was canceled";
                }
                else if (_task.IsFaulted)
                {
                    _result.Message = "SignUp encountered an error: " + _task.Exception;
                }
                else
                {
                    _result.IsSuccessful = true;
                    firebaseUser = _task.Result.User;
                    isEmailVerificationSent = false;
                    
                    CheckEmailVerification();
                }

                _callBack?.Invoke(_result);
            });
        }
        private void SendEmailVerification(Action<bool> _callback)
        {
            if (firebaseUser != null)
            {
                firebaseUser.SendEmailVerificationAsync().ContinueWithOnMainThread(_task =>
                {
                    if (_task.IsCanceled)
                    {
                        _callback(false);
                    }
                    else if (_task.IsFaulted)
                    {
                        _callback(false);
                    }
                    else
                    {
                        _callback(true);
                    }
                });
            }
            else
            {
                _callback(false);
                Debug.Log("No signed in user to send verification email to.");
            }
        }

        private void CheckEmailVerification()
        {
            return;
            firebaseUser.ReloadAsync().ContinueWithOnMainThread(_task =>
            {
                if (_task.IsCompleted && !_task.IsFaulted)
                {
                    if (firebaseUser.IsEmailVerified)
                    {
                        return;
                    }
                    
                    if(!isEmailVerificationSent)
                    {
                        SendEmailVerification((_isSent) =>
                        {
                            Debug.Log(_isSent ? "Verification sent" : "Verification not sent");
                            isEmailVerificationSent = _isSent;
                        });
                    }
                    
                    DialogsManager.Instance.ShowOkDialog("Please check your email for verification link", CheckEmailVerification);
                }
                else
                {
                    Debug.LogError("Failed to reload user data.");
                }
            });
        }

        public void SendPasswordResetEmail(string _email, Action<SignInResult> _callBack)
        {
            auth.SendPasswordResetEmailAsync(_email).ContinueWith(_task =>
            {
                SignInResult _result = new SignInResult();
                if (_task.IsCanceled)
                {
                    _result.Message = "Password reset was canceled.";
                }
                else if (_task.IsFaulted)
                {
                    _result.Message = "Password reset encountered an error.";
                }
                else
                {
                    _result.Message = "Password reset email sent successfully.";
                    _result.IsSuccessful = true;
                }

                _callBack?.Invoke(_result);
            });
        }

        private (bool, SignInResult) CanSignIn()
        {
            SignInResult _result = new();
            bool _canSignIn;

            if (firebaseUser != null)
            {
                _result.Message = "Already signed in";
                _result.IsSuccessful = true;
                _canSignIn = false;
            }
            else
            {
                _canSignIn = true;
            }

            return (_canSignIn, _result);
        }

        public SignOutResult SignOut()
        {
            SignOutResult _result = new();
            if (firebaseUser == null)
            {
                _result.Message = "Not signed in";
            }
            else
            {
                auth.SignOut();
                firebaseUser = null;
                _result.Message = "Successfully signed out";
                _result.IsSuccessful = true;
            }

            return _result;
        }

        
    }
}