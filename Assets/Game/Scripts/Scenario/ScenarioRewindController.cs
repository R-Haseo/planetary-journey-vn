using System.Collections.Generic;
using Newtonsoft.Json;
using UnityEngine;

#if UNITY_EDITOR || DEVELOPMENT_BUILD
public class ScenarioRewindController : MonoBehaviour
{
    [SerializeField] private CharacterPlayer characterPlayer;
    [SerializeField] private BackgroundPlayer backgroundPlayer;
    [SerializeField] private BGMPlayer bgmPlayer;

    public bool TryRewind(
        IReadOnlyList<TextAsset> scenarioJsons,
        int currentScenarioIndex,
        int currentCommandIndex,
        out RewindResult result)
    {
        result = null;

        if (!TryFindPreviousDisplayCommand(
                scenarioJsons,
                currentScenarioIndex,
                currentCommandIndex,
                out var targetScenarioIndex,
                out var targetCommandIndex))
        {
            Debug.Log("No previous display command.");
            return false;
        }

        var targetCommands = GetScenarioCommands(scenarioJsons, targetScenarioIndex);

        var currentCharacterStates = GetCharacterStates(scenarioJsons, currentScenarioIndex, currentCommandIndex);

        RestoreScenarioState(
            scenarioJsons,
            targetScenarioIndex,
            targetCommandIndex,
            currentCharacterStates);

        result = new RewindResult(
            targetScenarioIndex,
            targetCommandIndex,
            targetCommands);

        Debug.Log($"Rewind: scenario={targetScenarioIndex}, command={targetCommandIndex}");

        return true;
    }

    private bool TryFindPreviousDisplayCommand(
        IReadOnlyList<TextAsset> scenarioJsons,
        int scenarioIndex,
        int commandIndex,
        out int targetScenarioIndex,
        out int targetCommandIndex)
    {
        for (var i = scenarioIndex; i >= 0; i--)
        {
            var scenarioCommands = GetScenarioCommands(scenarioJsons, i);

            var startIndex = i == scenarioIndex
                ? commandIndex - 1
                : scenarioCommands.Count - 1;

            for (var j = startIndex; j >= 0; j--)
            {
                var command = scenarioCommands[j];

                if (!IsDisplayCommand(command))
                {
                    continue;
                }

                targetScenarioIndex = i;
                targetCommandIndex = j;
                return true;
            }
        }

        targetScenarioIndex = -1;
        targetCommandIndex = -1;
        return false;
    }

    private void RestoreScenarioState(
        IReadOnlyList<TextAsset> scenarioJsons,
        int targetScenarioIndex,
        int targetCommandIndex,
        Dictionary<CharacterPosition, ScenarioCommandDto> currentCharacterStates)
    {
        string backgroundAssetId = null;
        string bgmAssetId = null;
        var bgmPlaying = false;

        var targetCharacterStates = new Dictionary<CharacterPosition, ScenarioCommandDto>();

        for (var scenarioIndex = 0; scenarioIndex <= targetScenarioIndex; scenarioIndex++)
        {
            var scenarioCommands = GetScenarioCommands(scenarioJsons, scenarioIndex);

            var endIndex = scenarioIndex == targetScenarioIndex
                ? targetCommandIndex
                : scenarioCommands.Count - 1;

            for (var commandIndex = 0; commandIndex <= endIndex; commandIndex++)
            {
                var command = scenarioCommands[commandIndex];

                switch (command.Type)
                {
                    case "background":
                        backgroundAssetId = command.AssetId;
                        break;

                    case "character":
                        UpdateCharacterState(targetCharacterStates, command);
                        break;

                    case "bgm":
                        UpdateBgmState(command, ref bgmAssetId, ref bgmPlaying);
                        break;
                }
            }
        }

        RestoreBackground(backgroundAssetId);

        RestoreCharacters(currentCharacterStates, targetCharacterStates);

        RestoreBgm(bgmAssetId, bgmPlaying);
    }

