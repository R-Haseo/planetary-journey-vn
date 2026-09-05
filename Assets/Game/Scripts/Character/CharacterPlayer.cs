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

    public void Show(string assetId, CharacterPosition position)
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

                if (currentHandles.TryGetValue(position, out var currentHandle) &&
                    currentHandle.Equals(completedHandle))
                {
                    currentHandles.Remove(position);
                }

                return;
            }

            // ロード中に同じ位置が別キャラクターに変更された場合、
            // 古いロード結果は表示しない。
            if (!currentHandles.TryGetValue(position, out var registeredHandle) ||
                !registeredHandle.Equals(completedHandle))
            {
                return;
            }

            characterView.Show(completedHandle.Result, position);
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
