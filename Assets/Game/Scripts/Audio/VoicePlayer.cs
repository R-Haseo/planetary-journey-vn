using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

public class VoicePlayer : MonoBehaviour
{
    [SerializeField] private AudioSource audioSource;

    private AsyncOperationHandle<AudioClip>? currentHandle;

    public async void Play(string id)
    {
        Stop();

        var address = $"voice/{id}";
        var handle = Addressables.LoadAssetAsync<AudioClip>(address);

        currentHandle = handle;

        var clip = await handle.Task;

        if (handle.Status != AsyncOperationStatus.Succeeded)
        {
            Debug.LogWarning($"Failed to load voice: {address}");
            currentHandle = null;
            return;
        }

        audioSource.clip = clip;
        audioSource.Play();
    }

    public void Stop()
    {
        audioSource.Stop();
        audioSource.clip = null;

        if (currentHandle.HasValue)
        {
            Addressables.Release(currentHandle.Value);
            currentHandle = null;
        }
    }
}
