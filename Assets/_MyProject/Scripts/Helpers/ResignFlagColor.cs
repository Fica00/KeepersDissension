using UnityEngine;
using UnityEngine.UI;

public class ResignFlagColor : MonoBehaviour
{
    [SerializeField] private Button reference;
    private Image image;

    private void Awake()
    {
        image = GetComponent<Image>();
    }

    private void Update()
    {
        Color _color = image.color;
        _color.a = reference.interactable ? 1 : 0.3f;
        image.color = _color;
    }
}
