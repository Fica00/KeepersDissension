using System;
using System.Collections;
using UnityEngine;
using FirebaseAuthHandler;
using Firebase;
using Firebase.Auth;
using Firebase.Database;
using Firebase.Extensions;
using FirebaseMultiplayer.Room;

public class FirebaseManager : MonoBehaviour
{
    private const string USERS_KEY = "users";
    private const string GAME_DATA_KEY = "gameData";
    private const string ROOMS_KEY = "rooms";

    public static FirebaseManager Instance;
    public FirebaseAuthentication Authentication = new();
    public NewRoomHandler RoomHandler = new();
    private DatabaseReference database;

    public string PlayerId => Authentication.UserId;
    public string OpponentId => RoomHandler.GetOpponent().Id;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void Init(Action _callBack)
    {
        FirebaseApp.CheckAndFixDependenciesAsync().ContinueWithOnMainThread(_result =>
        {
            if (_result.Result == DependencyStatus.Available)
            {
                Authentication.Init(FirebaseAuth.DefaultInstance);
                database = FirebaseDatabase.DefaultInstance.RootReference;
                RoomHandler.Init(database, $"{GAME_DATA_KEY}/{ROOMS_KEY}");
                
                _callBack?.Invoke();
            }
            else
            {
                throw new Exception("Couldn't fix dependencies in FirebaseManager.cs");
            }
        });
    }

    public void CollectData(Action<bool> _callBack)
    {
        CollectPlayerData(_callBack);
    }

    private void CollectPlayerData(Action<bool> _callBack)
    {
        database.Child(USERS_KEY).Child(Authentication.UserId).GetValueAsync().ContinueWithOnMainThread(_task =>
        {
            if (_task.IsCanceled)
            {
                _callBack?.Invoke(false);
            }
            else if (_task.IsFaulted)
            {
                _callBack?.Invoke(false);
            }

            string _result = _task.Result.GetRawJsonValue();
            DataManager.Instance.SetPlayerData(_result);
            
            if (string.IsNullOrEmpty(DataManager.Instance.PlayerData.DeviceId))
            {
                DataManager.Instance.PlayerData.DeviceId = SystemInfo.deviceUniqueIdentifier;
            }
            
            else if (DataManager.Instance.PlayerData.DeviceId != SystemInfo.deviceUniqueIdentifier)
            {
                DialogsManager.Instance.ShowYesNoDialog("Looks like you are already signed in on another device. Do you want to logout from other devices and continue?",
                    () =>
                    {
                        YesLogOut(_callBack);
                    }, () =>
                    {
                        NoDontLogout(_callBack);
                    });
                
                return;
            }

            CollectGameData(_callBack);
        });
    }

    private void YesLogOut(Action<bool> _callBack)
    {
        SignOut(() =>
        {
            _callBack?.Invoke(false);
        });
    }
    
    private void NoDontLogout(Action<bool> _callBack)
    {
        DataManager.Instance.PlayerData.DeviceId = SystemInfo.deviceUniqueIdentifier;
        CollectGameData(_callBack);
    }

    public void SignOut(Action _callBack = null)
    {
        StartCoroutine(SignOutRoutine());
        
        IEnumerator SignOutRoutine()
        {
            DataManager.Instance.PlayerData.DeviceId = string.Empty;
            yield return new WaitForSeconds(2);
            Authentication.SignOut();
            PlayerPrefs.DeleteAll();
            PlayerPrefs.Save();
            _callBack?.Invoke();
        }
    }

    private void CollectGameData(Action<bool> _callBack)
    {
        database.Child(GAME_DATA_KEY).GetValueAsync().ContinueWithOnMainThread(_task =>
        {
            if (_task.IsCanceled)
            {
                _callBack?.Invoke(false);
            }
            else if (_task.IsFaulted)
            {
                _callBack?.Invoke(false);
            }

            string _result = _task.Result.GetRawJsonValue();
            DataManager.Instance.SetGameData(_result);
            _callBack?.Invoke(true);
        });
    }

    public void UpdatePlayerData(string _data, Action<bool> _callBack)
    {
        database.Child(USERS_KEY).Child(Authentication.UserId).SetRawJsonValueAsync(_data).ContinueWithOnMainThread(_task =>
        {
            if (_task.IsCanceled || _task.IsFaulted)
            {
                Debug.Log("Failed to save player data");
                _callBack?.Invoke(false);
                return;
            }

            _callBack?.Invoke(true);
        });
    }

    public void SendPasswordResetEmail(string _email, Action<bool> _callBack)
    {
        Authentication.SendPasswordResetEmail(_email, _result =>
        {
            DialogsManager.Instance.ShowOkDialog(_result.Message);
            _callBack?.Invoke(_result.IsSuccessful);
        });
    }

    public void SaveValue<T>(string _path, T _value)
    {
        database.Child(USERS_KEY).Child(Authentication.UserId).Child(_path).SetValueAsync(_value);
    }

    public void DeleteUserDataAndAccount(Action<bool> _callBack)
    {
        database.Child(USERS_KEY).Child(Authentication.UserId).RemoveValueAsync().ContinueWithOnMainThread(_task =>
        {
            if (_task.IsCompleted)
            {
                DeleteUserAccount(_callBack);
            }
            else
            {
                _callBack?.Invoke(false);
            }
        });
    }

    private void DeleteUserAccount(Action<bool> _callBack)
    {
        Authentication.FirebaseUser.DeleteAsync().ContinueWithOnMainThread(_task => { _callBack?.Invoke(_task.IsCompleted); });
    }

    public void SendEmailVerification()
    {
        var _user = Authentication.FirebaseUser;
        _user.SendEmailVerificationAsync().ContinueWith(_task => 
        {
            if (_task.IsCanceled)
            {
                Debug.Log("SendEmailVerificationAsync was canceled.");
                return;
            }
            if (_task.IsFaulted)
            {
                Debug.Log("SendEmailVerificationAsync encountered an error: " + _task.Exception);
                return;
            }

            Debug.Log("Verification email sent successfully.");
        });
    }
}