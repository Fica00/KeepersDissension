using UnityEngine;
using UnityEngine.UI;

public class ElipsisActionsController : MonoBehaviour
{
    [SerializeField] private Button endTurn;

    public void Setup()
    {
        endTurn.interactable = !GameplayManager.Instance.IsSettingUpTable;
    }
}
