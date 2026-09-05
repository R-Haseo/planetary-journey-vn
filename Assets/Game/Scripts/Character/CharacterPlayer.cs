using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

public class CharacterPlayer : MonoBehaviour
{
    [SerializeField]
    private CharacterView characterView;

    private AsyncOperationHandle<Sprite>? currentHandle;

    public async void Show(string assetId, CharacterPosition position)
    {
        ReleaseCurrent();

        var address = $"character/{assetId}";
        var handle = Addressables.LoadAssetAsync<Sprite>(address);

        currentHandle = handle;

        await handle.Task;

        if (handle.Status != AsyncOperationStatus.Succeeded)
        {
            Debug.LogError($"Failed to load character: {address}");
            currentHandle = null;
            return;
        }

        characterView.SetPosition(position);
        characterView.Show(handle.Result);
    }

    public void Hide()
    {
        characterView.Hide();
        ReleaseCurrent();
    }

    private void OnDestroy()
    {
        ReleaseCurrent();
    }

    private void ReleaseCurrent()
    {
        if (!currentHandle.HasValue)
        {
            return;
        }

        Addressables.Release(currentHandle.Value);
        currentHandle = null;
    }
}
