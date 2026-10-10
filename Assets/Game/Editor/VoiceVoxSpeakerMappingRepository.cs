using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;

public static class VoiceVoxSpeakerMappingRepository
{
    private const string MappingDirectory = "Assets/Game/Data/VoiceVox";

    public static Dictionary<string, string> Load(string episodeId)
    {
        string path = GetPath(episodeId);

        if (!File.Exists(path))
        {
            return new Dictionary<string, string>();
        }

        string json = File.ReadAllText(path);

        var data = JsonConvert.DeserializeObject<VoiceVoxSpeakerMappingData>(json);

        var mappings = new Dictionary<string, string>();

        if (data?.Mappings == null)
        {
            return mappings;
        }

        foreach (VoiceVoxSpeakerMapping mapping in data.Mappings)
        {
            mappings[mapping.ScenarioSpeaker ?? string.Empty] = mapping.VoiceVoxCharacter;
        }

        return mappings;
    }

    public static List<VoiceVoxSpeakerMapping> LoadList(string episodeId)
    {
        string path = GetPath(episodeId);

        if (!File.Exists(path))
        {
            return new List<VoiceVoxSpeakerMapping>();
        }

        string json = File.ReadAllText(path);

        var data = JsonConvert.DeserializeObject<VoiceVoxSpeakerMappingData>(json);

        return data?.Mappings ?? new List<VoiceVoxSpeakerMapping>();
    }

    public static void Save(string episodeId, List<VoiceVoxSpeakerMapping> mappings)
    {
        Directory.CreateDirectory(MappingDirectory);

        var data = new VoiceVoxSpeakerMappingData
        {
            Mappings = mappings
        };

        string json = JsonConvert.SerializeObject(data, Formatting.Indented);

        File.WriteAllText(
            GetPath(episodeId),
            json,
            new System.Text.UTF8Encoding(false));
    }

    private static string GetPath(string episodeId)
    {
        return Path.Combine(MappingDirectory, $"{episodeId}.json");
    }
}

[Serializable]
public class VoiceVoxSpeakerMappingData
{
    public List<VoiceVoxSpeakerMapping> Mappings { get; set; } = new();
}

[Serializable]
public class VoiceVoxSpeakerMapping
{
    public string ScenarioSpeaker { get; set; }
    public string VoiceVoxCharacter { get; set; }
}
