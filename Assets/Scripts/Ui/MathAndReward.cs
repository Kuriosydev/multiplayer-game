using UnityEngine;

public class MathAndReward : MonoBehaviour
{
    // 1 + 2 - 3 * 4 /; //0 null, 1 fire, 2 ice, 3 gaint; //0 null, 1 sword, 2exp, 3devil.
    [SerializeField] int _plusMiusMultiplayDevide, _devilPower, _rewardType;
    [SerializeField] GameObject _quizPanel;
    bool _isQuizPanelOpen = false;
    GameObject _currentPanel;

    private void OnTriggerEnter(Collider other)
    {
        if (!_isQuizPanelOpen)
        {
            _isQuizPanelOpen = true;
            Instantiate(_quizPanel, FindFirstObjectByType<Canvas>().gameObject.transform);
        }
    }
}
