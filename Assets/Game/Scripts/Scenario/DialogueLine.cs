using System;

[Serializable]
public class DialogueLine
{
    public string Speaker;
    public string Message;

    public DialogueLine(string speaker, string message)
    {
        Speaker = speaker;
        Message = message;
    }
}
