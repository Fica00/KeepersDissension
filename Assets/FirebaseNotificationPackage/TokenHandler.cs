using System;
using Firebase.Auth;
using Firebase.Database;
using Firebase.Extensions;
using Firebase.Messaging;
using UnityEngine;

public class TokenHandler : MonoBehaviour
{
    public static TokenHandler Instance;

    private FirebaseAuth auth;
    private DatabaseReference databaseReference;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(this);
        }
    }

    public void Init(FirebaseAuth _auth, DatabaseReference _databaseReference)
    {
        auth = _auth;
        databaseReference = _databaseReference;
    }

    public void FetchAndUpdateUserTokens()
    {
        FirebaseMessaging.GetTokenAsync().ContinueWithOnMainThread(_task =>
        {
            if (!_task.IsFaulted && !_task.IsCanceled)
            {
                string _newToken = _task.Result;
                UpdateToken(auth.CurrentUser.UserId, _newToken);
            }
            else
            {
                Debug.LogError("Failed to get FCM token.");
            }
        });
    }

    public void UpdateToken(string _userId, string _newToken)
    {
        DatabaseReference _tokenRef = databaseReference.Child("notifications").Child("users").Child(_userId).Child("fcmToken");

        _tokenRef.SetValueAsync(_newToken).ContinueWithOnMainThread(_task =>
        {
            if (_task.IsFaulted)
            {
                Debug.LogError("Failed to set FCM token: " + _task.Exception);
            }
        });
    }

    public void RemoveCurrentToken()
    {
        if (auth.CurrentUser == null)
        {
            return;
        }

        string _userId = auth.CurrentUser.UserId;
        DatabaseReference _tokenRef = databaseReference.Child("notifications").Child("users").Child(_userId).Child("fcmToken");

        _tokenRef.RemoveValueAsync().ContinueWithOnMainThread(_task =>
        {
            if (_task.IsFaulted || _task.IsCanceled)
            {
                Debug.LogError("Failed to remove FCM token.");
            }
            else
            {
                Debug.Log("FCM token removed successfully.");
            }
        });
    }

    public void GetTokenForUser(string _userId, Action<string> _callback)
    {
        DatabaseReference _tokenRef = databaseReference.Child("notifications").Child("users").Child(_userId).Child("fcmToken");

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