    private Dictionary<CharacterPosition, ScenarioCommandDto> GetCharacterStates(
            IReadOnlyList<TextAsset> scenarioJsons,
            int targetScenarioIndex,
            int targetCommandIndex)
    {
        var characterStates =  new Dictionary<CharacterPosition, ScenarioCommandDto>();

        for (var scenarioIndex = 0; scenarioIndex <= targetScenarioIndex; scenarioIndex++)
        {
            var scenarioCommands = GetScenarioCommands(scenarioJsons, scenarioIndex);

            var endIndex = scenarioIndex == targetScenarioIndex
                ? targetCommandIndex
                : scenarioCommands.Count - 1;

            for (var commandIndex = 0; commandIndex <= endIndex; commandIndex++)
            {
                var command = scenarioCommands[commandIndex];

                if (command.Type != "character")
                {
                    continue;
                }

                UpdateCharacterState(characterStates, command);
            }
        }

        return characterStates;
    }

    private static void UpdateCharacterState(
        Dictionary<CharacterPosition, ScenarioCommandDto> characterStates,
        ScenarioCommandDto command)
    {
        var position = ParseCharacterPosition(command.Position);

        switch (command.Action)
        {
            case "show":
                characterStates[position] = command;
                break;

            case "hide":
                characterStates.Remove(position);
                break;
        }
    }

    private static void UpdateBgmState(
        ScenarioCommandDto command,
        ref string bgmAssetId,
        ref bool bgmPlaying)
    {
        switch (command.Action)
        {
            case "play":
                bgmAssetId = command.AssetId;
                bgmPlaying = true;
                break;

            case "stop":
                bgmAssetId = null;
                bgmPlaying = false;
                break;
        }
    }

    private void RestoreBackground(string assetId)
    {
        if (string.IsNullOrEmpty(assetId))
        {
            return;
        }

        backgroundPlayer.Show(assetId);
    }

    private void RestoreCharacters(
        Dictionary<CharacterPosition, ScenarioCommandDto> currentStates,
        Dictionary<CharacterPosition, ScenarioCommandDto> targetStates)
    {
        var positions = new[]
        {
            CharacterPosition.Left,
            CharacterPosition.Center,
            CharacterPosition.Right
        };

        foreach (var position in positions)
        {
            var hasCurrent = currentStates.TryGetValue(position, out var currentCommand);

            var hasTarget = targetStates.TryGetValue(position, out var targetCommand);

            if (!hasTarget)
            {
                if (hasCurrent)
                {
                    characterPlayer.Hide(position);
                }

                continue;
            }

            if (hasCurrent && IsSameCharacterState(currentCommand, targetCommand))
            {
                continue;
            }

            characterPlayer.Show(
                targetCommand.AssetId,
                position,
                targetCommand.Width,
                targetCommand.Height,
                targetCommand.OffsetX,
                targetCommand.OffsetY);
        }
    }

    private void RestoreBgm(string assetId, bool isPlaying)
    {
        if (isPlaying && !string.IsNullOrEmpty(assetId))
        {
            bgmPlayer.Play(assetId);
            return;
        }

        bgmPlayer.Stop();
    }

    private static bool IsSameCharacterState(
        ScenarioCommandDto current,
        ScenarioCommandDto target)
    {
        return current.AssetId == target.AssetId &&
               current.Width == target.Width &&
               current.Height == target.Height &&
               current.OffsetX == target.OffsetX &&
               current.OffsetY == target.OffsetY;
    }

    private static bool IsDisplayCommand(ScenarioCommandDto command)
    {
        return command.Type == "dialogue" ||
               command.Type == "description";
    }

    private static CharacterPosition ParseCharacterPosition(
        string position)
    {
        return position switch
        {
            "left" => CharacterPosition.Left,
            "right" => CharacterPosition.Right,
            _ => CharacterPosition.Center
        };
    }

    private static List<ScenarioCommandDto> GetScenarioCommands(
        IReadOnlyList<TextAsset> scenarioJsons,
        int scenarioIndex)
    {
        var scenarioJson = scenarioJsons[scenarioIndex];
        var scenarioData = JsonConvert.DeserializeObject<ScenarioDataDto>(scenarioJson.text);

        return scenarioData.Commands;
    }
}

public class RewindResult
{
    public int ScenarioIndex { get; }
    public int CommandIndex { get; }
    public List<ScenarioCommandDto> Commands { get; }

    public RewindResult(
        int scenarioIndex,
        int commandIndex,
        List<ScenarioCommandDto> commands)
    {
        ScenarioIndex = scenarioIndex;
        CommandIndex = commandIndex;
        Commands = commands;
    }
}
#endif