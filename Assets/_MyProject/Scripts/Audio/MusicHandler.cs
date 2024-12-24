using UnityEngine;
using UnityEngine.UI;

public class MusicHandler : MonoBehaviour
{
    [SerializeField] private Sprite on;
    [SerializeField] private Sprite off;
    [SerializeField] private Image image;
    [SerializeField] private Button button;

    private void OnEnable()
    {
        button.onClick.AddListener(Toggle);
        Show();
    }

    private void OnDisable()
    {
        button.onClick.RemoveListener(Toggle);
    }

    private void Toggle()
    {
        DataManager.Instance.PlayerData.PlayMusic = !DataManager.Instance.PlayerData.PlayMusic;
        Show();
    }

    private void Show()
    {
        image.sprite = DataManager.Instance.PlayerData.PlayMusic ? on : off;
    }
}
