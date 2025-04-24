using Photon.Pun;
using Photon.Realtime;
using Photon.Voice.Unity;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class UiManager : MonoBehaviourPunCallbacks
{
    [SerializeField] TMP_Text _healthText, _energyText, _expText , _levelText, _money, _rank, _quest, _requireMents;
    [SerializeField] Sprite _micOn, _micOff;
    public Vector3[] _playerPosition, _guardPositions, _chiefPosition, _traineePosition;
    [SerializeField] Image _healthBar, _energyBar, _expBar;
    [SerializeField] Sprite _buttonSelected, _buttonUnSelected;
    public AudioClip _coins;
    [SerializeField] Button _jump, _dash, _mic, _meleeMode, _swordMode, _devilMode;
    [SerializeField] GameObject _player, _guard, _chief, _trainee, _dummyBoss, _questIndicator;
    public PlayerMovement _localPlayer;
    public int _reSpawn = 1, _questType;
    [SerializeField] Recorder _recorder;
    [SerializeField] Slider _slider;
    public bool _questAccepted = false;
    public int _kills, _requiredKills;

    private void Start()
    {
        _slider.onValueChanged.AddListener(SliderValueChanged);
        _jump.onClick.AddListener(JumpClicked);
        _meleeMode.onClick.AddListener(MeleeMode);
        _swordMode.onClick.AddListener(SwordMode);
        _devilMode.onClick.AddListener(DevilMode);
        _dash.onClick.AddListener(Dash);
        _mic.onClick.AddListener(MicOnOff);
        if (PhotonNetwork.IsMasterClient)
        {
            foreach (Vector3 _pos in _guardPositions)
            {
                GameObject _newGuard = PhotonNetwork.InstantiateRoomObject(_guard.name, _pos, Quaternion.identity);
                _newGuard.GetComponent<Enemy>()._startPos = _pos;
            }
            foreach (Vector3 _pos in _chiefPosition)
            {
                GameObject _newChief = PhotonNetwork.InstantiateRoomObject(_chief.name, _pos, Quaternion.identity);
                _newChief.GetComponent<Enemy>()._startPos = _pos;
            }
            foreach (Vector3 _pos in _traineePosition)
            {
                GameObject _newTrainee = PhotonNetwork.InstantiateRoomObject(_trainee.name, _pos, Quaternion.identity);
                _newTrainee.GetComponent<Enemy>()._startPos = _pos;
            }
            GameObject _boss = PhotonNetwork.InstantiateRoomObject(_dummyBoss.name, _dummyBoss.transform.position, Quaternion.identity);
            _boss.GetComponent<Enemy>()._startPos = _dummyBoss.transform.position;
        }
    }

    void Update()
    {
        if (!FindAnyObjectByType<PlayerMovement>() || !FindAnyObjectByType<PlayerMovement>().photonView.IsMine && _reSpawn > 0)
        {
            _reSpawn = 0;
            PhotonNetwork.Instantiate(_player.name, _playerPosition[Random.Range(0, _playerPosition.Length - 1)], Quaternion.identity);
            foreach (PlayerMovement player in FindObjectsByType<PlayerMovement>(FindObjectsSortMode.None))
            {
                if (player.photonView.IsMine)
                {
                    _localPlayer = player;
                    if (PhotonNetwork.LocalPlayer.NickName == "")
                    {
                        PhotonNetwork.LocalPlayer.NickName = "Player" + PhotonNetwork.LocalPlayer.ActorNumber.ToString();
                    }
                }
            }
        }
        if (PhotonNetwork.MasterClient.IsInactive)
        {
            if (!PhotonNetwork.LocalPlayer.IsMasterClient)
            {
                PhotonNetwork.SetMasterClient(PhotonNetwork.LocalPlayer);
            }
        }
        if (_localPlayer != null)
        {
            _energyBar.fillAmount = _localPlayer._currentEnergy / _localPlayer._maxEnergy;
            _energyText.text = ((int)_localPlayer._currentEnergy).ToString() + "/" + _localPlayer._maxEnergy.ToString();
            _healthBar.fillAmount = _localPlayer._currentHealth / _localPlayer._maxHealth;
            _healthText.text = ((int)_localPlayer._currentHealth).ToString() + "/" + _localPlayer._maxHealth.ToString();
            _expBar.fillAmount = (float)_localPlayer.GetComponent<PlayerMovement>()._exp / (float)_localPlayer.GetComponent<PlayerMovement>()._expRequired;
            _expText.text = _localPlayer.GetComponent<PlayerMovement>()._exp.ToString() + "/" + _localPlayer.GetComponent<PlayerMovement>()._expRequired.ToString();
            _devilMode.image.fillAmount = _localPlayer.GetComponent<PlayerMovement>()._timer / 120f;
            _money.text = "$ " + _localPlayer.GetComponent<PlayerMovement>()._money.ToString();
            _rank.text = "Rank " + _localPlayer.GetComponent<PlayerMovement>()._rank.ToString();
            if (_localPlayer.GetComponent<PlayerMovement>()._timer <= 0)
            {
                _devilMode.gameObject.SetActive(false);
            }
            if (_localPlayer.GetComponent<PlayerMovement>()._devilFruit > 0)
            {
                _devilMode.gameObject.SetActive(true);
            }
            if(_localPlayer.GetComponent<PlayerMovement>()._state == 1)
            {
                if (_meleeMode.gameObject.GetComponent<Image>().sprite != _buttonSelected)
                {
                    _meleeMode.gameObject.GetComponent<Image>().sprite = _buttonSelected;
                    _swordMode.gameObject.GetComponent<Image>().sprite = _buttonUnSelected;
                    _devilMode.gameObject.GetComponent<Image>().sprite = _buttonUnSelected;
                }
            }
            else if (_localPlayer.GetComponent<PlayerMovement>()._state == 2)
            {
                if (_swordMode.gameObject.GetComponent<Image>().sprite != _buttonSelected)
                {
                    _meleeMode.gameObject.GetComponent<Image>().sprite = _buttonUnSelected;
                    _swordMode.gameObject.GetComponent<Image>().sprite = _buttonSelected;
                    _devilMode.gameObject.GetComponent<Image>().sprite = _buttonUnSelected;
                }
            }
            else if (_localPlayer.GetComponent<PlayerMovement>()._state == 3)
            {
                if (_devilMode.gameObject.GetComponent<Image>().sprite != _buttonSelected)
                {
                    _meleeMode.gameObject.GetComponent<Image>().sprite = _buttonUnSelected;
                    _swordMode.gameObject.GetComponent<Image>().sprite = _buttonUnSelected;
                    _devilMode.gameObject.GetComponent<Image>().sprite = _buttonSelected;
                }
            }
            else
            {
                _meleeMode.gameObject.GetComponent<Image>().sprite = _buttonUnSelected;
                _swordMode.gameObject.GetComponent<Image>().sprite = _buttonUnSelected;
                _devilMode.gameObject.GetComponent<Image>().sprite = _buttonUnSelected;
            }
            _levelText.text = "Level " + _localPlayer._level.ToString();
        }
    }

    public void ParticleDestroy(GameObject particle)
    {
        DestroyImmediate(particle,true);
    }

    public void QuestStart(int number, string text)
    {
        _questIndicator.SetActive(true);
        _kills = 0;
        _quest.text = text;
        _questType = number;
        if (number == 1)
        {
            _requiredKills = 8;
            _requireMents.text = _kills.ToString() +"/"+ _requiredKills.ToString();
        }
        else if(number == 2)
        {
            _requiredKills = 1;
            _requireMents.text = _kills.ToString() + "/" + _requiredKills.ToString();
        }
    }

    public void CoinCollected()
    {
        GetComponent<AudioSource>().PlayOneShot(_coins);
        if(_localPlayer != null)
        {
            _localPlayer._money += 10;
            MoneyIncrease();
        }

    }

    public void QuestCompleteCheck()
    {
        _requireMents.text = _kills.ToString() + "/" + _requiredKills.ToString();
        if (_questType == 1)
        {
            if(_kills == _requiredKills)
            {
                MoneyIncrease();
                _localPlayer._exp += 500;
                _localPlayer._money += 1500;
                _questAccepted = false;
                _kills = 0;
                _requiredKills = 0;
                _questIndicator.SetActive(false);
            }
        }
        else if(_questType == 2) 
        {
            if(_kills == _requiredKills)
            {
                MoneyIncrease();
                _localPlayer._exp += 1000;
                _localPlayer._money += 3000;
                _questAccepted = false;
                _kills = 0;
                _requiredKills = 0 ;
                _questIndicator.SetActive(false);
            }
        }
    }

    public async void MoneyIncrease()
    {
        await Task.Delay(500);
        _money.gameObject.SetActive(false);
        await Task.Delay(500);
        _money.gameObject.SetActive(true);
        await Task.Delay(500);
        _money.gameObject.SetActive(false);
        await Task.Delay(500);
        _money.gameObject.SetActive(true);
        await Task.Delay(500);
        _money.gameObject.SetActive(false);
        await Task.Delay(500);
        _money.gameObject.SetActive(true);
        await Task.Delay(500);
        _money.gameObject.SetActive(false);
        await Task.Delay(500);
        _money.gameObject.SetActive(true);
    }

    public async void RespawnEnemies(int _enemyModel)
    {
        await Task.Delay(40000);
        ObjectTurnOn(_enemyModel);
    }

    [PunRPC]
    void ObjectTurnOn(int viewId)
    {
        PhotonView _pv = PhotonView.Find(viewId);
        _pv.gameObject.SetActive(true);
    }

    void Dash()
    {
        _localPlayer.DashForward();
    }

    void ColorChange()
    {

    }

    public async void WallReappear(GameObject _wall)
    {
        await Task.Delay(10000);
        _wall.gameObject.SetActive(true);
    }

    void MeleeMode()
    {
        _localPlayer.MeleeMode();
    }

    void SwordMode()
    {
        _localPlayer.SwordMode();
    }

    void DevilMode()
    {
        _localPlayer.DevilFruitMode();
    }

    void JumpClicked()
    {
        _localPlayer.Jump();
    }

    void MicOnOff()
    {
        if (_recorder.TransmitEnabled)
        {
            _recorder.TransmitEnabled = false;
            _mic.image.sprite = _micOff;
        }
        else
        {
            _recorder.TransmitEnabled = true;
            _mic.image.sprite = _micOn;
        }
    }

    void SliderValueChanged(float value)
    {
        foreach (GameObject speaker in GameObject.FindGameObjectsWithTag("Speaker"))
        {
            speaker.GetComponent<AudioSource>().volume = value;
        }
    }

    public override void OnMasterClientSwitched(Player newMasterClient)
    {
        Debug.Log("Master Client switched to: " + newMasterClient.NickName);
    }
}
