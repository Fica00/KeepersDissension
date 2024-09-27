using System;
using UnityEngine;
using FirebaseAuthHandler;
using Firebase;
using Firebase.Auth;
using Firebase.Database;
using Firebase.Extensions;
using FirebaseMultiplayer.Room;
using Newtonsoft.Json;

public class FirebaseManager : MonoBehaviour
{
    private const string USERS_KEY = "users";
    private const string GAME_DATA_KEY = "gameData";
    private const string ROOMS_KEY = "rooms";

    public static FirebaseManager Instance;
    public FirebaseAuthentication Authentication = new();
    public RoomHandler RoomHandler = new();
    private DatabaseReference database;

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
                SubscribeToDeviceIdChanges(OnDeviceIdChanged);

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
                UIManager.Instance.ShowYesNoDialog("Looks like you are already signed in on another device. Do you want to logout from other devices and continue?",
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
        DataManager.Instance.PlayerData.DeviceId = SystemInfo.deviceUniqueIdentifier;
        CollectGameData(_callBack);
    }
    
    private void NoDontLogout(Action<bool> _callBack)
    {
        Authentication.SignOut();
        PlayerPrefs.DeleteAll();
        PlayerPrefs.Save();
        _callBack?.Invoke(false);
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
            UIManager.Instance.ShowOkDialog(_result.Message);
            _callBack?.Invoke(_result.IsSuccessful);
        });
    }

    public void SaveValue<T>(string _path, T _value)
    {
        database.Child(USERS_KEY).Child(Authentication.UserId).Child(_path).SetValueAsync(_value);
    }

    public void CheckIfRoomExists(string _roomId, Action<RoomData> _callBack)
    {
        database.Child(GAME_DATA_KEY).Child(ROOMS_KEY).Child(_roomId).GetValueAsync().ContinueWithOnMainThread(_task =>
        {
            if (_task.IsFaulted)
            {
                _callBack?.Invoke(null);
            }
            else if (_task.IsCompleted)
            {
                if (_task.Result.Exists)
                {
                    RoomData _roomData = JsonConvert.DeserializeObject<RoomData>(_task.Result.GetRawJsonValue());

                    if (_roomData.Status == RoomStatus.MatchedUp)
                    {
                        _callBack?.Invoke(_roomData);
                        return;
                    }
                }

                _callBack?.Invoke(null);
            }
        });
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
    
    private void SubscribeToDeviceIdChanges(Action<string> _onDeviceIdChanged)
    {
        if (!Authentication.IsSignedIn)
        {
            return;
        }
        string _userId = Authentication.UserId; // Assuming this is already set
        DatabaseReference _deviceIdRef = FirebaseDatabase.DefaultInstance
            .GetReference($"users/{_userId}/DeviceId");

        _deviceIdRef.ValueChanged += (_, _args) =>
        {
            if (_args.DatabaseError != null)
            {
                Debug.LogError($"Database error: {_args.DatabaseError.Message}");
                return;
            }

            if (_args.Snapshot.Exists && _args.Snapshot.Value != null)
            {
                string _newDeviceId = _args.Snapshot.Value.ToString();
                _onDeviceIdChanged?.Invoke(_newDeviceId);
            }
            else
            {
                _onDeviceIdChanged?.Invoke(string.Empty);
            }
        };
    }
    
    private void OnDeviceIdChanged(string _newDeviceId)
    {
        if (SystemInfo.deviceUniqueIdentifier == _newDeviceId)
        {
            return;
        }
        
        UIManager.Instance.ShowOkDialog("Please log in to continue", () =>
        {
            Authentication.SignOut();
            PlayerPrefs.DeleteAll();
            PlayerPrefs.Save();
        });
    }
}