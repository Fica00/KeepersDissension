using System;
using Firebase.Auth;
using Firebase.Messaging;
using UnityEngine;

public class FirebaseNotificationHandler : MonoBehaviour
{
    public static FirebaseNotificationHandler Instance;

    private FirebaseAuth auth;

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

    public void Init(FirebaseAuth _auth)
    {
        auth = _auth;
        // if (DataManager.Instance.PlayerData.GameplayNotifications)
        // {
        
        FirebaseMessaging.TokenReceived += OnTokenReceived;
        FirebaseMessaging.MessageReceived += OnMessageReceived;
        // }

        if (auth.CurrentUser != null)
        {
            TokenHandler.Instance.FetchAndUpdateUserTokens();
        }

        auth.StateChanged += OnAuthStateChanged;
    }

    private void OnAuthStateChanged(object _sender, EventArgs _eventArgs)
    {
        if (auth.CurrentUser != null)
        {
            TokenHandler.Instance.FetchAndUpdateUserTokens();
        }
    }

    private void OnTokenReceived(object _sender, TokenReceivedEventArgs _token)
    {
        TokenHandler.Instance.UpdateToken(auth.CurrentUser.UserId, _token.Token);
    }

    private void OnMessageReceived(object _sender, MessageReceivedEventArgs _e)
    {
        Debug.Log("Received a new message from FCM!");
    }

    private void OnDestroy()
    {
        FirebaseMessaging.TokenReceived -= OnTokenReceived;
        FirebaseMessaging.MessageReceived -= OnMessageReceived;

        if (auth != null)
        {
            auth.StateChanged -= OnAuthStateChanged;
        }
    }
    
    // public void ToggleNotifications(bool _isEnabled)
    // {
    //     // if (_isEnabled)
    //     // {
    //     //     SubscribeToNotifications();
    //     // }
    //     // else
    //     // {
    //     //     UnsubscribeFromNotifications();
    //     // }
    // }
}