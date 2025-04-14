using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.IO;
using System.Collections;
using UnityEngine.Networking;

public class QuizManagerJson : MonoBehaviour
{
    [System.Serializable]
    public class Question
    {
        public string questionText;
        public string[] options;
        public int correctAnswerIndex;
    }

    [System.Serializable]
    public class QuestionList
    {
        public List<Question> questions;
    }

    private List<Question> questions;
    private int currentQuestionIndex = 0;
    private int score = 0;
    private int _questionsCount = 0;

    public TMP_Text questionText;
    public TMP_Text scoreText;
    public Button[] optionButtons;

    string hexCorrectColor = "#4EEE45";
    string hexWrongColor = "#FA7D78";
    string hexDefaultColor = "#70CCFF";

    [SerializeField]
    Image[] _questionsNo;

    [SerializeField]
    Sprite[] _checkImages;

    Color newColor;

    [SerializeField]
    GameObject _quizPanel;
    void Start()
    {
        StartCoroutine(LoadQuestions());
        //DisplayQuestion();
    }

    void PCLoadQuestions()
    {
        string filePath = Path.Combine(Application.streamingAssetsPath, "questions.json");

        if (File.Exists(filePath))
        {
            string json = File.ReadAllText(filePath);
            QuestionList ql = JsonUtility.FromJson<QuestionList>(json);
            questions = ql.questions;
        }
        else
        {
            Debug.LogError("JSON file not found at: " + filePath);
            questions = new List<Question>();
        }
    }

    IEnumerator LoadQuestions()
    {
        string filePath = Path.Combine(Application.streamingAssetsPath, "questions.json");

        UnityWebRequest www = UnityWebRequest.Get(filePath);
        yield return www.SendWebRequest();

#if UNITY_2020_1_OR_NEWER
        if (www.result != UnityWebRequest.Result.Success)
#else
    if (www.isNetworkError || www.isHttpError)
#endif
        {
            Debug.LogError("Failed to load questions: " + www.error);
        }
        else
        {
            string json = www.downloadHandler.text;
            QuestionList ql = JsonUtility.FromJson<QuestionList>(json);
            questions = ql.questions;
            DisplayQuestion();
        }
    }

    void DisplayQuestion()
    {
        currentQuestionIndex = Random.Range(0, questions.Count - 1);
        //if (currentQuestionIndex < questions.Count)
        if (_questionsCount < 3)
        {
            Question q = questions[currentQuestionIndex];
            questionText.text = q.questionText;

            ChangeButtonColor(hexDefaultColor);
            for (int i = 0; i < optionButtons.Length; i++)
            {
                TMP_Text btnText = optionButtons[i].GetComponentInChildren<TMP_Text>();
                btnText.text = q.options[i];

                int index = i;
                optionButtons[i].onClick.RemoveAllListeners();
                optionButtons[i].onClick.AddListener(() => OnAnswerSelected(index));
                optionButtons[i].transform.GetComponent<Image>().color = newColor;
            }

            scoreText.text = "Score: " + score;
        }
        else
        {
            //questionText.text = "Quiz Finished!";
            //scoreText.text = "Final Score: " + score;
            //foreach (var btn in optionButtons)
            //    btn.gameObject.SetActive(false);
            Close();
        }
    }

    void OnAnswerSelected(int index)
    {
        if (index == questions[currentQuestionIndex].correctAnswerIndex)
        {
            score++;
            ChangeButtonColor(hexCorrectColor);
            optionButtons[index].transform.GetComponent<Image>().color = newColor;

            _questionsNo[_questionsCount].sprite = _checkImages[0];
        }
        else
        {
            ChangeButtonColor(hexWrongColor);
            optionButtons[index].transform.GetComponent<Image>().color = newColor;
            _questionsNo[_questionsCount].sprite = _checkImages[1];
        }

        StartCoroutine(NextQuestion());
    }

    IEnumerator NextQuestion()
    {
        questions.RemoveAt(currentQuestionIndex);
        yield return new WaitForSeconds(2f);
        _questionsCount++;
        DisplayQuestion();
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

    public void Close()
    {
        Destroy(_quizPanel);
    }
}
