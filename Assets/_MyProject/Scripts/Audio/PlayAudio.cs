using UnityEngine;

public class PlayAudio : MonoBehaviour
{
    [SerializeField] private AudioClip clip;
    [SerializeField] private bool playOnAwake;

    private void Awake()
    {
        if (playOnAwake)
        {
            AudioManager.Instance.PlaySoundEffect(clip);
        }
    }
}
