using UnityEngine;
using UnityEngine.UI;

public class Rebuilder : MonoBehaviour
{
    [SerializeField] private RectTransform rectTransform;

    private void OnEnable()
    {
        LayoutRebuilder.ForceRebuildLayoutImmediate(rectTransform);
    }
}
