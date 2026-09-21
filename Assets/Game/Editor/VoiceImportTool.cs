using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.AddressableAssets;
using UnityEditor.AddressableAssets.Settings;
using UnityEngine;

public class VoiceImportTool : EditorWindow
{
    private DefaultAsset targetFolder;
    private string episodeId = "episode01";
    private string sceneId = "s02";
    private int startNumber = 1;

    [MenuItem("Tools/Voice/Voice Import Tool")]
    public static void ShowWindow()
    {
        GetWindow<VoiceImportTool>("Voice Import Tool");
    }

    private void OnGUI()
    {
        EditorGUILayout.LabelField("Voice Import Tool", EditorStyles.boldLabel);

        targetFolder = (DefaultAsset)EditorGUILayout.ObjectField(
            "Target Folder",
            targetFolder,
            typeof(DefaultAsset),
            false);

        episodeId = EditorGUILayout.TextField("Episode ID", episodeId);
        sceneId = EditorGUILayout.TextField("Scene ID", sceneId);
        startNumber = EditorGUILayout.IntField("Start Number", startNumber);

        EditorGUILayout.Space();

        using (new EditorGUI.DisabledScope(targetFolder == null))
        {
            if (GUILayout.Button("Rename & Setup Voice Assets"))
            {
                ProcessVoiceAssets();
            }

            if (GUILayout.Button("Renumber Existing Voice Assets"))
            {
                RenumberExistingVoiceAssets();
            }
        }
    }

