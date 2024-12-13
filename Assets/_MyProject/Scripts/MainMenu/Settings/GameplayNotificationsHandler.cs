using UnityEngine;
using UnityEngine.UI;

public class GameplayNotificationsHandler : MonoBehaviour
{
    [SerializeField] private Toggle toggle;
    
    private void OnEnable()
    {
        toggle.onValueChanged.AddListener(UpdateGameplayNotifications);
        toggle.isOn = DataManager.Instance.PlayerData.GameplayNotifications;
    }

    private void OnDisable()
    {
        toggle.onValueChanged.RemoveListener(UpdateGameplayNotifications);
    }
    
    private void UpdateGameplayNotifications(bool _isOn)
    {
        DataManager.Instance.PlayerData.GameplayNotifications = _isOn;
    }
}