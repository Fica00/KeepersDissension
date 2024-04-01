using UnityEngine;
using UnityEngine.UI;
using FirebaseAuthHandler;
using TMPro;

namespace AuthDemo
{
    public class Demo : MonoBehaviour
    {
        [SerializeField] private Button signInAnonymous;
        [SerializeField] private Button signInGoogle;
        [SerializeField] private Button signOut;
        
        [SerializeField] private Button signUpEmail;
        [SerializeField] private Button signInEmail;
        [SerializeField] private TMP_InputField emailInput;
        [SerializeField] private TMP_InputField passwordInput;

        private void OnEnable()
        {
            signOut.onClick.AddListener(SignOut);
            signInAnonymous.onClick.AddListener(SignInAsAnonymous);
            signUpEmail.onClick.AddListener(SignUpEmail);
            signInEmail.onClick.AddListener(SignInEmail);
        }

        private void OnDisable()
        {
            signOut.onClick.RemoveListener(SignOut);
            signInAnonymous.onClick.RemoveListener(SignInAsAnonymous);
            signUpEmail.onClick.RemoveListener(SignUpEmail);
            signInEmail.onClick.RemoveListener(SignInEmail);
        }

        private void SignOut()
        {
            SignOutResult _result = FirebaseManager.Authentication.SignOut();
            Debug.Log(_result.Message);
        }

        private void SignInAsAnonymous()
        {
            if (!CanSignIn())
            {
                return;
            }
            
            ManageInteractables(false);
            FirebaseManager.Authentication.SignInAnonymous(HandleSignIn);
        }

        private void SignUpEmail()
        {
            if (!CanSignIn())
            {
                return;
            }

            string _email = emailInput.text;
            string _password = passwordInput.text;
            if (!(ValidateEmail(_email) && ValidatePassword(_password)))
            {
                return;
            }
            
            ManageInteractables(false);
            FirebaseManager.Authentication.SignUpEmail(_email,_password,HandleSignIn);
        }
        
        private void SignInEmail()
        {
            if (!CanSignIn())
            {
                return;
            }

            string _email = emailInput.text;
            string _password = passwordInput.text;
            if (!(ValidateEmail(_email) && ValidatePassword(_password)))
            {
                return;
            }
            
            ManageInteractables(false);
            FirebaseManager.Authentication.SignInEmail(_email,_password,HandleSignIn);
        }

        private bool ValidateEmail(string _email)
        {
            if (string.IsNullOrEmpty(_email))
            {
                return false;
            }

            if (!_email.Contains('@'))
            {
                return false;
            }

            return true;
        }

        private bool ValidatePassword(string _password)
        {
            if (string.IsNullOrEmpty(_password))
            {
                return false;
            }

            if (_password.Length<6)
            {
                return false;
            }

            return true;
        }
        
        private bool CanSignIn()
        {
            if (FirebaseManager.Authentication.IsSignedIn)
            {
                Debug.Log("Already signed in");
                return false;
            }

            return true;
        }
        
        private void HandleSignIn(SignInResult _result)
        {
            ManageInteractables(true);
            
            if (!_result.IsSuccessful)
            {
                Debug.Log(_result.Message);
                return;
            }
            
            Debug.Log("Successfully signed in");
        }
        
        private void Start()
        {
            ManageInteractables(false);
            FirebaseManager.Init(() =>
            {
                ManageInteractables(true);
            });
        }
        
        private void ManageInteractables(bool _status)
        {
            signInAnonymous.interactable = _status;
            signOut.interactable = _status;
            signInGoogle.interactable = _status;
            signInEmail.interactable = _status;
            signUpEmail.interactable = _status;
            emailInput.interactable = _status;
            passwordInput.interactable = _status;
        }
    }

}
