using System.Collections.Generic;
using Newtonsoft.Json;
using UnityEditor.Rendering;
using UnityEngine;

public class NotificationSender : MonoBehaviour
{
    public static NotificationSender Instance;

    private const string SEND_NOTIFICATION = "https://sendnotificationtodevice-e3mmrpwoya-uc.a.run.app";
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

    public void SendNotificationToUser(string _userId, string _title, string _body)
    {
        if (GameplayManager.Instance.IsExecutingOldActions)
        {
            return;
        }
        TokenHandler.Instance.GetTokenForUser(_userId, _token =>
        {
            if (_token == null)
            {
                return;
            }

            SendNotificationToToken(_token, _title, _body);
        });
    }

    private void SendNotificationToToken(string _token, string _title, string _body)
    {
        if (GameplayManager.Instance.IsExecutingOldActions)
        {
            return;
        }
        return;
        Debug.Log("sending notif");
        WebRequests.Instance.SetUserToken(_token);

        Dictionary<string, object> _data = new() { { "token", _token }, { "title", _title }, { "body", _body } };

        WebRequests.Instance.Post(SEND_NOTIFICATION, JsonConvert.SerializeObject(_data),
            _response => { Debug.Log("Notification sent successfully, response: " + _response); },
            _response => { Debug.LogError("Error calling function: " + _response); });
    }
}