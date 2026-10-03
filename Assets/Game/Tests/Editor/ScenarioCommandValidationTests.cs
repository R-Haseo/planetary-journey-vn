using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json.Linq;
using NUnit.Framework;
using UnityEngine;

public class ScenarioCommandValidationTests
{
    private const string ScenarioDirectory = "Assets/Game/Data/Scenario";

    private static readonly HashSet<string> CommonProperties = new()
    {
        "Type"
    };

    [Test]
    public void ScenarioCommands_ShouldFollowCommandRules()
    {
        var scenarioFiles = Directory
            .GetFiles(ScenarioDirectory, "scene*.json")
            .OrderBy(path => path)
            .ToArray();

        Assert.That(
            scenarioFiles,
            Is.Not.Empty,
            $"No scenario files found in {ScenarioDirectory}.");

        foreach (var scenarioFile in scenarioFiles)
        {
            ValidateScenarioFile(scenarioFile);
        }
    }

    private static void ValidateScenarioFile(string scenarioFile)
    {
        var json = File.ReadAllText(scenarioFile);
        var root = JObject.Parse(json);

        var commands = root["Commands"] as JArray;

        Assert.That(
            commands,
            Is.Not.Null,
            $"{scenarioFile}: Commands array is missing.");

        for (var i = 0; i < commands!.Count; i++)
        {
            Assert.That(
                commands[i],
                Is.TypeOf<JObject>(),
                $"{scenarioFile}: Commands[{i}] must be an object.");

            ValidateCommand(
                (JObject)commands[i],
                scenarioFile,
                i);
        }
    }

    private static void ValidateCommand(
        JObject command,
        string scenarioFile,
        int commandIndex)
    {
        RequireNonEmptyString(
            command,
            "Type",
            scenarioFile,
            commandIndex);

        var type = command.Value<string>("Type");

        switch (type)
        {
            case "dialogue":
                ValidateDialogue(command, scenarioFile, commandIndex);
                break;

            case "description":
                ValidateDescription(command, scenarioFile, commandIndex);
                break;

            case "background":
                ValidateBackground(command, scenarioFile, commandIndex);
                break;

            case "character":
                ValidateCharacter(command, scenarioFile, commandIndex);
                break;

            case "bgm":
                ValidateBgm(command, scenarioFile, commandIndex);
                break;

            case "fade":
                ValidateFade(command, scenarioFile, commandIndex);
                break;

            case "end":
                ValidateEnd(command, scenarioFile, commandIndex);
                break;

            default:
                Assert.Fail(
                    $"{CreatePrefix(scenarioFile, commandIndex)} " +
                    $"Unknown command Type '{type}'.");
                break;
        }
    }

    private static void ValidateDialogue(
        JObject command,
        string scenarioFile,
        int commandIndex)
    {
        ValidateAllowedProperties(
            command,
            scenarioFile,
            commandIndex,
            "Id",
            "Speaker",
            "Text");

        RequireNonEmptyString(
            command,
            "Id",
            scenarioFile,
            commandIndex);

        RequireNonEmptyString(
            command,
            "Speaker",
            scenarioFile,
            commandIndex);

        RequireString(
            command,
            "Text",
            scenarioFile,
            commandIndex);
    }

    private static void ValidateDescription(
        JObject command,
        string scenarioFile,
        int commandIndex)
    {
        ValidateAllowedProperties(
            command,
            scenarioFile,
            commandIndex,
            "Text");

        RequireString(
            command,
            "Text",
            scenarioFile,
            commandIndex);
    }

    private static void ValidateBackground(
        JObject command,
        string scenarioFile,
        int commandIndex)
    {
        ValidateAllowedProperties(
            command,
            scenarioFile,
            commandIndex,
            "AssetId");

        RequireNonEmptyString(
            command,
            "AssetId",
            scenarioFile,
            commandIndex);
    }

    private static void ValidateCharacter(
        JObject command,
        string scenarioFile,
        int commandIndex)
    {
        RequireNonEmptyString(
            command,
            "Action",
            scenarioFile,
            commandIndex);

        RequireNonEmptyString(
            command,
            "Position",
            scenarioFile,
            commandIndex);

        var action = command.Value<string>("Action");

        ValidateAllowedValue(
            action,
            new[] { "show", "hide" },
            "Action",
            scenarioFile,
            commandIndex);

        var position = command.Value<string>("Position");

        ValidateAllowedValue(
            position,
            new[] { "left", "center", "right" },
            "Position",
            scenarioFile,
            commandIndex);

        switch (action)
        {
            case "show":
                ValidateAllowedProperties(
                    command,
                    scenarioFile,
                    commandIndex,
                    "AssetId",
                    "Action",
                    "Position",
                    "Width",
                    "Height",
                    "OffsetX",
                    "OffsetY");

                RequireNonEmptyString(
                    command,
                    "AssetId",
                    scenarioFile,
                    commandIndex);

                RequirePositiveNumber(
                    command,
                    "Width",
                    scenarioFile,
                    commandIndex);

                RequirePositiveNumber(
                    command,
                    "Height",
                    scenarioFile,
                    commandIndex);

                ValidateOptionalNumber(
                    command,
                    "OffsetX",
                    scenarioFile,
                    commandIndex);

                ValidateOptionalNumber(
                    command,
                    "OffsetY",
                    scenarioFile,
                    commandIndex);
                break;

            case "hide":
                ValidateAllowedProperties(
                    command,
                    scenarioFile,
                    commandIndex,
                    "Action",
                    "Position");
                break;
        }
    }

