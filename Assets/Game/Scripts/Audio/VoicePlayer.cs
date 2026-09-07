using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

public class VoicePlayer : MonoBehaviour
{
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private string episodeId = "episode01";

    private AsyncOperationHandle<AudioClip>? currentHandle;
    private int requestVersion;

    public void Play(string id)
    {
        Stop();

        var address = $"voice/{episodeId}/{id}";
        var version = ++requestVersion;

        var handle = Addressables.LoadAssetAsync<AudioClip>(address);

        currentHandle = handle;

        handle.Completed += completedHandle =>
        {
            if (version != requestVersion)
            {
                return;
            }

            if (completedHandle.Status != AsyncOperationStatus.Succeeded)
            {
                Debug.LogWarning($"Failed to load voice: {address}");
                ReleaseCurrentHandle();
                return;
            }

            audioSource.clip = completedHandle.Result;
            audioSource.Play();
        };
    }

    public void Stop()
    {
        requestVersion++;

        audioSource.Stop();
        audioSource.clip = null;

        ReleaseCurrentHandle();
    }

    private void ReleaseCurrentHandle()
    {
        if (!currentHandle.HasValue)
        {
            return;
        }

        var handle = currentHandle.Value;

        if (handle.IsValid())
        {
            Addressables.Release(handle);
        }

        currentHandle = null;
    }

    private void OnDestroy()
    {
        Stop();
    }
}
