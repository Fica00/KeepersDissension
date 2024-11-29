using System;
using Firebase.Database;
using Firebase.Extensions;
using Firebase.Messaging;
using UnityEngine;

public class FirebaseNotificationHandler : MonoBehaviour
{
    public static FirebaseNotificationHandler Instance;
    
    private DatabaseReference databaseReference;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(this);
        }
    }

    private void OnEnable()
    {
        FirebaseMessaging.MessageReceived += OnMessageReceived;
        FirebaseMessaging.TokenReceived += OnTokenReceived;
    }

    private void OnDisable()
    {
        FirebaseMessaging.MessageReceived -= OnMessageReceived;
        FirebaseMessaging.TokenReceived -= OnTokenReceived;
    }

    private void OnMessageReceived(object _sender, MessageReceivedEventArgs _e)
    {
        Debug.Log("Received a new message from FCM!");
    }

    private void OnTokenReceived(object _sender, TokenReceivedEventArgs _e)
    {
        SaveTokenToDatabase(_e.Token, null);
    }

    public void Init(DatabaseReference _databaseReference, Action _callBack)
    {
        databaseReference = _databaseReference;
        FirebaseMessaging.GetTokenAsync().ContinueWithOnMainThread(_task =>
        {
            if (_task.IsCompleted && !_task.IsFaulted && !_task.IsCanceled)
            {
                string _fcmToken = _task.Result;
                SaveTokenToDatabase(_fcmToken,_callBack);
            }
            else
            {
                _callBack?.Invoke();
            }
        });
    }
    
    private void SaveTokenToDatabase(string _token, Action _callBack)
    {
        if (databaseReference == null)
        {
            _callBack?.Invoke();
            return;
        }

        Debug.Log("------ Saving token");
        DatabaseReference _tokenRef = databaseReference.Child("notifications").Child(FirebaseManager.Instance.Authentication.UserId).Child("fcmToken");

        _tokenRef.SetValueAsync(_token).ContinueWithOnMainThread(_task =>
        {
            _callBack?.Invoke();
        });
    }
    
    public void GetTokenForUser(string _userId, Action<string> _callback)
    {
        DatabaseReference _tokenRef = databaseReference.Child("notifications").Child(_userId).Child("fcmToken");

        _tokenRef.GetValueAsync().ContinueWithOnMainThread(_task =>
        {
            if (_task.IsFaulted)
            {
                Debug.LogError("Error retrieving token: " + _task.Exception);
                _callback?.Invoke(null);
            }
            else if (_task.Result.Exists)
            {
                string _token = _task.Result.Value.ToString();
                _callback?.Invoke(_token);
            }
            else
            {
                _callback?.Invoke(null);
            }
        });
    }
}