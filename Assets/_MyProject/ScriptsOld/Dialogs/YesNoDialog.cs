using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.Events;

public class YesNoDialog : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI messageDisplay;
    [SerializeField] private Button yesButton;
    [SerializeField] private Button noButton;

    private readonly UnityEvent onYesPressed = new ();
    private readonly UnityEvent onNoPressed = new ();

    public void Show(string _message, UnityAction _onYesPressed = null, UnityAction _onNoPressed = null)
    {
        if (_onYesPressed != null)
        {
            onYesPressed.AddListener(_onYesPressed);
        }

        if (_onNoPressed != null)
        {
            onNoPressed.AddListener(_onNoPressed);
        }

        messageDisplay.text = _message;
        gameObject.SetActive(true);
    }

    private void OnEnable()
    {
        yesButton.onClick.AddListener(YesPressed);
        noButton.onClick.AddListener(NoPressed);
    }

    private void OnDisable()
    {
        yesButton.onClick.RemoveListener(YesPressed);
        noButton.onClick.RemoveListener(NoPressed);
    }

    private void YesPressed()
    {
        onYesPressed?.Invoke();
        Close();
    }

    private void NoPressed()
    {
        onNoPressed?.Invoke();
        Close();
    }

    private void Close()
    {
        onYesPressed.RemoveAllListeners();
        onNoPressed.RemoveAllListeners();
        Destroy(gameObject);
    }
}