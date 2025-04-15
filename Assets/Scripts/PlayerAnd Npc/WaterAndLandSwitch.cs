using UnityEngine;

public class WaterAndLandSwitch : MonoBehaviour
{
    private void OnTriggerStay(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            if (other.GetComponent<PlayerMovement>().photonView.IsMine)
            {
                other.GetComponent<PlayerMovement>().LandSwitch();
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            if (other.GetComponent<PlayerMovement>().photonView.IsMine)
            {
                other.GetComponent<PlayerMovement>().WaterSwitch();
            }
        }
    }
}
