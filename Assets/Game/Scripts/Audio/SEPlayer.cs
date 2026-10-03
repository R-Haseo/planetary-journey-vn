using System.Collections;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

public class SEPlayer : MonoBehaviour
{
    [SerializeField] private AudioSource audioSource;

    public void Play(string assetId)
    {
        StartCoroutine(PlayCoroutine(assetId));
    }

    private IEnumerator PlayCoroutine(string assetId)
    {
        var address = $"se/{assetId}";
        var handle = Addressables.LoadAssetAsync<AudioClip>(address);

        yield return handle;

        if (handle.Status != AsyncOperationStatus.Succeeded)
        {
            Debug.LogError($"Failed to load SE: {address}");
            Addressables.Release(handle);
            yield break;
        }

        var clip = handle.Result;

        audioSource.PlayOneShot(clip);

        yield return new WaitForSecondsRealtime(clip.length);

        Addressables.Release(handle);
    }
}
