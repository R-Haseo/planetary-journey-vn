using TMPro;
using UnityEngine;

public class DialogueView : MonoBehaviour
{
    [SerializeField] private TMP_Text speakerText;
    [SerializeField] private TMP_Text dialogueText;

    public void Show(DialogueLine line)
    {
        speakerText.text = line.Speaker;
        dialogueText.text = line.Message;
    }
}
