using UnityEngine;

public class WaterAndLandSwitch : MonoBehaviour
{
    //make the player recognise it's not on land
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

    //make the player recognise it's on land
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
