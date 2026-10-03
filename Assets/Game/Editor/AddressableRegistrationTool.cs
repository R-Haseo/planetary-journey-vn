using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.AddressableAssets;
using UnityEditor.AddressableAssets.Settings;
using UnityEngine;

public class AddressableRegistrationTool : EditorWindow
{
    private UnityEngine.Object target;

    [MenuItem("Tools/Addressables/Registration Tool")]
    private static void Open()
    {
        GetWindow<AddressableRegistrationTool>("Addressable Registration");
    }

    private void OnGUI()
    {
        EditorGUILayout.LabelField("Addressable Registration Tool", EditorStyles.boldLabel);

        EditorGUILayout.Space();

        EditorGUILayout.HelpBox(
            "Select a file or folder under a supported asset path.\n" +
            "Folders are processed recursively.",
            MessageType.Info);

        EditorGUILayout.Space();

        target = EditorGUILayout.ObjectField(
            "Target",
            target,
            typeof(UnityEngine.Object),
            false);

        EditorGUILayout.Space();

        using (new EditorGUI.DisabledScope(target == null))
        {
            if (GUILayout.Button("Register Addressables"))
            {
                RegisterTarget();
            }
        }
    }

    private void RegisterTarget()
    {
        var targetPath = AssetDatabase.GetAssetPath(target);

        if (string.IsNullOrEmpty(targetPath))
        {
            Debug.LogError("Failed to get target asset path.");
            return;
        }

        var settings = AddressableAssetSettingsDefaultObject.Settings;

        if (settings == null)
        {
            Debug.LogError("AddressableAssetSettings could not be found.");
            return;
        }

        var assetPaths = GetTargetAssetPaths(targetPath);

        var registeredCount = 0;
        var skippedCount = 0;

        foreach (var assetPath in assetPaths)
        {
            if (!TryCreateAddress(assetPath, out var address))
            {
                Debug.LogWarning($"Skipped unsupported asset: {assetPath}");

                skippedCount++;
                continue;
            }

            if (RegisterAsset(settings, assetPath, address))
            {
                registeredCount++;
            }
            else
            {
                skippedCount++;
            }
        }

        AssetDatabase.SaveAssets();

        Debug.Log(
            $"Addressable registration completed. " +
            $"Registered: {registeredCount}, " +
            $"Skipped: {skippedCount}");
    }

    private static List<string> GetTargetAssetPaths(string targetPath)
    {
        var assetPaths = new List<string>();

        if (!AssetDatabase.IsValidFolder(targetPath))
        {
            assetPaths.Add(targetPath);
            return assetPaths;
        }

        var guids = AssetDatabase.FindAssets(string.Empty, new[] { targetPath });

        foreach (var guid in guids)
        {
            var assetPath = AssetDatabase.GUIDToAssetPath(guid);

            if (AssetDatabase.IsValidFolder(assetPath))
            {
                continue;
            }

            assetPaths.Add(assetPath);
        }

        return assetPaths;
    }

    private static bool TryCreateAddress(string assetPath, out string address)
    {
        address = null;

        var normalizedPath = assetPath.Replace("\\", "/");

        if (TryCreateBackgroundAddress(normalizedPath, out address))
        {
            return true;
        }

        if (TryCreateCharacterAddress(normalizedPath, out address))
        {
            return true;
        }

        if (TryCreateVoiceAddress(normalizedPath, out address))
        {
            return true;
        }

        if (TryCreateBgmAddress(normalizedPath, out address))
        {
            return true;
        }

        if (TryCreateSeAddress(normalizedPath, out address))
        {
            return true;
        }

        return false;
    }

    private static bool TryCreateBackgroundAddress(string assetPath, out string address)
    {
        const string root = "Assets/Game/Art/Backgrounds/";

        address = null;

        if (!assetPath.StartsWith(root, StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        var relativePath = RemoveExtension(assetPath.Substring(root.Length));

        address = $"background/{relativePath.ToLowerInvariant()}";

        return true;
    }

    private static bool TryCreateCharacterAddress(string assetPath, out string address)
    {
        const string root = "Assets/Game/Art/Characters/";

        address = null;

        if (!assetPath.StartsWith(root, StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        var relativePath = RemoveExtension(assetPath.Substring(root.Length));

        address = $"character/{relativePath.ToLowerInvariant()}";

        return true;
    }

    private static bool TryCreateVoiceAddress(string assetPath, out string address)
    {
        const string root = "Assets/Game/Audio/Voice/";

        address = null;

        if (!assetPath.StartsWith(root, StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        var relativePath =  RemoveExtension(assetPath.Substring(root.Length));

        var parts = relativePath.Split('/');

        // Physical:
        // episode01/scene08/s08_001.wav
        //
        // Address:
        // voice/episode01/s08_001

        if (parts.Length != 3)
        {
            Debug.LogWarning($"Unexpected voice path structure: {assetPath}");

            return false;
        }

        var episode = parts[0].ToLowerInvariant();

        var fileName = parts[2].ToLowerInvariant();

        address = $"voice/{episode}/{fileName}";

        return true;
    }

    private static bool TryCreateBgmAddress(string assetPath, out string address)
    {
        const string root = "Assets/Game/Audio/BGM/";

        address = null;

        if (!assetPath.StartsWith(root, StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        var relativePath = RemoveExtension(assetPath.Substring(root.Length));

        address = $"bgm/{relativePath.ToLowerInvariant()}";

        return true;
    }

    private static bool TryCreateSeAddress(string assetPath, out string address)
    {
        const string root = "Assets/Game/Audio/SE/";

        address = null;

        if (!assetPath.StartsWith(root, StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        var relativePath = RemoveExtension(assetPath.Substring(root.Length));

        address = $"se/{relativePath.ToLowerInvariant()}";

        return true;
    }

    private static bool RegisterAsset(AddressableAssetSettings settings, string assetPath, string address)
    {
        var guid = AssetDatabase.AssetPathToGUID(assetPath);

        if (string.IsNullOrEmpty(guid))
        {
            Debug.LogWarning($"GUID could not be found: {assetPath}");
            return false;
        }

        var existingEntry = settings.FindAssetEntry(guid);

        if (existingEntry != null)
        {
            existingEntry.address = address;

            Debug.Log(
                $"Updated Addressable: " +
                $"{assetPath} -> {address}");

            return true;
        }

        var defaultGroup = settings.DefaultGroup;

        if (defaultGroup == null)
        {
            Debug.LogError("Default Addressables Group could not be found.");
            return false;
        }

        var entry = settings.CreateOrMoveEntry(guid, defaultGroup);

        entry.address = address;

        Debug.Log(
            $"Registered Addressable: " +
            $"{assetPath} -> {address}");

        return true;
    }

    private static string RemoveExtension(string path)
    {
        var extension = Path.GetExtension(path);

        if (string.IsNullOrEmpty(extension))
        {
            return path;
        }

        return path.Substring(0, path.Length - extension.Length);
    }
}
