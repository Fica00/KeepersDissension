using UnityEngine;
using TMPro;

public class EconomyDisplay : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI whiteStrangeMatterDisplay;
    private GameplayPlayer player;

    public void Setup(GameplayPlayer _player)
    {
        player = _player;
        player.UpdatedStrangeMatter += ShowWhiteStrangeMatter;
        ShowWhiteStrangeMatter();
    }

    private void OnDisable()
    {
        player.UpdatedStrangeMatter -= ShowWhiteStrangeMatter;
    }

    private void ShowWhiteStrangeMatter()
    {
        whiteStrangeMatterDisplay.text = player.StrangeMatter.ToString();
    }
}
