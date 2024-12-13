using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.Events;

public class OkDialog : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI messageDisplay;
    [SerializeField] private Button okButton;
    
    private readonly UnityEvent onOkPressed = new ();

    public void Show(string _message,UnityAction _onOkPressed=null)
    {
        if (_onOkPressed != null)
        {
            onOkPressed.AddListener(_onOkPressed);
        }
        messageDisplay.text = _message;
        gameObject.SetActive(true);
    }

    private void OnEnable()
    {
        okButton.onClick.AddListener(OkPressed);
    }

    private void OnDisable()
    {
        okButton.onClick.RemoveListener(OkPressed);
    }

    private void OkPressed()
    {
        onOkPressed?.Invoke();
        Close();
    }

    private void Close()
    {
        onOkPressed.RemoveAllListeners();
        Destroy(gameObject);
    }
}
