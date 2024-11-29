using System;
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
   
   private Action authCallBack;
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
         ShowRegister();
         DialogsManager.Instance.ShowOkDialog("Email already in use");
         return;
      }

      DataManager.Instance.CreateNewPlayer();
      DataManager.Instance.PlayerData.Name = _name;
      string _playerData = JsonConvert.SerializeObject(DataManager.Instance.PlayerData);
      FirebaseManager.Instance.UpdatePlayerData(_playerData,HandleFinishedUpdatingPlayerData);
   }
   
   private void HandleFinishedUpdatingPlayerData(bool _result)
   {
      if (!_result)
      {
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

   private void SwitchPanel(IPanel _panel)
   {
      login.Close();
      register.Close();
      forgotPassword.Close();
      
      _panel.Setup();
   }
}