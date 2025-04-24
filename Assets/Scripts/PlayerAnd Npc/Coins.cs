using Photon.Pun;
using UnityEngine;

public class Coins : MonoBehaviour
{
    public Vector3 rotationSpeed = new Vector3(0, 180, 0);
    UiManager uiManager;
    private void Start()
    {
        uiManager = FindFirstObjectByType<UiManager>();
    }

    private void Update()
    {
        transform.Rotate(rotationSpeed * Time.deltaTime);
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
