using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

public class CharacterPlayer : MonoBehaviour
{
    [SerializeField]
    private CharacterView characterView;

    private readonly Dictionary<CharacterPosition, AsyncOperationHandle<Sprite>>
        currentHandles = new();

    public void Show(string assetId, CharacterPosition position, float width, float height, float offsetX, float offsetY)
    {
        Release(position);

        var address = $"character/{assetId}";
        var handle = Addressables.LoadAssetAsync<Sprite>(address);

        currentHandles[position] = handle;

        handle.Completed += completedHandle =>
        {
            if (completedHandle.Status != AsyncOperationStatus.Succeeded)
            {
                Debug.LogError($"Failed to load character: {address}");
                return;
            }

            if (!currentHandles.TryGetValue(position, out var registeredHandle) ||
                !registeredHandle.Equals(completedHandle))
            {
                return;
            }

            characterView.Show(
                completedHandle.Result,
                position,
                width,
                height,
                offsetX,
                offsetY);
        };
    }

    public void Hide(CharacterPosition position)
    {
        characterView.Hide(position);
        Release(position);
    }

    private void Release(CharacterPosition position)
    {
        if (!currentHandles.TryGetValue(position, out var handle))
        {
            return;
        }

        if (handle.IsValid())
        {
            Addressables.Release(handle);
        }

        currentHandles.Remove(position);
    }

    private void OnDestroy()
    {
        foreach (var handle in currentHandles.Values)
        {
            if (handle.IsValid())
            {
                Addressables.Release(handle);
            }
        }

        currentHandles.Clear();
    }
}
