using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.AddressableAssets;
using UnityEditor.AddressableAssets.Settings;
using UnityEditor.AddressableAssets.Settings.GroupSchemas;
using UnityEngine;

public class VoiceImportTool : EditorWindow
{
    private DefaultAsset targetFolder;
    private string sceneId = "s02";
    private int startNumber = 1;

    [MenuItem("Tools/VN/Voice Import Tool")]
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

        sceneId = EditorGUILayout.TextField("Scene ID", sceneId);
        startNumber = EditorGUILayout.IntField("Start Number", startNumber);

        EditorGUILayout.Space();

        using (new EditorGUI.DisabledScope(targetFolder == null))
        {
            if (GUILayout.Button("Rename & Setup Voice Assets"))
            {
                ProcessVoiceAssets();
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
            SetupAddressable(addressableSettings, group, newAssetPath, fileId);
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
        string fileId)
    {
        var guid = AssetDatabase.AssetPathToGUID(assetPath);

        var entry = settings.CreateOrMoveEntry(
            guid,
            group,
            readOnly: false,
            postEvent: false);

        entry.address = $"voice/{fileId}";
    }
}
