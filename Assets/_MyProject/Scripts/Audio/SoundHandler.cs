using UnityEngine;
using UnityEngine.UI;

public class SoundHandler : MonoBehaviour
{
    [SerializeField] private Sprite on;
    [SerializeField] private Sprite off;

    [SerializeField] private Button button;
    [SerializeField] private Image image;

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
        DataManager.Instance.PlayerData.PlaySoundEffect = !DataManager.Instance.PlayerData.PlaySoundEffect;
        Show();
    }

    private void Show()
    {
        image.sprite = DataManager.Instance.PlayerData.PlaySoundEffect ? on : off;
    }
}
