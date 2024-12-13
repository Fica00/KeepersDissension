using UnityEngine;
using UnityEngine.Events;

public class DialogsManager : MonoBehaviour
{
    public static DialogsManager Instance;

    [SerializeField] private OkDialog okDialogPrefab;
    [SerializeField] private OkDialog okDialogBigPrefab;
    [SerializeField] private YesNoDialog yesNoDialogPrefab;

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
    
    public void ShowOkDialog(string _message, UnityAction _onOkPressed=null)
    {
        if (SceneManager.IsGameplayScene && !DataManager.Instance.PlayerData.GameplayNotifications)
        {
            _onOkPressed?.Invoke();
            return;
        }
        OkDialog _okDialog = Instantiate(okDialogPrefab, transform);
        _okDialog.Show(_message,_onOkPressed);
    }
    
    public void ShowOkBigDialog(string _message, UnityAction _onOkPressed=null)
    {
        if (SceneManager.IsGameplayScene && !DataManager.Instance.PlayerData.GameplayNotifications)
        {
            _onOkPressed?.Invoke();
            return;
        }
        OkDialog _okDialog = Instantiate(okDialogBigPrefab, transform);
        _okDialog.Show(_message,_onOkPressed);
    }

    public void ShowYesNoDialog(string _question, UnityAction _onYesPressed = null, UnityAction _onNoPressed = null)
    {
        YesNoDialog _yesNoDialog = Instantiate(yesNoDialogPrefab, transform);
        _yesNoDialog.Show(_question,_onYesPressed,_onNoPressed);
    }
}
