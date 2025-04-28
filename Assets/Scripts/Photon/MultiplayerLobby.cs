using Photon.Pun;
using Photon.Realtime;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MultiplayerLobby : MonoBehaviourPunCallbacks
{
    [SerializeField] GameObject _roomJoiningPanel, _ConnectionPanel, _nameItemprefab, _customJoin, _directJoin, _playerSelection;//, _roomItemprefab;
    [SerializeField]
    GameObject[] _parts;
    [SerializeField] TMP_InputField _playerName, _randomPlayerName, _roomNameCreate, _roomNameJoin;
    [SerializeField] TMP_Text _roomNameAndCreater, _roomServerMessages;
    [SerializeField] Transform _playerNameSpawnLocation;//, _roomNameSpawnLocation;
    public bool _join = true, _create = true, _connected = false, _randomJoinCreate = false;


    private void Start()
    {
        if (PlayerPrefs.HasKey("exit"))
        {
            if (PlayerPrefs.GetInt("exit") == 1)
            {
                _ConnectionPanel.SetActive(false);
                _roomJoiningPanel.SetActive(true);
                _connected = true;
                PlayerPrefs.SetInt("exit", 0);
            }
        }
        else
        {
            PlayerPrefs.SetInt("exit", 0);
        }
        PhotonNetwork.Disconnect();
        _playerName.onSelect.AddListener(NameSelect);
        _playerName.onDeselect.AddListener(NameDeselect);
        _roomNameJoin.onSelect.AddListener(RoomSelectJoinCode);
        _roomNameJoin.onDeselect.AddListener(RoomDeselectJoinCode);
        _roomNameCreate.onSelect.AddListener(RoomSelectCreateCode);
        _roomNameCreate.onDeselect.AddListener(RoomDeselectCreateCode);
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
            _playerName.text = " ";
        }
    }

    void NameDeselect(string i)
    {
        if (i == " ")
        {
            _playerName.text = "";
        }
    }

    void RoomSelectJoinCode(string i)
    {
        if (i.Length < 1)
        {
            _roomNameJoin.text = " ";
        }
    }

    void RoomDeselectJoinCode(string i)
    {
        if (i == " ")
        {
            _roomNameJoin.text = "";
        }
    }

    void RoomSelectCreateCode(string i)
    {
        if (i.Length < 1)
        {
            _roomNameCreate.text = " ";
        }
    }

    void RoomDeselectCreateCode(string i)
    {
        if (i == " ")
        {
            _roomNameCreate.text = "";
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
        if (_roomNameJoin.text.Length > 2)
        {
            if (_join)
            {
                _join = false;
                PhotonNetwork.JoinRoom(_roomNameJoin.text);
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
    
    public void OncolorClicked(int _colorNumber)
    {
        _customJoin.SetActive(true);
        _playerSelection.SetActive(false);
        PlayerPrefs.SetInt("color", _colorNumber);
        foreach (GameObject _part in _parts)
        {
            if (_colorNumber == 1)
            {
                _part.GetComponent<Renderer>().material.color = Color.red;
            }
            else if (_colorNumber == 2)
            {
                _part.GetComponent<Renderer>().material.color = Color.blue;
            }
            else if (_colorNumber == 3)
            {
                _part.GetComponent<Renderer>().material.color = Color.magenta;
            }
            else if (_colorNumber == 4)
            {
                _part.GetComponent<Renderer>().material.color = Color.green;
            }
        }
    }

    public void CreateRoom()
    {
        if (_roomNameCreate.text.Length > 2)
        {
            if (_create)
            {
                if (!PhotonNetwork.CreateRoom(_roomNameCreate.text, new RoomOptions() { IsVisible = false, MaxPlayers = 10 }))
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
        PlayerPrefs.SetInt("exit",1);
        SceneManager.LoadScene(2);
    }

    public void JoinorCreateRandom()
    {
        PhotonNetwork.JoinRandomOrCreateRoom();
        _randomJoinCreate = true;
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
        _directJoin.SetActive(true);
        _playerSelection.SetActive(false);
        _customJoin.SetActive(false);
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


    private void OnApplicationQuit()
    {
        PlayerPrefs.SetInt("exit", 0);
    }
}