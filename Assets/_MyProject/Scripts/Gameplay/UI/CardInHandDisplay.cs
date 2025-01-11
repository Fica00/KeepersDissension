using System;
using UnityEngine;
using UnityEngine.UI;

public class CardInHandDisplay : MonoBehaviour
{
    public static Action<CardBase> OnClicked;
    [SerializeField] private Image image;
    [SerializeField] private Button button;

    public string UniqueId { get; private set; }

    public void Setup(string _uniqueId)
    {
        UniqueId = _uniqueId;
    }
}
