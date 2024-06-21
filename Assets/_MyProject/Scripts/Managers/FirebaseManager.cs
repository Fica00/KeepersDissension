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
                UIManager.Instance.ShowOkDialog("Please logout from other device and try again!");
                Authentication.SignOut();
                PlayerPrefs.DeleteAll();
                PlayerPrefs.Save();
                _callBack?.Invoke(false);
                return;
            }

            CollectGameData(_callBack);
        });
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
}