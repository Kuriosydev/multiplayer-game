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

    public List<Question> questions;
    

    [SerializeField] AudioClip _correctAnswer, _wrongAnswer, _arenaUnlock, _wallDestroyed, _devilRoar, _chestSound;
    public Button _buy;
    AudioSource _audiosource;
    public GameObject _wall, _coins;
    private bool _isAnswered = false;
    //private List<Question> questions;
    private int currentQuestionIndex = 0;
    private int _score = 0;
    public int _questionsCount = 0;
    public int _powerUp = 0;
    public TMP_Text questionText;
    public TMP_Text scoreText;
    public Button[] optionButtons;
    public bool _chest = false,_bought;
    string hexCorrectColor = "#4EEE45";
    string hexWrongColor = "#FA7D78";
    string hexDefaultColor = "#70CCFF";

    [SerializeField]
    Image[] _questionsNo;

    [SerializeField]
    Sprite[] _checkImages;

    Color newColor;
    GameObject _particle;
    public
    GameObject _quizPanel, _totalQuestion, _wallDestroy, _chestObj;
    void Start()
    {
        _audiosource = GetComponent<AudioSource>();
        _buy.onClick.AddListener(Close);
        for(int i = 40; i< 5000; i++)
        {
            questions.Add(questions[i - 39]);
        }
        //StartCoroutine(LoadQuestions());
         DisplayQuestion();
    }

    //void PCLoadQuestions()
    //{
    //    string filePath = Path.Combine(Application.streamingAssetsPath, "questions.json");

    //    if (File.Exists(filePath))
    //    {
    //        string json = File.ReadAllText(filePath);
    //        QuestionList ql = JsonUtility.FromJson<QuestionList>(json);
    //        questions = ql.questions;
    //    }
    //    else
    //    {
    //        Debug.LogError("JSON file not found at: " + filePath);
    //        questions = new List<Question>();
    //    }
    //}

//    IEnumerator LoadQuestions()
//    {
//        //TextAsset jsonFile = Resources.Load<TextAsset>("questions");

//        string filePath = Path.Combine(Application.streamingAssetsPath, "questions.json");

//        UnityWebRequest www = UnityWebRequest.Get(filePath);
//        yield return www.SendWebRequest();

//#if UNITY_2020_1_OR_NEWER
//        if (www.result != UnityWebRequest.Result.Success)
//#else
//    if (www.isNetworkError || www.isHttpError)
//#endif
//        {
//            Debug.LogError("Failed to load questions: " + www.error);
//        }
//        else
//        {
//            string json = www.downloadHandler.text;
//            QuestionList ql = JsonUtility.FromJson<QuestionList>(json);
//            questions = ql.questions;
//            DisplayQuestion();
//        }
//    }

    void DisplayQuestion()
    {
        //dispalyes question
        _isAnswered = false;
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

            scoreText.text = "Score: " + _score;
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
        //check if answer is selected
        if (!_isAnswered)
        {
            if (index == questions[currentQuestionIndex].correctAnswerIndex)
            {
                _score++;
                ChangeButtonColor(hexCorrectColor);
                optionButtons[index].transform.GetComponent<Image>().color = newColor;
                _questionsNo[_questionsCount].sprite = _checkImages[0];
                _audiosource.PlayOneShot(_arenaUnlock);
                Invoke("WallsDestroy", 1.1f);
                if (_wall != null)
                {
                    Invoke("UniqueSound", 0.7f);
                }
            }
            else
            {
                _audiosource.PlayOneShot(_wrongAnswer);
                ChangeButtonColor(hexWrongColor);
                optionButtons[index].transform.GetComponent<Image>().color = newColor;
                _questionsNo[_questionsCount].sprite = _checkImages[1];
            }
            _isAnswered = true;
            if (_powerUp == 0)
            {
                Invoke("UniqueSound", 0.6f);
            }
            else if (_powerUp > 0)
            {
                Invoke("UniqueSound", 0.6f);
            }
            StartCoroutine(NextQuestion());
        }
    }

    IEnumerator NextQuestion()
    {
        //moves to next questions
        questions.RemoveAt(currentQuestionIndex);
        yield return new WaitForSeconds(1.2f);
        _questionsCount++;
        DisplayQuestion();
    }

    public void ChangeButtonColor(string hex)
    {
        //changes the color
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

    void WallsDestroy()
    {
        // wall question answer destroy wall
        if (_wall != null)
        {
            Instantiate(_coins, _wall.transform.position, Quaternion.identity);
            Instantiate(_coins, _wall.transform.position, Quaternion.identity);
            Instantiate(_coins, _wall.transform.position, Quaternion.identity);
            Instantiate(_coins, _wall.transform.position, Quaternion.identity);
            _particle = Instantiate(_wallDestroy, new Vector3(_wall.transform.position.x,
            FindFirstObjectByType<UiManager>()._localPlayer.transform.position.y,
            FindFirstObjectByType<UiManager>()._localPlayer.transform.position.z), Quaternion.identity);
            _wall.SetActive(false);
            Destroy(_particle, 0.2f);
            FindFirstObjectByType<UiManager>().WallReappear(_wall);
        }
    }

    void UniqueSound()
    {
        //unique souns as per question the wall,chest or dealers
        if (_wall != null && _score == 1)
        {
            _audiosource.Stop();
            _audiosource.PlayOneShot(_wallDestroyed);
        }
        if (_questionsCount == 2)
        {
            if (_powerUp == 0 && _wall == null)
            {
                _audiosource.Stop();
                FindFirstObjectByType<UiManager>().MoneyIncrease();
                _audiosource.PlayOneShot(_chestSound);
            }
            else if (_powerUp > 0 && _score == 3)
            {
                _audiosource.Stop();
                _audiosource.PlayOneShot(_devilRoar);
            }
        }
    }

    public void Buy()
    {
        // when you buy devil fruit
        _bought = true;
        Close();
    }

    public void Close()
    {
        // destroys the question panel
        foreach (TriggerEvents i in FindObjectsByType<TriggerEvents>(FindObjectsSortMode.None))
        {
            if (i.isActiveAndEnabled)
            {
                if (_powerUp > 0 && _score == 3)
                {
                    i.gameObject.GetComponent<PlayerMovement>()._devilFruit = _powerUp;
                    i.gameObject.GetComponent<PlayerMovement>()._timer = 120f;
                    i.gameObject.GetComponent<PlayerMovement>().NormalsMode();
                }
                if (_chest)
                {
                    i.gameObject.GetComponent<PlayerMovement>()._money += _score * 200;
                    _chestObj.GetComponent<Animator>().SetBool("Open", true);
                    //Instantiate(_coins, _chestObj.transform.position, Quaternion.identity);
                    //Instantiate(_coins, _chestObj.transform.position, Quaternion.identity);
                    //Instantiate(_coins, _chestObj.transform.position, Quaternion.identity);
                    //Instantiate(_coins, _chestObj.transform.position, Quaternion.identity);
                    i.ChestClose(_chestObj);
                }
                try
                {
                    if (_bought)
                    {
                        i.gameObject.GetComponent<PlayerMovement>()._money -= 300;
                        i.gameObject.GetComponent<PlayerMovement>()._devilFruit = _powerUp;
                        i.gameObject.GetComponent<PlayerMovement>()._timer = 120f;
                        i.gameObject.GetComponent<PlayerMovement>().NormalsMode();
                    }
                }
                catch
                {

                }
                i.gameObject.GetComponent<PlayerMovement>().ScoreIncrease(_score);
            }
        }
        Destroy(_quizPanel);
    }
}
