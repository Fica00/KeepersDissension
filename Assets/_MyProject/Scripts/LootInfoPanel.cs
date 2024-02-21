using System;
using UnityEngine;

public class LootInfoPanel : MonoBehaviour
{
    public static Action OnShowed;
    public static Action OnClosed;

    private void OnEnable()
    {
        OnShowed?.Invoke();
    }

    private void OnDisable()
    {
        OnClosed?.Invoke();
    }
}
