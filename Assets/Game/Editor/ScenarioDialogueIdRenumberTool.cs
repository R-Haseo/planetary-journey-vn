using System.IO;
using System.Text.RegularExpressions;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEditor;
using UnityEngine;

public static class ScenarioDialogueIdRenumberTool
{
    private const string ScenarioDirectory = "Assets/Game/Data/Scenario";

    [MenuItem("Tools/Scenario/Renumber Dialogue IDs")]
    private static void RenumberDialogueIds()
    {
        var path = EditorUtility.OpenFilePanel(
            "Select Scenario JSON",
            Path.GetFullPath(ScenarioDirectory),
            "json");

        if (string.IsNullOrEmpty(path))
        {
            return;
        }

        if (!TryGetSceneNumber(path, out var sceneNumber))
        {
            EditorUtility.DisplayDialog(
                "Renumber Dialogue IDs",
                "Could not determine the scene number from the file name.\n" +
                "Expected format: scene05.json",
                "OK");

            return;
        }

        var json = File.ReadAllText(path);
        var root = JObject.Parse(json);

        if (root["Commands"] is not JArray commands)
        {
            EditorUtility.DisplayDialog(
                "Renumber Dialogue IDs",
                "Commands array was not found.",
                "OK");

            return;
        }

        var dialogueIndex = 1;

        foreach (var token in commands)
        {
            if (token is not JObject command)
            {
                continue;
            }

            var type = command.Value<string>("Type");

            if (type != "dialogue")
            {
                continue;
            }

            command["Id"] = $"s{sceneNumber:D2}_{dialogueIndex:D3}";
            dialogueIndex++;
        }

        var output = root.ToString(Formatting.Indented);
        File.WriteAllText(path, output);

        AssetDatabase.Refresh();

        EditorUtility.DisplayDialog(
            "Renumber Dialogue IDs",
            $"Renumbered {dialogueIndex - 1} dialogue IDs.\n" +
            $"s{sceneNumber:D2}_001 - s{sceneNumber:D2}_{dialogueIndex - 1:D3}",
            "OK");

        Debug.Log(
            $"Renumbered {dialogueIndex - 1} dialogue IDs in {Path.GetFileName(path)}.");
    }

    private static bool TryGetSceneNumber(string path, out int sceneNumber)
    {
        sceneNumber = 0;

        var fileName = Path.GetFileNameWithoutExtension(path);
        var match = Regex.Match(fileName, @"^scene(\d+)$");

        return match.Success &&
               int.TryParse(match.Groups[1].Value, out sceneNumber);
    }
}
