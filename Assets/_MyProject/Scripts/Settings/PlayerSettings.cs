using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayerSettings : MonoBehaviour
{
    [SerializeField] private TMP_InputField nameInput;
    [SerializeField] private Button save;

    private void OnEnable()
    {
        save.onClick.AddListener(Save);        
    }

    private void OnDisable()
    {
        save.onClick.RemoveListener(Save);
    }

    public void Setup()
    {
        gameObject.SetActive(true);
        nameInput.text = DataManager.Instance.PlayerData.Name;
    }

    private void Save()
    {
        if (CredentialsValidator.ValidateName(nameInput.text))
        {
            DataManager.Instance.PlayerData.Name = nameInput.text;
        }

        gameObject.SetActive(false);
    }
}