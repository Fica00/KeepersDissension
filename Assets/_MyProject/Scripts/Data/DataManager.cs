using Newtonsoft.Json;
using UnityEngine;

public class DataManager : MonoBehaviour
{
    public static DataManager Instance;
    public PlayerData PlayerData { get; private set; }
    public GameData GameData { get; private set; }

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

    private void OnDisable()
    {
        PlayerData.OnUpdatedMatchesPlayed -= SaveMatchesPlayed;
        PlayerData.OnUpdatedName -= SaveName;
        PlayerData.OnUpdatedMusic -= SaveMusic;
        PlayerData.OnUpdatedGameplayNotifications -= SaveGameplayNotifications;
        PlayerData.OnUpdatedSoundEffects -= SaveSoundEffects;
        PlayerData.OnUpdatedDeviceId -= SaveDeviceId;
        PlayerData.OnUpdatedCurrentRoomId -= SaveCurrentRoomId;
    }

    public void CreateNewPlayer()
    {
        PlayerData = new PlayerData();
    }

    public void SetGameData(string _dataJson)
    {
        GameData = JsonConvert.DeserializeObject<GameData>(_dataJson);
    }

    public void SetPlayerData(string _dataJson)
    {
        PlayerData = JsonConvert.DeserializeObject<PlayerData>(_dataJson);
        SubscribeEvents();
    }

    private void SubscribeEvents()
    {
        PlayerData.OnUpdatedMatchesPlayed += SaveMatchesPlayed;
        PlayerData.OnUpdatedName += SaveName;
        PlayerData.OnUpdatedMusic += SaveMusic;
        PlayerData.OnUpdatedGameplayNotifications += SaveGameplayNotifications;
        PlayerData.OnUpdatedSoundEffects += SaveSoundEffects;
        PlayerData.OnUpdatedDeviceId += SaveDeviceId;
        PlayerData.OnUpdatedCurrentRoomId += SaveCurrentRoomId;
    }

    private void SaveMatchesPlayed()
    {
        FirebaseManager.Instance.SaveValue(nameof(PlayerData.MatchesPlayed), PlayerData.MatchesPlayed);
    }
    
    private void SaveCurrentRoomId()
    {
        FirebaseManager.Instance.SaveValue(nameof(PlayerData.CurrentRoomId), PlayerData.CurrentRoomId);
    }

    private void SaveName()
    {
        FirebaseManager.Instance.SaveValue(nameof(PlayerData.Name), PlayerData.Name);
    }

    private void SaveMusic()
    {
        FirebaseManager.Instance.SaveValue(nameof(PlayerData.PlayMusic), PlayerData.PlayMusic);
    }

    private void SaveDeviceId()
    {
        FirebaseManager.Instance.SaveValue(nameof(PlayerData.DeviceId), PlayerData.DeviceId);
    }

    private void SaveGameplayNotifications()
    {
        FirebaseManager.Instance.SaveValue(nameof(PlayerData.GameplayNotifications), PlayerData.GameplayNotifications);
    }


    private void SaveSoundEffects()
    {
        FirebaseManager.Instance.SaveValue(nameof(PlayerData.PlaySoundEffect), PlayerData.PlaySoundEffect);
    }
}