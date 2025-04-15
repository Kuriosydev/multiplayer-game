using Photon.Pun;
using Photon.Voice.Unity;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UiManager : MonoBehaviourPunCallbacks
{
    [SerializeField] Button _jump, _dash, _mic, _normalMode, _meleeMode, _swordMode;
    [SerializeField] GameObject _player, _guard, _chief, _trainee;
    PlayerMovement _localPlayer;
    public int _reSpawn = 1;
    [SerializeField] Recorder _recorder;
    [SerializeField] TMP_Text _text;
    [SerializeField] Slider _slider;

    private void Start()
    {
        _slider.onValueChanged.AddListener(SliderValueChanged);
        _jump.onClick.AddListener(JumpClicked);
        _normalMode.onClick.AddListener(NormalMode);
        _meleeMode.onClick.AddListener(MeleeMode);
        _swordMode.onClick.AddListener(SwordMode);
        _dash.onClick.AddListener(Dash);
        _mic.onClick.AddListener(MicOnOff);
        if (PhotonNetwork.IsMasterClient)
        {
            PhotonNetwork.InstantiateRoomObject(_guard.name, _guard.transform.position, Quaternion.identity);
            PhotonNetwork.InstantiateRoomObject(_chief.name, _chief.transform.position, Quaternion.identity);
            PhotonNetwork.InstantiateRoomObject(_trainee.name, _trainee.transform.position, Quaternion.identity);
        }
    }

    void Update()
    {
        if (!FindAnyObjectByType<PlayerMovement>() || !FindAnyObjectByType<PlayerMovement>().photonView.IsMine && _reSpawn > 0)
        {
            _reSpawn = 0;
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

    void Dash()
    {
        _localPlayer.DashForward();
    }

    void NormalMode()
    {
        _localPlayer.NormalsMode();
    }

    void MeleeMode()
    {
        _localPlayer.MeleeMode();
    }

    void SwordMode()
    {
        _localPlayer.SwordMode();
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
            _text.text = "Mic Off";
        }
        else
        {
            _recorder.TransmitEnabled = true;
            _text.text = "Mic On";
        }
    }

    void SliderValueChanged(float value)
    {
        foreach (GameObject speaker in GameObject.FindGameObjectsWithTag("Speaker"))
        {
            speaker.GetComponent<AudioSource>().volume = value;
        }
    }


}
