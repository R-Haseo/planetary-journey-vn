using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

public class BGMPlayer : MonoBehaviour
{
    [SerializeField]
    private AudioSource audioSource;

    private string currentAssetId;
    private AsyncOperationHandle<AudioClip>? currentHandle;
    private int requestVersion;

    public void Play(string assetId)
    {
        if (currentAssetId == assetId && audioSource.isPlaying)
        {
            return;
        }

        StopCurrent();

        currentAssetId = assetId;

        var address = $"bgm/{assetId}";
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
                Debug.LogError($"Failed to load BGM: {address}");
                ReleaseCurrentHandle();
                return;
            }

            audioSource.clip = completedHandle.Result;
            audioSource.loop = true;
            audioSource.Play();
        };
    }

    public void Stop()
    {
        requestVersion++;
        currentAssetId = null;
        StopCurrent();
    }

    private void StopCurrent()
    {
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
        requestVersion++;
        StopCurrent();
    }
}
