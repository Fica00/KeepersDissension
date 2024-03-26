using FirebaseAuthHandler;
using UnityEngine;

public class Initializator : MonoBehaviour
{
   public static Initializator Instance;

   private void Awake()
   {
      Instance = this;
   }

   private void OnEnable()
   {
      SplashAnimation.OnFinished += InitSOs;
   }

   private void OnDisable()
   {
      SplashAnimation.OnFinished -= InitSOs;
   }

   private void InitSOs()
   {
      FactionSO.Init();
      InitFirebase();
   }

   private void InitFirebase()
   {
      FirebaseManager.Instance.Init(Authenticate);
   }

   private void Authenticate()
   {
      AuthenticationCredentials _credentials = DataManager.GetAuthCredentials();
      if (_credentials==null)
      {
         AuthenticationUI.Instance.ShowLogin();
      }
      else
      {
         if (SettingsPanel.IsSigningOut)
         {
            SettingsPanel.IsSigningOut = false;
            AuthenticationUI.Instance.ShowLogin();
            return;
         }
         FirebaseManager.Instance.Authentication.SignInEmail(_credentials.Email, _credentials.Password, FinishSignIn);
      }
   }

   private void FinishSignIn(SignInResult _result)
   {
      if (!_result.IsSuccessful)
      {
         AuthenticationUI.Instance.ShowLogin();
         UIManager.Instance.ShowOkDialog("Wrong email or password");
         return;
      }

      CollectData();
   }

   public void CollectData()
   {
      FirebaseManager.Instance.CollectData(FinishInit);
   }

   private void FinishInit(bool _status)
   {
      if (!_status)
      {
         AuthenticationUI.Instance.ShowLogin();
         return;
      }

      FirebaseManager.Instance.RoomHandler.SetLocalPlayerId(FirebaseManager.Instance.Authentication.UserId);
      SceneManager.LoadMainMenu();
   }

 
}
