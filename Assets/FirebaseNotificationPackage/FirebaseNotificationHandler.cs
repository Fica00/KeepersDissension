using System;
using System.Collections.Generic;
using Firebase.Database;
using Firebase.Extensions;
using Firebase.Messaging;
using Newtonsoft.Json;
using UnityEngine;

public class FirebaseNotificationHandler : MonoBehaviour
{
    private const string SEND_NOTIFICATION = "https://sendnotificationtodevice-e3mmrpwoya-uc.a.run.app";

    public static FirebaseNotificationHandler Instance;
    
    private DatabaseReference databaseReference;
    private Dictionary<string, string> cashedTokens = new ();

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

        DatabaseReference _tokenRef = databaseReference.Child("notifications").Child(FirebaseManager.Instance.Authentication.UserId).Child("fcmToken");

        _tokenRef.SetValueAsync(_token).ContinueWithOnMainThread(_task =>
        {
            _callBack?.Invoke();
        });
    }
    
    public void SendNotificationToUser(string _userId, string _title, string _body)
    {
        GetTokenForUser(_userId, _token =>
        {
            if (_token == null)
            {
                return;
            }

            SendNotificationToToken(_token, _title, _body);
        });
    }
    
    private void GetTokenForUser(string _userId, Action<string> _callback)
    {
        if (cashedTokens.TryGetValue(_userId, out var _cashedToken))
        {
            _callback?.Invoke(_cashedToken);
            return;   
        }
        
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
                cashedTokens.Add(_userId,_token);
                _callback?.Invoke(_token);
            }
            else
            {
                _callback?.Invoke(null);
            }
        });
    }

    private void SendNotificationToToken(string _token, string _title, string _body)
    {
        return;
        Debug.Log("sending notif");
        WebRequests.Instance.SetUserToken(_token);

        Dictionary<string, object> _data = new() { { "token", _token }, { "title", _title }, { "body", _body } };

        WebRequests.Instance.Post(SEND_NOTIFICATION, JsonConvert.SerializeObject(_data),
            _response => { Debug.Log("Notification sent successfully, response: " + _response); },
            _response => { Debug.LogError("Error calling function: " + _response); });
    }
}