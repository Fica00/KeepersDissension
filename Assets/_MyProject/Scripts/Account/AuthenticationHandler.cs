using System;
using System.Collections;
using Newtonsoft.Json;
using UnityEngine;
using FirebaseAuthHandler;

public class AuthenticationHandler : MonoBehaviour
{
   private const string AUTH_CREDENTIALS = "AuthCredentials";

   public static AuthenticationHandler Instance;
   [SerializeField] private LoginUI login;
   [SerializeField] private RegisterUI register;
   [SerializeField] private ForgotPasswordUI forgotPassword;
   [SerializeField] private EmailVerificationUI emailVerificationUI;
   
   private Action authCallBack;
   private Action emailVerifyCallback;
   private AuthenticationCredentials credentials;
   

   private void Awake()
   {
      Instance = this;
   }

   public void Init(Action _callBack)
   {
      authCallBack = _callBack;
      credentials = GetAuthCredentials();
      
      if (credentials == null)
      {
         ShowLogin();
      }
      else
      {
         TryLogin(credentials);
      }
   }
   
   private AuthenticationCredentials GetAuthCredentials()
   {
      if (!PlayerPrefs.HasKey(AUTH_CREDENTIALS))
      {
         return null;
      }
      
      AuthenticationCredentials _credentials = JsonConvert.DeserializeObject<AuthenticationCredentials>(PlayerPrefs.GetString(AUTH_CREDENTIALS));
      return _credentials;
   }

   public void TryLogin(AuthenticationCredentials _credentials)
   {
      credentials = _credentials;
      FirebaseManager.Instance.Authentication.SignInEmail(_credentials.Email, _credentials.Password, FinishSignIn);
   }
   
   private void FinishSignIn(SignInResult _result)
   {
      if (!_result.IsSuccessful)
      {
         ShowLogin();
         DialogsManager.Instance.ShowOkDialog("Wrong email or password");
         return;
      }

      SaveCredentials();
      authCallBack?.Invoke();
   }

   public void TryToRegister(AuthenticationCredentials _credentials, string _name)
   {
      credentials = _credentials;
      FirebaseManager.Instance.Authentication.SignUpEmail(_credentials.Email,_credentials.Password, _result =>
      {
         HandleRegister(_result, _name);
      });
   }
   
   private void HandleRegister(SignInResult _result, string _name)
   {
      if (!_result.IsSuccessful)
      {  
         Debug.Log(6666666);
         ShowRegister();
         DialogsManager.Instance.ShowOkDialog("Email already in use");
         return;
      }

      Debug.Log(77777);
      FirebaseManager.Instance.SendEmailVerification();
      DataManager.Instance.CreateNewPlayer();
      DataManager.Instance.PlayerData.Name = _name;
      string _playerData = JsonConvert.SerializeObject(DataManager.Instance.PlayerData);
      FirebaseManager.Instance.UpdatePlayerData(_playerData,HandleFinishedUpdatingPlayerData);
   }
   
   private void HandleFinishedUpdatingPlayerData(bool _result)
   {
      Debug.Log(8888888);
      if (!_result)
      {
         Debug.Log(9999999);
         DialogsManager.Instance.ShowOkDialog("Something went wrong while setting starting data");
         return;
      }

      SaveCredentials();
      authCallBack?.Invoke();
   }

   private void SaveCredentials()
   {
      PlayerPrefs.SetString(AUTH_CREDENTIALS, JsonConvert.SerializeObject(credentials));
   }

   public void SendPasswordReset(string _email)
   {
      FirebaseManager.Instance.SendPasswordResetEmail(_email,HandlePasswordSentResult);
   }
   
   private void HandlePasswordSentResult(bool _result)
   {
      if (!_result)
      {
         DialogsManager.Instance.ShowOkDialog("Something went wrong, please check email or try again later");
         return;
      }
      
      DialogsManager.Instance.ShowOkDialog("Email with instructions to reset your password has been sent");
      ShowLogin();
   }
   
   public void ShowLogin()
   {
      SwitchPanel(login);
   }

   public void ShowRegister()
   {
      SwitchPanel(register);
   }

   public void ShowForgotPassword()
   {
      SwitchPanel(forgotPassword);
   }

   public void VerifyAccount(Action _callBack)
   {
      emailVerifyCallback = _callBack;
      StartCoroutine(VerifyAccountRoutine());
   }

   public void RecheckAccountVerification()
   {
      StartCoroutine(VerifyAccountRoutine());
   }

   private IEnumerator VerifyAccountRoutine()
   {
      var _user = FirebaseManager.Instance.Authentication.FirebaseUser;

      var _reloadTask = _user.ReloadAsync();
      yield return new WaitUntil(() => _reloadTask.IsCompleted);

      if (_user.IsEmailVerified || credentials.IsAnonymous)
      {
         Debug.Log("Email is verified!");
         emailVerifyCallback?.Invoke();
         yield break;
      }

      ShowVerifyEmail();
   }
   
   private void ShowVerifyEmail()
   {
      SwitchPanel(emailVerificationUI);
   }

   private void SwitchPanel(IPanel _panel)
   {
      login.Close();
      register.Close();
      forgotPassword.Close();
      emailVerificationUI.Close();
      
      _panel.Setup();
   }
}