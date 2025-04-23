using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class QuestPanel : MonoBehaviour
{
    public int _questNumber;
    [SerializeField] Button _accept, _neverMind;
    [SerializeField] TMP_Text _quest;
    string _enemyQuest = "Kill 10 Enemies\nReward $1500", _bossQuest = "Kill Hollowood\nReward $3000";

    void Start()
    {
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
        FindFirstObjectByType<UiManager>().QuestStart(_questNumber);
        Destroy(gameObject);
    }
     
    void NeverMind()
    {
        Destroy(gameObject);
    }
}
