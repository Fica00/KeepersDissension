using Firebase.Database;
using UnityEngine;

public class Initialization : MonoBehaviour
{
    private void Awake()
    {
        Application.runInBackground = true;
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
        AuthenticationHandler.Instance.Init(CollectData);
    }

    private void CollectData()
    {
        FirebaseManager.Instance.CollectData(CheckForVerification);
    }

    private void CheckForVerification(bool _status)
    {
        if (!_status)
        {
            Authenticate();
            return;
        }

        AuthenticationHandler.Instance.VerifyAccount(SetupNotifications);
    }

    private void SetupNotifications()
    {
        FirebaseManager.Instance.RoomHandler.SetLocalPlayerId(FirebaseManager.Instance.Authentication.UserId);
        FirebaseNotificationHandler.Instance.Init(FirebaseDatabase.DefaultInstance.RootReference, LoadAppropriateScene);
    }

    private void LoadAppropriateScene()
    {
        DeviceIdHandler.Instance.Setup();
        SceneManager.LoadMainMenu();
    }
}