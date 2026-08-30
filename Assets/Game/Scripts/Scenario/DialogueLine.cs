using System;
using UnityEngine;

[Serializable]
public class DialogueLine
{
    public string Speaker;
    public string Message;
    public Sprite Background;
    public AudioClip Voice;

    public DialogueLine(string speaker, string message, Sprite background = null, AudioClip voice = null)
    {
        Speaker = speaker;
        Message = message;
        Background = background;
        Voice = voice;
    }
}
