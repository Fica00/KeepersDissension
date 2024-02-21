public class GameplayUIPVP : GameplayUI
{
    public override void ShowResult(bool _didIWin)
    {
        if (_didIWin)
        {
            UIManager.Instance.ShowOkDialog("You won!",LeaveRoom);
        }
        else
        {
            UIManager.Instance.ShowOkDialog("You lost!",LeaveRoom);
        }
    }
    
    private void LeaveRoom()
    {
        PhotonManager.OnILeftRoom += ShowMainMenu;
        PhotonManager.Instance.LeaveRoom();
    }

    private void ShowMainMenu()
    {
        PhotonManager.OnILeftRoom -= ShowMainMenu;
        SceneManager.LoadMainMenu();
    }
}
