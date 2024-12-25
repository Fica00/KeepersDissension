using System.Collections;
using UnityEngine;
using TMPro;

public class EconomyDisplay : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI whiteStrangeMatterDisplay;
    private GameplayPlayer player;

    public void Setup(GameplayPlayer _player)
    {
        player = _player;
    }

    private void OnEnable()
    {
        StartCoroutine(ShowWhiteMatterRoutine());
    }

    private void OnDisable()
    {
        StopAllCoroutines();
    }

    private IEnumerator ShowWhiteMatterRoutine()
    {
        while (gameObject.activeSelf)
        {
            yield return new WaitForSeconds(1);
            if (player==null)
            {
                continue;
            }
            
            int _matter = player.IsMy ? GameplayManager.Instance.MyStrangeMatter() : GameplayManager.Instance.OpponentsStrangeMatter();
            whiteStrangeMatterDisplay.text = _matter.ToString();
        }
    }
}
