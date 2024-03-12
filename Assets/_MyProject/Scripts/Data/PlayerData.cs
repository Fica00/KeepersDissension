using System;
using Newtonsoft.Json;

[Serializable]
public class PlayerData
{
   private string name;
   private int matchesPlayed;
   private DateTime dateCreated;
   private bool playMusic= true;
   private bool playSoundEffect = true;
   private bool gameplayNotifications=true;
   private string deviceId;
   
   [JsonIgnore] public int FactionId;

   public static Action OnUpdatedName;
   public static Action OnUpdatedMusic;
   public static Action OnUpdatedSoundEffects;
   public static Action OnUpdatedMatchesPlayed;
   public static Action OnUpdatedGameplayNotifications;
   public static Action OnUpdatedDeviceId;

   public string Name
   {
      get => name;
      set
      {
         name = value;
         OnUpdatedName?.Invoke();
      }
   }
   
   public bool PlayMusic
   {
      get
      {
         return playMusic;
      }
      set
      {
         playMusic = value;
         OnUpdatedMusic?.Invoke();
      }
   }

   public bool PlaySoundEffect
   {
      get
      {
         return playSoundEffect;
      }
      set
      {
         playSoundEffect = value;
         OnUpdatedSoundEffects?.Invoke();
      }
   }
   
   public int MatchesPlayed
   {
      get => matchesPlayed;
      set
      {
         matchesPlayed = value;
         OnUpdatedMatchesPlayed?.Invoke();
      }
   }

   public DateTime DateCreated
   {
      get => dateCreated;
      set => dateCreated = value;
   }

   public bool GameplayNotifications
   {
      get => gameplayNotifications;
      set
      {
         gameplayNotifications = value;
         OnUpdatedGameplayNotifications?.Invoke();
      }
   }

   public string DeviceId
   {
      get => deviceId;
      set
      {
         deviceId = value;
         OnUpdatedDeviceId?.Invoke();
      }
   }
}