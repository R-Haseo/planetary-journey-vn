using System;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

public class VoicePlayer : MonoBehaviour
{
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private string episodeId = "episode01";

    private AsyncOperationHandle<AudioClip>? currentHandle;
    private int requestVersion;
    private bool isPlaying;
    public bool IsPlaying => isPlaying;

    public event Action PlaybackCompleted;

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
            isPlaying = true;
        };
    }

    private void Update()
    {
        if (!isPlaying || audioSource.isPlaying)
        {
            return;
        }

        isPlaying = false;
        PlaybackCompleted?.Invoke();
    }

    public void Stop()
    {
        requestVersion++;
        isPlaying = false;

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