    private void ProcessVoiceAssets()
    {
        var folderPath = AssetDatabase.GetAssetPath(targetFolder);

        if (string.IsNullOrEmpty(folderPath) ||
            !AssetDatabase.IsValidFolder(folderPath))
        {
            Debug.LogError("Valid target folder is required.");
            return;
        }

        var guids = AssetDatabase.FindAssets("t:AudioClip", new[] { folderPath });

        var assets = guids
            .Select(guid => AssetDatabase.GUIDToAssetPath(guid))
            .Where(path =>
                string.Equals(
                    Path.GetExtension(path),
                    ".wav",
                    StringComparison.OrdinalIgnoreCase))
            .Select(path => new
            {
                Path = path,
                Prefix = GetNumericPrefix(Path.GetFileNameWithoutExtension(path))
            })
            .Where(x => x.Prefix.HasValue)
            .OrderBy(x => x.Prefix.Value)
            .ToList();

        if (assets.Count == 0)
        {
            Debug.LogWarning("No WAV files with a numeric prefix were found.");
            return;
        }

        if (!EditorUtility.DisplayDialog(
                "Voice Import Tool",
                $"{assets.Count} voice files will be processed.\nContinue?",
                "Run",
                "Cancel"))
        {
            return;
        }

        var addressableSettings = AddressableAssetSettingsDefaultObject.Settings;

        if (addressableSettings == null)
        {
            Debug.LogError("Addressables settings were not found.");
            return;
        }

        var group = addressableSettings.DefaultGroup;

        for (var i = 0; i < assets.Count; i++)
        {
            var asset = assets[i];
            var number = startNumber + i;

            var fileId = $"{sceneId}_{number:000}";
            var directory = Path.GetDirectoryName(asset.Path)?.Replace("\\", "/");

            if (string.IsNullOrEmpty(directory))
            {
                continue;
            }

            var newAssetPath = $"{directory}/{fileId}.wav";

            var renameError = AssetDatabase.RenameAsset(asset.Path, fileId);

            if (!string.IsNullOrEmpty(renameError))
            {
                Debug.LogError(
                    $"Rename failed: {asset.Path}\n{renameError}");
                continue;
            }

            ApplyAudioImportSettings(newAssetPath);
            SetupAddressable(addressableSettings, group, newAssetPath, episodeId, fileId);
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log($"Voice import completed: {assets.Count} files");
    }

    private static int? GetNumericPrefix(string fileName)
    {
        if (fileName.Length < 3)
        {
            return null;
        }

        var prefix = fileName.Substring(0, 3);

        return int.TryParse(prefix, out var value)
            ? value
            : null;
    }

    private static void ApplyAudioImportSettings(string assetPath)
    {
        var importer = AssetImporter.GetAtPath(assetPath) as AudioImporter;

        if (importer == null)
        {
            Debug.LogWarning($"AudioImporter not found: {assetPath}");
            return;
        }

        importer.forceToMono = true;
        importer.loadInBackground = false;

        var settings = importer.defaultSampleSettings;

        settings.loadType = AudioClipLoadType.CompressedInMemory;
        settings.compressionFormat = AudioCompressionFormat.Vorbis;
        settings.quality = 0.7f;

        importer.defaultSampleSettings = settings;
        importer.SaveAndReimport();
    }

    private static void SetupAddressable(
        AddressableAssetSettings settings,
        AddressableAssetGroup group,
        string assetPath,
        string episodeId,
        string fileId)
    {
        var guid = AssetDatabase.AssetPathToGUID(assetPath);

        var entry = settings.CreateOrMoveEntry(
            guid,
            group,
            readOnly: false,
            postEvent: false);

        entry.address = $"voice/{episodeId}/{fileId}";
    }

    private void RenumberExistingVoiceAssets()
    {
        var folderPath = AssetDatabase.GetAssetPath(targetFolder);

        if (string.IsNullOrEmpty(folderPath) ||
            !AssetDatabase.IsValidFolder(folderPath))
        {
            Debug.LogError("Valid target folder is required.");
            return;
        }

        var guids = AssetDatabase.FindAssets("t:AudioClip", new[] { folderPath });

        var assets = guids
            .Select(guid => AssetDatabase.GUIDToAssetPath(guid))
            .Where(path =>
                string.Equals(
                    Path.GetExtension(path),
                    ".wav",
                    StringComparison.OrdinalIgnoreCase))
            .Select(path => new
            {
                Path = path,
                Number = GetExistingVoiceNumber(
                    Path.GetFileNameWithoutExtension(path),
                    sceneId)
            })
            .Where(x => x.Number.HasValue)
            .OrderBy(x => x.Number.Value.Number)
            .ThenBy(x => x.Number.Value.SubNumber)
            .ToList();

        if (assets.Count == 0)
        {
            Debug.LogWarning(
                $"No existing voice assets for scene '{sceneId}' were found.");
            return;
        }

        if (!EditorUtility.DisplayDialog(
                "Renumber Existing Voice Assets",
                $"{assets.Count} existing voice assets will be renumbered.\n" +
                $"Start number: {startNumber:000}\n\nContinue?",
                "Run",
                "Cancel"))
        {
            return;
        }

        var addressableSettings = AddressableAssetSettingsDefaultObject.Settings;

        if (addressableSettings == null)
        {
            Debug.LogError("Addressables settings were not found.");
            return;
        }

        var group = addressableSettings.DefaultGroup;

        // First pass:
        // Rename everything to temporary unique names to avoid collisions.
        var temporaryAssets = assets
            .Select((asset, index) =>
            {
                var temporaryId = $"__voice_temp_{Guid.NewGuid():N}";

                var renameError = AssetDatabase.RenameAsset(
                    asset.Path,
                    temporaryId);

                if (!string.IsNullOrEmpty(renameError))
                {
                    Debug.LogError(
                        $"Temporary rename failed: {asset.Path}\n{renameError}");

                    return null;
                }

                var directory = Path.GetDirectoryName(asset.Path)?
                    .Replace("\\", "/");

                if (string.IsNullOrEmpty(directory))
                {
                    return null;
                }

                return new
                {
                    Path = $"{directory}/{temporaryId}.wav",
                    Index = index
                };
            })
            .Where(x => x != null)
            .ToList();

        if (temporaryAssets.Count != assets.Count)
        {
            Debug.LogError(
                "Temporary renaming failed. Renumbering was aborted.");
            AssetDatabase.Refresh();
            return;
        }

        // Second pass:
        // Rename temporary assets to their final sequential IDs
        // and update their Addressables addresses.
        foreach (var asset in temporaryAssets)
        {
            var number = startNumber + asset.Index;
            var fileId = $"{sceneId}_{number:000}";

            var directory = Path.GetDirectoryName(asset.Path)?
                .Replace("\\", "/");

            if (string.IsNullOrEmpty(directory))
            {
                continue;
            }

            var newAssetPath = $"{directory}/{fileId}.wav";

            var renameError = AssetDatabase.RenameAsset(
                asset.Path,
                fileId);

            if (!string.IsNullOrEmpty(renameError))
            {
                Debug.LogError(
                    $"Final rename failed: {asset.Path}\n{renameError}");
                continue;
            }

            SetupAddressable(
                addressableSettings,
                group,
                newAssetPath,
                episodeId,
                fileId);
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log(
            $"Voice renumber completed: {assets.Count} files " +
            $"({sceneId}_{startNumber:000} - " +
            $"{sceneId}_{startNumber + assets.Count - 1:000})");
    }

    private static (int Number, int SubNumber)? GetExistingVoiceNumber(
        string fileName,
        string sceneId)
    {
        var prefix = $"{sceneId}_";

        if (!fileName.StartsWith(
                prefix,
                StringComparison.OrdinalIgnoreCase))
        {
            return null;
        }

        var numberPart = fileName.Substring(prefix.Length);
        var parts = numberPart.Split('_');

        if (!int.TryParse(parts[0], out var number))
        {
            return null;
        }

        var subNumber = 0;

        if (parts.Length >= 2 &&
            !int.TryParse(parts[1], out subNumber))
        {
            return null;
        }

        return (number, subNumber);
    }
}
