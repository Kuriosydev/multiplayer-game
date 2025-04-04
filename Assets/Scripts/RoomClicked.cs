using Photon.Pun;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class RoomClicked : MonoBehaviourPunCallbacks
{
    public string _roomName, _totalPlayers;
    [SerializeField] Button _btn;
    [SerializeField] TMP_Text _roomNameText, _totalPlayersText;
    private void Start()
    {
        _btn.onClick.AddListener(RoomBtnClicked);
        _roomNameText.text = _roomName;
        _totalPlayersText.text = _totalPlayers;
    }

    void RoomBtnClicked()
    {
        PhotonNetwork.JoinRoom(_roomName);

    }
}
