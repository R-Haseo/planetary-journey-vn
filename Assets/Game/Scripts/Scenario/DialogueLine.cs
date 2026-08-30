using System;
using UnityEngine;

[Serializable]
public class DialogueLine
{
    public string Speaker;
    public string Message;
    public Sprite Background;

    public DialogueLine(string speaker, string message, Sprite background)
    {
        Speaker = speaker;
        Message = message;
        Background = background;
    }
}
