using UnityEngine;

public class TriggerEvents : MonoBehaviour
{
    public float rayDistance = 5f; // How far the player can interact
    GameObject _quizPanelHandler, _chestobject;
    public GameObject _quizPanel;
    //bool _quizPanelOpened;

    // Update is called once per frame
    //void Update()
    //{
    //    Ray ray = new Ray(transform.position, transform.forward);
    //    RaycastHit hit;

    //    if (Physics.Raycast(ray, out hit, rayDistance))
    //    {
    //        if (hit.collider.CompareTag("Chest"))
    //        {
    //            //Debug.Log("Chest detected at distance: " + hit.distance);

    //            if(_quizPanelOpened == false)
    //            {
    //                _quizPanelHandler = Instantiate(_quizPanel);
    //                _quizPanelOpened = true;
    //            }
    //        }
    //        else
    //        {
    //            if (_quizPanelHandler == null)
    //            {
    //                _quizPanelOpened = false;
    //            }
    //        }
    //    }
    //}

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Chest"))
        {
            _quizPanelHandler = Instantiate(_quizPanel);
        }
        else if (other.CompareTag("Wall"))
        {
            _quizPanelHandler = Instantiate(_quizPanel);
            _quizPanelHandler.GetComponent<QuizManagerJson>()._questionsCount = 2;
            _quizPanelHandler.GetComponent<QuizManagerJson>()._totalQuestion.SetActive(false);
            _quizPanelHandler.GetComponent<QuizManagerJson>()._wall = other.gameObject;
        }
        else if (other.CompareTag("Powerup"))
        {
            _quizPanelHandler = Instantiate(_quizPanel);
            _quizPanelHandler.GetComponent<QuizManagerJson>()._powerUp = int.Parse(other.gameObject.name);
        }
    }
}
