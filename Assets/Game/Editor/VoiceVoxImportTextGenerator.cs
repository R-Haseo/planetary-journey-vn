using System.Collections.Generic;
using System.IO;
using System.Text;
using Newtonsoft.Json;
using UnityEditor;
using UnityEngine;

public static class VoiceVoxImportTextGenerator
{
    private const string ScenarioDirectory = "Assets/Game/Data/Scenario";
    private const string OutputDirectory = "Assets/Game/Generated/VoiceVox";

    [MenuItem("Tools/Voice/Generate VOICEVOX Import Text")]
    private static void Generate()
    {
        string scenarioPath = EditorUtility.OpenFilePanel(
            "Select Scenario JSON",
            ScenarioDirectory,
            "json");

        if (string.IsNullOrEmpty(scenarioPath))
        {
            return;
        }

        string json = File.ReadAllText(scenarioPath);

        ScenarioDataDto scenario = JsonConvert.DeserializeObject<ScenarioDataDto>(json);

        if (scenario?.Commands == null)
        {
            Debug.LogError($"Failed to load scenario: {scenarioPath}");
            return;
        }

        string episodeId = new DirectoryInfo(Path.GetDirectoryName(scenarioPath)!).Name;

        Dictionary<string, string> speakerMap = VoiceVoxSpeakerMappingRepository.Load(episodeId);

        if (speakerMap.Count == 0)
        {
            Debug.LogError(
                $"VOICEVOX speaker mappings not found: {episodeId}");
            return;
        }

        var builder = new StringBuilder();

        foreach (ScenarioCommandDto command in scenario.Commands)
        {
            if (command.Type != "dialogue")
            {
                continue;
            }

            // IDがないdialogueは音声対象外
            if (string.IsNullOrWhiteSpace(command.Id))
            {
                continue;
            }

            if (string.IsNullOrWhiteSpace(command.Text))
            {
                continue;
            }

            string speaker = command.Speaker ?? string.Empty;

            if (!speakerMap.TryGetValue(speaker, out string voiceVoxSpeaker))
            {
                Debug.LogError(
                    $"VOICEVOX speaker mapping not found. " +
                    $"Scenario speaker: '{speaker}', Dialogue ID: '{command.Id}'");

                return;
            }

            builder.Append(voiceVoxSpeaker);
            builder.Append(',');
            builder.AppendLine(command.Text);
        }

        Directory.CreateDirectory(OutputDirectory);

        string scenarioName = Path.GetFileNameWithoutExtension(scenarioPath);
        string outputPath =
            Path.Combine(OutputDirectory, $"{scenarioName}_voicevox.txt");

        File.WriteAllText(
            outputPath,
            builder.ToString(),
            new UTF8Encoding(false));

        AssetDatabase.Refresh();

        Debug.Log(
            $"VOICEVOX import text generated: {outputPath}");
    }
}
