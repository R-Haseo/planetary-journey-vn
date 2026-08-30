using TMPro;
using UnityEngine;

public class DialogueView : MonoBehaviour
{
    [SerializeField] private TMP_Text speakerText;
    [SerializeField] private TMP_Text dialogueText;

    public void Show(string speaker, string text)
    {
        speakerText.text = speaker;
        dialogueText.text = text;
    }
}
