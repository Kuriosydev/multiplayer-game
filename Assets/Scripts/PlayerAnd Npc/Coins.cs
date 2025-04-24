using Photon.Pun;
using UnityEngine;

public class Coins : MonoBehaviour
{
    UiManager uiManager;
    private void Start()
    {
        uiManager = FindFirstObjectByType<UiManager>();
    }

    private void Update()
    {
       transform.rotation = Quaternion.Euler(0f, transform.rotation.y + 25f, 0f);
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.collider.gameObject.CompareTag("Player"))
        {
            if (collision.collider.gameObject.GetComponent<PhotonView>().IsMine)
            {
                uiManager.CoinCollected();
                Destroy(gameObject);
            }
        }
    }
}