    private static void ValidateBgm(
        JObject command,
        string scenarioFile,
        int commandIndex)
    {
        RequireNonEmptyString(
            command,
            "Action",
            scenarioFile,
            commandIndex);

        var action = command.Value<string>("Action");

        ValidateAllowedValue(
            action,
            new[] { "play", "stop" },
            "Action",
            scenarioFile,
            commandIndex);

        switch (action)
        {
            case "play":
                ValidateAllowedProperties(
                    command,
                    scenarioFile,
                    commandIndex,
                    "Action",
                    "AssetId");

                RequireNonEmptyString(
                    command,
                    "AssetId",
                    scenarioFile,
                    commandIndex);
                break;

            case "stop":
                ValidateAllowedProperties(
                    command,
                    scenarioFile,
                    commandIndex,
                    "Action");
                break;
        }
    }

    private static void ValidateFade(
        JObject command,
        string scenarioFile,
        int commandIndex)
    {
        ValidateAllowedProperties(
            command,
            scenarioFile,
            commandIndex,
            "Action",
            "Duration");

        RequireNonEmptyString(
            command,
            "Action",
            scenarioFile,
            commandIndex);

        var action = command.Value<string>("Action");

        ValidateAllowedValue(
            action,
            new[] { "in", "out" },
            "Action",
            scenarioFile,
            commandIndex);

        RequirePositiveNumber(
            command,
            "Duration",
            scenarioFile,
            commandIndex);
    }

    private static void ValidateEnd(
        JObject command,
        string scenarioFile,
        int commandIndex)
    {
        ValidateAllowedProperties(
            command,
            scenarioFile,
            commandIndex);
    }

    private static void ValidateAllowedProperties(
        JObject command,
        string scenarioFile,
        int commandIndex,
        params string[] additionalAllowedProperties)
    {
        var allowedProperties = new HashSet<string>(CommonProperties);

        foreach (var property in additionalAllowedProperties)
        {
            allowedProperties.Add(property);
        }

        var unexpectedProperties = command
            .Properties()
            .Select(property => property.Name)
            .Where(name => !allowedProperties.Contains(name))
            .ToArray();

        Assert.That(
            unexpectedProperties,
            Is.Empty,
            $"{CreatePrefix(scenarioFile, commandIndex)} " +
            $"Unexpected properties: " +
            $"{string.Join(", ", unexpectedProperties)}.");
    }

    private static void RequireNonEmptyString(
        JObject command,
        string propertyName,
        string scenarioFile,
        int commandIndex)
    {
        RequireString(
            command,
            propertyName,
            scenarioFile,
            commandIndex);

        var value = command.Value<string>(propertyName);

        Assert.That(
            string.IsNullOrWhiteSpace(value),
            Is.False,
            $"{CreatePrefix(scenarioFile, commandIndex)} " +
            $"'{propertyName}' must not be empty.");
    }

    private static void RequireString(
        JObject command,
        string propertyName,
        string scenarioFile,
        int commandIndex)
    {
        var property = command.Property(propertyName);

        Assert.That(
            property,
            Is.Not.Null,
            $"{CreatePrefix(scenarioFile, commandIndex)} " +
            $"Required property '{propertyName}' is missing.");

        Assert.That(
            property!.Value.Type,
            Is.EqualTo(JTokenType.String),
            $"{CreatePrefix(scenarioFile, commandIndex)} " +
            $"'{propertyName}' must be a string.");
    }

    private static void RequirePositiveNumber(
        JObject command,
        string propertyName,
        string scenarioFile,
        int commandIndex)
    {
        var property = command.Property(propertyName);

        Assert.That(
            property,
            Is.Not.Null,
            $"{CreatePrefix(scenarioFile, commandIndex)} " +
            $"Required property '{propertyName}' is missing.");

        Assert.That(
            IsNumber(property!.Value),
            Is.True,
            $"{CreatePrefix(scenarioFile, commandIndex)} " +
            $"'{propertyName}' must be a number.");

        var value = property.Value.Value<double>();

        Assert.That(
            value,
            Is.GreaterThan(0),
            $"{CreatePrefix(scenarioFile, commandIndex)} " +
            $"'{propertyName}' must be greater than 0.");
    }

    private static void ValidateOptionalNumber(
        JObject command,
        string propertyName,
        string scenarioFile,
        int commandIndex)
    {
        var property = command.Property(propertyName);

        if (property == null)
        {
            return;
        }

        Assert.That(
            IsNumber(property.Value),
            Is.True,
            $"{CreatePrefix(scenarioFile, commandIndex)} " +
            $"'{propertyName}' must be a number.");
    }

    private static bool IsNumber(JToken token)
    {
        return token.Type == JTokenType.Integer ||
               token.Type == JTokenType.Float;
    }

    private static void ValidateAllowedValue(
        string value,
        IEnumerable<string> allowedValues,
        string propertyName,
        string scenarioFile,
        int commandIndex)
    {
        var values = allowedValues.ToArray();

        Assert.That(
            values,
            Does.Contain(value),
            $"{CreatePrefix(scenarioFile, commandIndex)} " +
            $"Invalid '{propertyName}' value '{value}'. " +
            $"Allowed values: {string.Join(", ", values)}.");
    }

    private static string CreatePrefix(
        string scenarioFile,
        int commandIndex)
    {
        return $"{Path.GetFileName(scenarioFile)} Commands[{commandIndex}]";
    }
}
