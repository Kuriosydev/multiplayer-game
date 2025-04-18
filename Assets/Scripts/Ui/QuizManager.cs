using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using System.Collections;

public class QuizManager : MonoBehaviour
{
    [System.Serializable]
    public class Question
    {
        public string questionText;
        public string[] options; // length = 4
        public int correctAnswerIndex;
    }

    public List<Question> questions;
    private int currentQuestionIndex = 0;
    private int score = 0;

    public TMP_Text questionText,_resultText;
    public Button[] optionButtons;

    string hexCorrectColor = "#4EEE45";
    string hexWrongColor = "#FA7D78";
    string hexDefaultColor = "#70CCFF";
    Color newColor;

    [SerializeField]
    PlayerTutorial _tutorial;

    void Start()
    {
        DisplayQuestion();
    }

    void DisplayQuestion()
    {
       
            Question q = questions[currentQuestionIndex];
            questionText.text = q.questionText;

            for (int i = 0; i < optionButtons.Length; i++)
            {
                TMP_Text btnText = optionButtons[i].GetComponentInChildren<TMP_Text>();
                btnText.text = q.options[i];

                int index = i; // local copy for closure
                optionButtons[i].onClick.RemoveAllListeners();
                optionButtons[i].onClick.AddListener(() => OnAnswerSelected(index));
            }
    }

    void OnAnswerSelected(int index)
    {
        if (index == questions[currentQuestionIndex].correctAnswerIndex)
        {
            //score++;
            ChangeButtonColor(hexCorrectColor);
            optionButtons[index].transform.GetComponent<Image>().color = newColor;
            _tutorial.SwordMode();
        }
        else
        {
            ChangeButtonColor(hexWrongColor);
            optionButtons[index].transform.GetComponent<Image>().color = newColor;
            _resultText.text = "Wrong !!! Try Again !";
            StartCoroutine(ResetQuestion());
        }
    }

    public void ChangeButtonColor(string hex)
    {

        if (ColorUtility.TryParseHtmlString(hex, out newColor))
        {
            // Change button background color
            //targetButton.image.color = newColor;
        }
        else
        {
            Debug.LogError("Invalid Hex Color Code: " + hex);
        }
    }

    IEnumerator ResetQuestion()
    {
        yield return new WaitForSeconds(2f);
        ChangeButtonColor(hexDefaultColor);
        optionButtons[1].transform.GetComponent<Image>().color = newColor;
        _resultText.text = string.Empty;
    }
}
