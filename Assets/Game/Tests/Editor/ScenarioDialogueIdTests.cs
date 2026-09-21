using System.IO;
using System.Text.RegularExpressions;
using Newtonsoft.Json.Linq;
using NUnit.Framework;
using UnityEngine;

public class ScenarioDialogueIdTests
{
    private const string ScenarioDirectory = "Assets/Game/Data/Scenario";

    [Test]
    public void DialogueIds_AreSequential()
    {
        var scenarioFiles = Directory.GetFiles(
            ScenarioDirectory,
            "scene*.json",
            SearchOption.TopDirectoryOnly);

        Assert.That(
            scenarioFiles,
            Is.Not.Empty,
            $"No scenario JSON files found in {ScenarioDirectory}.");

        foreach (var path in scenarioFiles)
        {
            ValidateDialogueIds(path);
        }
    }

    private static void ValidateDialogueIds(string path)
    {
        var fileName = Path.GetFileNameWithoutExtension(path);
        var match = Regex.Match(fileName, @"^scene(\d+)$");

        Assert.That(
            match.Success,
            Is.True,
            $"Invalid scenario file name: {fileName}");

        var sceneNumber = int.Parse(match.Groups[1].Value);

        var json = File.ReadAllText(path);
        var root = JObject.Parse(json);

        Assert.That(
            root["Commands"],
            Is.TypeOf<JArray>(),
            $"Commands array was not found in {path}.");

        var commands = (JArray)root["Commands"];

        var dialogueIndex = 1;

        foreach (var token in commands)
        {
            if (token is not JObject command)
            {
                continue;
            }

            if (command.Value<string>("Type") != "dialogue")
            {
                continue;
            }

            var expectedId = $"s{sceneNumber:D2}_{dialogueIndex:D3}";
            var actualId = command.Value<string>("Id");

            Assert.That(
                actualId,
                Is.EqualTo(expectedId),
                $"{fileName}: dialogue #{dialogueIndex} has an invalid ID. " +
                $"Expected '{expectedId}', but found '{actualId}'.");

            dialogueIndex++;
        }

        Debug.Log(
            $"{fileName}: validated {dialogueIndex - 1} sequential dialogue IDs.");
    }
}
