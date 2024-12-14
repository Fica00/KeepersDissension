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
        Debug.Log(666666666);
        FirebaseManager.Instance.CollectData(CheckForVerification);
    }

    private void CheckForVerification(bool _status)
    {
        Debug.Log(777777);
        if (!_status)
        {
            Debug.Log(8888);
            Authenticate();
            return;
        }

        Debug.Log(999999);
        AuthenticationHandler.Instance.VerifyAccount(SetupNotifications);
    }

    private void SetupNotifications()
    {
        Debug.Log("aaaaaaaaa");
        FirebaseManager.Instance.RoomHandler.SetLocalPlayerId(FirebaseManager.Instance.Authentication.UserId);
        FirebaseNotificationHandler.Instance.Init(FirebaseDatabase.DefaultInstance.RootReference, LoadAppropriateScene);
    }

    private void LoadAppropriateScene()
    {
        Debug.Log(FirebaseManager.Instance.Authentication.UserId);
        DeviceIdHandler.Instance.Setup();
        SceneManager.LoadMainMenu();
    }
}