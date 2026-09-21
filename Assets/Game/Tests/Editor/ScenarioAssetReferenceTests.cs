using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json.Linq;
using NUnit.Framework;
using UnityEditor.AddressableAssets;
using UnityEditor.AddressableAssets.Settings;
using UnityEngine;

public class ScenarioAssetReferenceTests
{
    private const string ScenarioDirectory = "Assets/Game/Data/Scenario";

    [Test]
    public void ScenarioAssets_HaveValidAddressablesSettings()
    {
        var settings = AddressableAssetSettingsDefaultObject.Settings;

        Assert.That(
            settings,
            Is.Not.Null,
            "Addressables settings could not be loaded.");

        var scenarioFiles = Directory.GetFiles(
            ScenarioDirectory,
            "scene*.json",
            SearchOption.TopDirectoryOnly);

        Assert.That(
            scenarioFiles,
            Is.Not.Empty,
            $"No scenario JSON files found in {ScenarioDirectory}.");

        var addressEntries = BuildAddressEntryMap(settings);

        foreach (var scenarioPath in scenarioFiles)
        {
            ValidateScenario(scenarioPath, addressEntries);
        }
    }

    private static void ValidateScenario(
        string scenarioPath,
        IReadOnlyDictionary<string, AddressableAssetEntry> addressEntries)
    {
        var json = File.ReadAllText(scenarioPath);
        var root = JObject.Parse(json);

        Assert.That(
            root["Commands"],
            Is.TypeOf<JArray>(),
            $"Commands array was not found in {scenarioPath}.");

        var commands = (JArray)root["Commands"];
        var scenarioName = Path.GetFileName(scenarioPath);

        foreach (var token in commands)
        {
            if (token is not JObject command)
            {
                continue;
            }

            var type = command.Value<string>("Type");

            switch (type)
            {
                case "dialogue":
                    ValidateVoice(command, scenarioName, addressEntries);
                    break;

                case "background":
                    ValidateAssetIdCommand(
                        command,
                        scenarioName,
                        "background",
                        addressEntries);
                    break;

                case "character":
                    ValidateCharacter(
                        command,
                        scenarioName,
                        addressEntries);
                    break;

                case "bgm":
                    ValidateAssetIdCommand(
                        command,
                        scenarioName,
                        "bgm",
                        addressEntries);
                    break;
            }
        }
    }

    private static void ValidateVoice(
        JObject command,
        string scenarioName,
        IReadOnlyDictionary<string, AddressableAssetEntry> addressEntries)
    {
        var dialogueId = command.Value<string>("Id");

        // A dialogue without an ID is treated as unvoiced.
        if (string.IsNullOrWhiteSpace(dialogueId))
        {
            return;
        }

        var expectedAddress = $"voice/episode01/{dialogueId}";

        ValidateAddressableEntry(
            scenarioName,
            $"dialogue '{dialogueId}'",
            expectedAddress,
            dialogueId,
            addressEntries);
    }

    private static void ValidateCharacter(
        JObject command,
        string scenarioName,
        IReadOnlyDictionary<string, AddressableAssetEntry> addressEntries)
    {
        var action = command.Value<string>("Action");

        if (action == "hide")
        {
            return;
        }

        ValidateAssetIdCommand(
            command,
            scenarioName,
            "character",
            addressEntries);
    }

    private static void ValidateAssetIdCommand(
        JObject command,
        string scenarioName,
        string addressPrefix,
        IReadOnlyDictionary<string, AddressableAssetEntry> addressEntries)
    {
        var assetId = command.Value<string>("AssetId");

        Assert.That(
            assetId,
            Is.Not.Null.And.Not.Empty,
            $"{scenarioName}: {addressPrefix} command has no AssetId.");

        var expectedAddress = $"{addressPrefix}/{assetId}";
        var expectedFileName = GetLastAddressSegment(expectedAddress);

        ValidateAddressableEntry(
            scenarioName,
            $"{addressPrefix} '{assetId}'",
            expectedAddress,
            expectedFileName,
            addressEntries);
    }

    private static void ValidateAddressableEntry(
        string scenarioName,
        string referenceDescription,
        string expectedAddress,
        string expectedFileName,
        IReadOnlyDictionary<string, AddressableAssetEntry> addressEntries)
    {
        Assert.That(
            addressEntries.TryGetValue(expectedAddress, out var entry),
            Is.True,
            $"{scenarioName}: {referenceDescription} is not registered with the expected " +
            $"Addressables address '{expectedAddress}'.");

        var assetPath = entry.AssetPath;

        Assert.That(
            assetPath,
            Is.Not.Null.And.Not.Empty,
            $"{scenarioName}: Addressables entry '{expectedAddress}' has no asset path.");

        Assert.That(
            File.Exists(assetPath),
            Is.True,
            $"{scenarioName}: Asset file does not exist for '{expectedAddress}'. " +
            $"Asset path: '{assetPath}'.");

        var actualFileName = Path.GetFileNameWithoutExtension(assetPath);

        Assert.That(
            actualFileName,
            Is.EqualTo(expectedFileName),
            $"{scenarioName}: Asset filename does not match its Addressables address. " +
            $"Expected filename: '{expectedFileName}', " +
            $"Actual filename: '{actualFileName}', " +
            $"Address: '{expectedAddress}', " +
            $"Asset path: '{assetPath}'.");

        Debug.Log(
            $"{scenarioName}: validated '{expectedAddress}' -> '{assetPath}'.");
    }

    private static Dictionary<string, AddressableAssetEntry> BuildAddressEntryMap(
        AddressableAssetSettings settings)
    {
        var entries = new Dictionary<string, AddressableAssetEntry>(
            StringComparer.Ordinal);

        foreach (var group in settings.groups)
        {
            if (group == null)
            {
                continue;
            }

            foreach (var entry in group.entries)
            {
                if (entry == null || string.IsNullOrWhiteSpace(entry.address))
                {
                    continue;
                }

                Assert.That(
                    entries.ContainsKey(entry.address),
                    Is.False,
                    $"Duplicate Addressables address found: '{entry.address}'.");

                entries.Add(entry.address, entry);
            }
        }

        return entries;
    }

    private static string GetLastAddressSegment(string address)
    {
        var separatorIndex = address.LastIndexOf('/');

        return separatorIndex >= 0
            ? address[(separatorIndex + 1)..]
            : address;
    }
}
