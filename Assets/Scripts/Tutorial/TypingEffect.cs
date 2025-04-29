using System.Collections;
using UnityEngine;
using TMPro;

public class TypingEffect : MonoBehaviour
{
    public TMP_Text textMeshPro; // Reference to the TextMeshPro component
    public float typingSpeed = 0.5f; // Speed of typing in seconds

    private string fullText; // The complete text to be typed

    [SerializeField]
    GameObject _welcomePanel,_player;

    private void Start()
    {
        //assign the vales
        fullText = textMeshPro.text; // Store the full text
        textMeshPro.text = string.Empty; // Clear the text
        StartCoroutine(TypeText()); // Start typing animation
    }

    // Coroutine to simulate typing effect
    IEnumerator TypeText()
    {
        //types the text in tutortial
        foreach (char letter in fullText)
        {
            textMeshPro.text += letter; // Append each letter to the text
            yield return new WaitForSeconds(typingSpeed); // Wait for the specified duration
        }

        if(textMeshPro.text == fullText)
        {
            yield return new WaitForSeconds(2f);
            _player.SetActive(true);
            _welcomePanel.SetActive(false);
        }
    }
}
