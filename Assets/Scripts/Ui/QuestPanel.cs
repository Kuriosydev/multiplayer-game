using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class QuestPanel : MonoBehaviour
{
    public int _questNumber;
    [SerializeField] Button _accept, _neverMind;
    [SerializeField] TMP_Text _quest;
    AudioSource _audioSource;
    [SerializeField] AudioClip _popSound;
    string _enemyQuest = "Defeat 10 Enemies\n$1500, XP 500", _bossQuest = "Defeat Hollowood\n$3000, XP 1000";

    void Start()
    {
        //Reference
        _audioSource = GetComponent<AudioSource>();
        _audioSource.PlayOneShot(_popSound);
        if(_questNumber == 1)
        {
            _quest.text = _enemyQuest;
        }
        else if(_questNumber == 2)
        {
            _quest.text = _bossQuest;
        }
        _accept.onClick.AddListener(Accept);
        _neverMind.onClick.AddListener(NeverMind);
    }

    void Accept()
    {
        //when you accept the quest
        FindFirstObjectByType<UiManager>()._questAccepted = true;
        FindFirstObjectByType<UiManager>().QuestStart(_questNumber, _quest.text);
        Destroy(gameObject);
    }
     
    void NeverMind()
    {
        //when you decline the quest
        FindFirstObjectByType<UiManager>()._questAccepted = false;
        Destroy(gameObject);
    }
}
