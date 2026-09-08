using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

public class BackgroundPlayer : MonoBehaviour
{
    [SerializeField] private BackgroundView backgroundView;

    private AsyncOperationHandle<Sprite>? currentHandle;
    private int requestVersion;

    public void Show(string assetId)
    {
        ReleaseCurrentHandle();

        var address = $"background/{assetId}";
        var version = ++requestVersion;

        var handle = Addressables.LoadAssetAsync<Sprite>(address);
        currentHandle = handle;

        handle.Completed += completedHandle =>
        {
            if (version != requestVersion)
            {
                return;
            }

            if (completedHandle.Status != AsyncOperationStatus.Succeeded)
            {
                Debug.LogError($"Failed to load background: {address}");
                ReleaseCurrentHandle();
                return;
            }

            backgroundView.Show(completedHandle.Result);
        };
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
        ReleaseCurrentHandle();
    }
}
