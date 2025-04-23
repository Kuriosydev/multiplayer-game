using Photon.Pun;
using Photon.Realtime;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MultiplayerLobby : MonoBehaviourPunCallbacks
{
    [SerializeField] GameObject _roomJoiningPanel, _ConnectionPanel, _nameItemprefab, _customJoin, _directJoin, _playerSelection;//, _roomItemprefab;
    [SerializeField] TMP_InputField _playerName, _randomPlayerName, _roomName;
    [SerializeField] TMP_Text _roomNameAndCreater, _roomServerMessages;
    [SerializeField] Transform _playerNameSpawnLocation;//, _roomNameSpawnLocation;
    public bool _join = true, _create = true, _connected = false, _randomJoinCreate = false;


    private void Start()
    {
        _randomPlayerName.onSelect.AddListener(NameSelect);
        _randomPlayerName.onDeselect.AddListener(NameDeselect);
        PhotonNetwork.ConnectUsingSettings();
        PhotonNetwork.EnableCloseConnection = true;
        PhotonNetwork.AutomaticallySyncScene = true;
        PhotonNetwork.ConnectToBestCloudServer();
        PhotonNetwork.ConnectToRegion("us");
    }

    void NameSelect(string i)
    {
        if (i.Length < 1)
        {
            _randomPlayerName.text = " ";
        }
    }

    void NameDeselect(string i)
    {
        if (i == " ")
        {
            _randomPlayerName.text = "";
        }
    }

    private void Update()
    {
        if (PhotonNetwork.IsConnectedAndReady)
        {
            if (!_connected)
            {
                if (!PhotonNetwork.InLobby && PhotonNetwork.CurrentLobby == null)
                {
                    PhotonNetwork.JoinLobby();
                }
                if (PhotonNetwork.InLobby)
                {
                    _ConnectionPanel.SetActive(false);
                    _roomJoiningPanel.SetActive(true);
                    _connected = true;
                }
            }
            try
            {
                if (!_randomJoinCreate)
                {
                    if (_playerName.text.Length > 1)
                    {
                        PhotonNetwork.LocalPlayer.NickName = _playerName.text;
                    }
                }
                else
                {
                    if (_randomPlayerName.text.Length > 1)
                    {
                        PhotonNetwork.LocalPlayer.NickName = _randomPlayerName.text;
                    }
                }
            }
            catch { }
        }
    }

    public void JoinRoom()
    {
        if (_roomName.text.Length > 2)
        {
            if (_join)
            {
                _join = false;
                PhotonNetwork.JoinRoom(_roomName.text);
                Invoke("CanJoinReset", 0.5f);
            }
        }
        else
        {
            _roomServerMessages.text = "Room doesn't exist...";
            Invoke("Messagenull", 5f);
        }
    }

    public void Play()
    {
        _directJoin.SetActive(false);
        _playerSelection.SetActive(true);
    }
    
    public void OncolorClicked(int _color)
    {
        _customJoin.SetActive(true);
        _playerSelection.SetActive(false);
        PlayerPrefs.SetInt("color", _color);
    }

    public void CreateRoom()
    {
        if (_roomName.text.Length > 2)
        {
            if (_create)
            {
                if (!PhotonNetwork.CreateRoom(_roomName.text, new RoomOptions() { IsVisible = false, MaxPlayers = 10 }))
                {
                    _create = true;
                    Invoke("JoinReset", 0.5f);
                    _roomServerMessages.text = "Room Code is already in use...";
                    Invoke("Messagenull", 4f);
                }
            }
        }
        else
        {
            _roomServerMessages.text = "Room name Length should be more than 2...";
            Invoke("Messagenull", 5f);
        }
    }

    public void Tutorial()
    {
        SceneManager.LoadScene(2);
    }

    public void JoinorCreateRandom()
    {
        PhotonNetwork.JoinRandomOrCreateRoom();
    }

    public override void OnJoinedRoom()
    {
        base.OnJoinedRoom();
        _connected = true;
        if (_randomJoinCreate)
        {
            PhotonNetwork.LoadLevel(1);
        }
        else
        {
            _roomNameAndCreater.text = "Room: '" + PhotonNetwork.CurrentRoom.Name + "' Created by: '" + PhotonNetwork.MasterClient.NickName + "'";
            //_roomJoiningPanel.SetActive(false);
            //_waitingRoomPanel.SetActive(true);
            Dictionary<int, Player> i = PhotonNetwork.CurrentRoom.Players;
            foreach (Player player in i.Values)
            {
                GameObject text = Instantiate(_nameItemprefab, _playerNameSpawnLocation);
                text.name = player.UserId;
                text.GetComponent<TMP_Text>().text = player.NickName;
            }
            StartClicked();
        }
    }

    public override void OnJoinRoomFailed(short returnCode, string message)
    {
        base.OnJoinRoomFailed(returnCode, message);
        _roomServerMessages.text = "Room doesn't exist...";
        Invoke("Messagenull", 5f);
    }

    public override void OnJoinRandomFailed(short returnCode, string message)
    {
        base.OnJoinRandomFailed(returnCode, message);
        _roomServerMessages.text = "No Room Available...";
        Invoke("Messagenull", 5f);
    }

    public override void OnLeftRoom()
    {
        base.OnLeftRoom();
        GameObject[] nameobj = GameObject.FindGameObjectsWithTag("Name");
        foreach (GameObject game in nameobj)
        {
            Destroy(game);
        }
    }

    public override void OnCreatedRoom()
    {
        base.OnCreatedRoom();
        if (_randomJoinCreate)
        {
            PhotonNetwork.LoadLevel(1);
        }
        else
        {
            _connected = true;
            _roomNameAndCreater.text = "Room: '" + PhotonNetwork.CurrentRoom.Name + "' Created by: '" + PhotonNetwork.MasterClient.NickName + "'";
            _create = false;
            //_roomJoiningPanel.SetActive(false);
            //_waitingRoomPanel.SetActive(true);
            //StartClicked();
        }
    }

    public void MainMenu()
    {
        SceneManager.LoadScene(0);
    }

    public void MainMenuRoomLeft()
    {
        SceneManager.LoadScene(0);
        PhotonNetwork.LeaveRoom();
    }

    public void RoomExit()
    {
        foreach (GameObject Name in GameObject.FindGameObjectsWithTag("Name"))
        {
            Destroy(Name);
        }
        if (PhotonNetwork.LeaveRoom())
        {
            PhotonNetwork.Disconnect();
            if (PhotonNetwork.ConnectUsingSettings())
            {
                PhotonNetwork.JoinLobby();
                _roomJoiningPanel.SetActive(true);
                _create = true;
                //_waitingRoomPanel.SetActive(false);
            }
        }
    }

    public void StartClicked()
    {
        //if (PhotonNetwork.LocalPlayer.UserId == PhotonNetwork.MasterClient.UserId)
        //{
            PhotonNetwork.CurrentRoom.IsVisible = false;
            PhotonNetwork.AutomaticallySyncScene = true;
            PhotonNetwork.LoadLevel(1);
        //}
    }

    void Messagenull()
    {
        _roomServerMessages.text = "";
    }

    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        base.OnPlayerEnteredRoom(newPlayer);
        GameObject[] nameobj = GameObject.FindGameObjectsWithTag("Name");
        foreach (GameObject game in nameobj)
        {
            Destroy(game);
        }
        Dictionary<int, Player> i = PhotonNetwork.CurrentRoom.Players;
        foreach (Player player in i.Values)
        {
            GameObject text = Instantiate(_nameItemprefab, _playerNameSpawnLocation);
            text.name = player.UserId;
            text.GetComponent<TMP_Text>().text = player.NickName;
        }
        _roomNameAndCreater.text = "Room: '" + PhotonNetwork.CurrentRoom.Name + "' Created by: '" + PhotonNetwork.MasterClient.NickName + "'";
    }

    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        base.OnPlayerLeftRoom(otherPlayer);
        GameObject[] nameobj = GameObject.FindGameObjectsWithTag("Name");
        foreach (GameObject game in nameobj)
        {
            Destroy(game);
        }
        Dictionary<int, Player> i = PhotonNetwork.CurrentRoom.Players;
        foreach (Player player in i.Values)
        {
            GameObject text = Instantiate(_nameItemprefab, _playerNameSpawnLocation);
            text.name = player.UserId;
            text.GetComponent<TMP_Text>().text = player.NickName;
        }
    }

    public void CustomJoinOnOff()
    {
        if (_customJoin.activeSelf)
        {
            _customJoin.SetActive(false);
            _directJoin.SetActive(true);
        }
        else
        {
            _customJoin.SetActive(true);
            _directJoin.SetActive(false);
        }
    }

    //Room availanle update code:-
    //public override void OnRoomListUpdate(List<RoomInfo> roomList)
    //{
    //    base.OnRoomListUpdate(roomList);
    //    foreach (GameObject room in GameObject.FindGameObjectsWithTag("Room"))
    //    {
    //        Destroy(room);
    //    }
    //    foreach (RoomInfo room in roomList)
    //    {
    //        GameObject NewRoom = Instantiate(_roomItemprefab, _roomNameSpawnLocation);
    //        NewRoom.GetComponent<RoomClicked>()._roomName = room.Name;
    //        NewRoom.GetComponent<RoomClicked>()._totalPlayers = room.PlayerCount.ToString() + "/" + room.MaxPlayers.ToString();
    //    }
    //}

    void CanJoinReset()
    {
        _join = true;
    }

    void JoinReset()
    {
        _create = true;
    }

}