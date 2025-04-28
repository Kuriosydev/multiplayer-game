using Photon.Pun;
using System.Threading.Tasks;
using UnityEngine;

public class TriggerEvents : MonoBehaviour
{
    GameObject _quizPanelHandler;
    public GameObject _quizPanel, _questPanel;
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
        if (gameObject.GetComponent<PhotonView>().IsMine)
        {
            if (other.CompareTag("Chest"))
            {
                _quizPanelHandler = Instantiate(_quizPanel);
                _quizPanelHandler.GetComponent<QuizManagerJson>()._chest = true;
                _quizPanelHandler.GetComponent<QuizManagerJson>()._chestObj = other.gameObject;
                other.GetComponent<Animator>().SetBool("Open", false);
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
                if(gameObject.GetComponent<PlayerMovement>()._money >= 300)
                {
                    _quizPanelHandler.GetComponent<QuizManagerJson>()._buy.gameObject.SetActive(true);
                }
            }
            else if (other.CompareTag("Quest"))
            {
                _quizPanelHandler = Instantiate(_questPanel);
                _quizPanelHandler.GetComponent<QuestPanel>()._questNumber = int.Parse(other.gameObject.name);
            }
        }
    }

    public async void ChestClose(GameObject other)
    {
        await Task.Delay(2000);
        other.GetComponent<Animator>().SetBool("Open", false);
    }
}
