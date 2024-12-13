public static class SceneManager
{
   private const string MAIN_MENU = "MainMenu";
   private const string GAMEPLAY = "Gameplay";
   private const string DATA_COLLECTOR = "DataCollector";

   public static bool IsGameplayScene => UnityEngine.SceneManagement.SceneManager.GetActiveScene().name == GAMEPLAY;
   
   public static void LoadMainMenu()
   {
      LoadScene(MAIN_MENU);   
   }

   public static void LoadGameplay()
   {
      LoadScene(GAMEPLAY);
   }

   public static void LoadDataCollector()
   {
      LoadScene(DATA_COLLECTOR);
   }

   private static void LoadScene(string _key)
   {
      UnityEngine.SceneManagement.SceneManager.LoadScene(_key);
   }
}
