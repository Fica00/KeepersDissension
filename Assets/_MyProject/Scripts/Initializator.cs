using System;
using Firebase.Auth;
using Firebase.Database;
using UnityEngine;
using SignInResult = FirebaseAuthHandler.SignInResult;

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
        if (_credentials == null)
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

    private void ManageNotifications()
    {
        TokenHandler.Instance.Init(FirebaseAuth.DefaultInstance, FirebaseDatabase.DefaultInstance.RootReference);
        FirebaseNotificationHandler.Instance.Init(FirebaseAuth.DefaultInstance);
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
        ManageNotifications();

        Debug.Log("Player's current room id " + DataManager.Instance.PlayerData.CurrentRoomId);

        LoadAppropriateScene();
    }
    
    private void LoadAppropriateScene()
    {
        if (!string.IsNullOrEmpty(DataManager.Instance.PlayerData.CurrentRoomId))
        {
            FirebaseManager.Instance.CheckIfRoomExists(DataManager.Instance.PlayerData.CurrentRoomId, _result =>
            {
                if (_result)
                {
                    try
                    {
                        //hvata mi nullreferenceexception kada hocu da se subscribe, koliko sam skapirao posto je roomData u RoomHandleru null jer se nigde ne setuje kada se udje u igricu opet
                        //vidim da se roomData setuje u JoinRoom i CreateRoom, ali valjda mi ne trebaju te metode jer tehnicki on nije ni leave room kada samo izadje iz igrice?
                        
                        FirebaseManager.Instance.RoomHandler.SubscribeToRoom();
                        SceneManager.LoadGameplay();
                    }
                    catch (Exception _exception)
                    {
                        Debug.Log(_exception);
                    }

                    return;
                }

                DataManager.Instance.PlayerData.CurrentRoomId = string.Empty;
                SceneManager.LoadMainMenu();
            });
        }
        else
        {
            SceneManager.LoadMainMenu();
        }
    }
}