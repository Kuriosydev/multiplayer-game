using UnityEngine;
using Photon.Pun;

public class UiManager : MonoBehaviourPunCallbacks
{
    [SerializeField] GameObject _player;
    PlayerMovement _localPlayer;
    int o = 1;

    void Update()
    {
        if (!FindAnyObjectByType<PlayerMovement>() || !FindAnyObjectByType<PlayerMovement>().photonView.IsMine && o > 0)
        {
            o = 0;
            PhotonNetwork.Instantiate(_player.name, _player.transform.position, Quaternion.identity);
            foreach (PlayerMovement player in FindObjectsByType<PlayerMovement>(FindObjectsSortMode.None))
            {
                if (player.photonView.IsMine)
                {
                    _localPlayer = player;
                    player.GetComponent<PlayerMovement>().enabled = true;
                }
            }
        }
    }
}
