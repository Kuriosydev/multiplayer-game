using UnityEngine;
using Photon.Pun;
using Photon.Voice.Unity;
using TMPro;
using UnityEngine.UI;

public class UiManager : MonoBehaviourPunCallbacks
{
    [SerializeField] Button _jump, _combatMode, _walkRun, _mic;
    [SerializeField] GameObject _player;
    PlayerMovement _localPlayer;
    public int _reSpawn = 1;
    [SerializeField] Recorder _recorder;
    [SerializeField] TMP_Text _text;
    [SerializeField] Slider _slider;

    private void Start()
    {
        _slider.onValueChanged.AddListener(SliderValueChanged);
        _jump.onClick.AddListener(JumpClicked);
        _combatMode.onClick.AddListener(CombatMode);
        _walkRun.onClick.AddListener(WalkRunSwitch);
        _mic.onClick.AddListener(MicOnOff);
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

    void WalkRunSwitch()
    {
        _localPlayer.RunAndWalkSwitch();
    }

    void CombatMode()
    {
        _localPlayer.CombatModeOnOff();
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
