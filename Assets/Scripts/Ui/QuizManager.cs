using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

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

    public TMP_Text questionText;
    public TMP_Text scoreText;
    public Button[] optionButtons;

    void Start()
    {
        DisplayQuestion();
    }

    void DisplayQuestion()
    {
        if (currentQuestionIndex < questions.Count)
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

            scoreText.text = "Score: " + score;
        }
        else
        {
            questionText.text = "Quiz Finished!";
            scoreText.text = "Final Score: " + score;
            foreach (var btn in optionButtons)
                btn.gameObject.SetActive(false);
        }
    }

    void OnAnswerSelected(int index)
    {
        if (index == questions[currentQuestionIndex].correctAnswerIndex)
            score++;

        currentQuestionIndex++;
        DisplayQuestion();
    }
}
