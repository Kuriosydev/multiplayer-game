using UnityEngine;

public class TriggerEvents : MonoBehaviour
{
    public float rayDistance = 5f; // How far the player can interact

    public GameObject _quizPanel;
    bool _quizPanelOpened;

    // Update is called once per frame
    void Update()
    {
        Ray ray = new Ray(transform.position, transform.forward);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, rayDistance))
        {
            if (hit.collider.CompareTag("Chest"))
            {
                //Debug.Log("Chest detected at distance: " + hit.distance);

                if(_quizPanelOpened == false)
                {
                    Instantiate(_quizPanel);
                    _quizPanelOpened = true;
                }
            }
            else
            {
                _quizPanelOpened = false;
            }
        }
    }
}
