using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class EmailVerificationUI : MonoBehaviour, IPanel
{
    private const string RESEND = "Resend";
    [SerializeField] private GameObject holder;
    [SerializeField] private Button resend;
    [SerializeField] private Button recheck;
    [SerializeField] private TextMeshProUGUI countdown;
    [SerializeField] private Button signOut; 

    private float cooldown;

    private void Awake()
    {
        countdown.text = RESEND;
    }

    public void Setup()
    {
        ManageInteractables(true);
        holder.SetActive(true);
    }

    public void Close()
    {
        holder.SetActive(false);
    }

    private void OnEnable()
    {
        recheck.onClick.AddListener(Recheck);
        resend.onClick.AddListener(Resend);
        signOut.onClick.AddListener(SignOut);
    }

    private void OnDisable()
    {
        recheck.onClick.RemoveListener(Recheck);
        resend.onClick.RemoveListener(Resend);
        signOut.onClick.RemoveListener(SignOut);
    }

    private void Recheck()
    {
        AuthenticationHandler.Instance.RecheckAccountVerification();
    }
    
    private void Resend()
    {
        if (cooldown>0)
        {
            return;
        }
        
        FirebaseManager.Instance.SendEmailVerification();
        StartCoroutine(CooldownRoutine());
    }

    private IEnumerator CooldownRoutine()
    {
        cooldown = 300;
        while (cooldown>0)
        {
            cooldown -= Time.deltaTime;
            countdown.text = RESEND+" (" + (int)cooldown+")";
            yield return null;
        }

        countdown.text = RESEND;
    }

    private void SignOut()
    {
        ManageInteractables(false);
        FirebaseManager.Instance.SignOut(SceneManager.LoadDataCollector);
    }

    private void ManageInteractables(bool _status)
    {
        resend.interactable = _status;
        recheck.interactable = _status;
        signOut.interactable = _status;
    }
}
