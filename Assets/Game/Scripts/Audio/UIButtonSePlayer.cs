using System.Collections;
using UnityEngine;

public class UIButtonSePlayer : MonoBehaviour
{
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip buttonClickSe;

    public void Play()
    {
        if (audioSource == null || buttonClickSe == null)
            return;

        audioSource.PlayOneShot(buttonClickSe);
    }

    public IEnumerator PlayAndWait()
    {
        Play();

        if (audioSource != null && buttonClickSe != null)
        {
            yield return new WaitForSecondsRealtime(
                buttonClickSe.length / Mathf.Max(0.01f, Mathf.Abs(audioSource.pitch)));
        }
    }
}
