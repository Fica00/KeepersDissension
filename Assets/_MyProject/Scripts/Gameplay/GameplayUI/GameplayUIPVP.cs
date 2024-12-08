using FirebaseMultiplayer.Room;

public class GameplayUIPVP : GameplayUI
{
    public override void ShowResult(bool _didIWin)
    {
        if (_didIWin)
        {
            DialogsManager.Instance.ShowOkDialog("You won!",LeaveRoom);
        }
        else
        {
            DialogsManager.Instance.ShowOkDialog("You lost!",LeaveRoom);
        }
    }
    
    private void LeaveRoom()
    {
        RoomHandler.OnILeftRoom += ShowMainMenu;
        FirebaseManager.Instance.RoomHandler.LeaveRoom();
    }

    private void ShowMainMenu()
    {
        RoomHandler.OnILeftRoom -= ShowMainMenu;
        SceneManager.LoadMainMenu();
    }
}
