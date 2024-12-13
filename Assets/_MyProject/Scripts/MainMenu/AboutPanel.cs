using UnityEngine;
using UnityEngine.UI;

public class AboutPanel : MonoBehaviour
{
    [SerializeField] private Button showWebsite;
    [SerializeField] private Button copyEmail;
    [SerializeField] private Button openIntro;

    public void Setup()
    {
        gameObject.SetActive(true);
    }

    private void OnEnable()
    {
        showWebsite.onClick.AddListener(ShowWebsite);
        copyEmail.onClick.AddListener(CopyEmail);
        openIntro.onClick.AddListener(OpenIntro);
        AudioManager.Instance.StopBackgroundMusic();
    }

    private void OnDisable()
    {
        showWebsite.onClick.RemoveListener(ShowWebsite);
        copyEmail.onClick.RemoveListener(CopyEmail);
        openIntro.onClick.RemoveListener(OpenIntro);
        AudioManager.Instance.PlayMainMenuMusic();
    }

    private void ShowWebsite()
    {
        Application.OpenURL("http://maverickmasterminds.net/");
    }

    private void CopyEmail()
    {
        GUIUtility.systemCopyBuffer = "info@maverickmasterminds.net";
    }
    
    private void OpenIntro()
    {
        Application.OpenURL("https://www.youtube.com/watch?v=43Ka4rxohEY");
    }
}
