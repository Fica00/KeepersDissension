using System;
using Firebase.Database;
using UnityEngine;

[Serializable]
public class DeviceIdHandler : MonoBehaviour
{
    public static DeviceIdHandler Instance;

    private void Awake()
    {
        if (Instance==null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void Setup()
    {
        SubscribeToDeviceIdChanges(OnDeviceIdChanged);
    }
    
    private void SubscribeToDeviceIdChanges(Action<string> _onDeviceIdChanged)
    {
        DatabaseReference _deviceIdRef = FirebaseDatabase.DefaultInstance
            .GetReference($"users/{FirebaseManager.Instance.Authentication.UserId}/DeviceId");

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

        if (string.IsNullOrEmpty(_newDeviceId))
        {
            return;
        }
        
        FirebaseManager.Instance.SignOut(SceneManager.LoadDataCollector);
        DialogsManager.Instance.ShowOkDialog("Please log in to continue");
    }
}
