using Photon.Pun;
using System.Collections.Generic;
using TMPro;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class PlayerMovement : MonoBehaviourPunCallbacks
{
    [SerializeField]
    GameObject[] _parts;
    [SerializeField] Transform _orientation;
    List<int> scores = new List<int>();
    [SerializeField] GameObject _leftHand, _rightHand, _dashEffect, _knife1, _knife2, _spawnLocation, _devilFruit1, _devilFruit2;
    public Image _healthbar, _starColor;
    [SerializeField] TMP_Text _playerName, _playerRank;
    [SerializeField] AudioClip _punchSound, _walkSound, _jumpSound, _doubleJumpSound, _runSound, _swordSound, _dashSound, _swimSound, _gainExp;
    FixedJoystick _joyStick;
    [SerializeField] CinemachineOrbitalFollow _followCamera;
    public AudioSource _audioSource;
    Animator _animator;
    [SerializeField] LayerMask _ground;
    Rigidbody _rb;
    string _numberOneRank = "FFC000", _numberTwoRank = "D8D9CD", _normalRankColor = "A0480C";
    public int _maxHealth = 100, _maxEnergy = 100, _runForce = 6000,
        _jumpForce = 200, _dashForce = 500, _verticalUp = 45, _verticalDown = 10,
        _sensitivity = 10, _damage = 10, _healthRegain = 5, _healthAdd = 25, _attackAdd = 10,
        _energyRegain = 5, _energyDeduction = 15, _exp = 100, _level = 1, _expRequired, _rank = 1;
    public float _currentHealth, _defence, _currentEnergy, _timer;
    float _speed = 15f, _rotationValue = 6f;
    float _defenceAdd;
    Vector3 _inputDir;
    RaycastHit _hit;
    int _hitCounter = 0, _jumpCount = 0;
    public int _state = 0, _devilFruit = 0, _money, _score;
    bool _canJump = true, _run = true, _attack = false, _combatMode = false, _canHit = true, _soundPlaying = false, _canDash = true;

    private void Awake()
    {
        _maxHealth = _maxHealth + (_level * _healthAdd);
        _damage = _damage + (_level * _attackAdd);
        _maxEnergy = _maxHealth;
        _expRequired = 2 * _exp;
        _defence = _maxHealth / 2;
        _currentHealth = _maxHealth;
        _currentEnergy = _maxEnergy;
        _defenceAdd = _healthAdd / 2;
        _animator = GetComponent<Animator>();
        _audioSource = GetComponent<AudioSource>();
        _rb = GetComponent<Rigidbody>();
        _rb.freezeRotation = true;
        photonView.RPC("ObjectTurnOff", RpcTarget.AllBuffered, _dashEffect.GetComponent<PhotonView>().ViewID);
        photonView.RPC("ObjectTurnOff", RpcTarget.AllBuffered, _leftHand.GetComponent<PhotonView>().ViewID);
        photonView.RPC("ObjectTurnOff", RpcTarget.AllBuffered, _rightHand.GetComponent<PhotonView>().ViewID);
        photonView.RPC("ObjectTurnOff", RpcTarget.AllBuffered, _knife1.GetComponent<PhotonView>().ViewID);
        photonView.RPC("ObjectTurnOff", RpcTarget.AllBuffered, _knife2.GetComponent<PhotonView>().ViewID);
    }

    private void Start()
    {
        _playerName.text = photonView.Owner.NickName;
        _joyStick = FindFirstObjectByType<FixedJoystick>();
        if (photonView.IsMine)
        {
            _followCamera.gameObject.SetActive(true);
            _healthbar.gameObject.SetActive(false);
            GetComponent<TriggerEvents>().enabled = true;
            foreach (GameObject _part in _parts)
            {
                if (PlayerPrefs.GetInt("color") == 1)
                {
                    _part.GetComponent<Renderer>().material.color = Color.white;
                }
                else if (PlayerPrefs.GetInt("color") == 2)
                {
                    _part.GetComponent<Renderer>().material.color = Color.red;
                }
                else if (PlayerPrefs.GetInt("color") == 3)
                {
                    _part.GetComponent<Renderer>().material.color = Color.black;
                }
                else if (PlayerPrefs.GetInt("color") == 4)
                {
                    _part.GetComponent<Renderer>().material.color = Color.yellow;
                }
            }
        }
        else
        {
            GetComponent<TriggerEvents>().enabled = false;
        }
    }

    public void ScoreIncrease(int score)
    {
        _score += score;
        photonView.RPC("UpdateScore", RpcTarget.AllBuffered, _score);
    }

    public void LevelUp()
    {
        if (_exp >= _expRequired)
        {
            _level += 1;
            _expRequired = 2 * _expRequired;
        }
        _maxHealth = 100 + (_level * _healthAdd);
        _damage = 10 + (_level * _attackAdd);
        _maxEnergy = _maxHealth;
        _defence = _maxHealth / 2;
    }

    public void ExpIncrease(int _value)
    {
        _exp += _value;
        LevelUp();
        _soundPlaying = true;
        _audioSource.Stop();
        _audioSource.PlayOneShot(_gainExp);
    }

    void Update()
    {
        if (_state == 3)
        {
            if (_timer > 0)
            {
                _timer -= 1 * Time.deltaTime;
            }
            else
            {
                _timer = 0;
                _devilFruit = 0;
                NormalsMode();
            }
        }
        _healthbar.fillAmount = _currentHealth / _maxHealth;
        if (_currentHealth < _maxHealth && _currentHealth > 0)
        {
            _currentHealth += _healthRegain * Time.deltaTime;
            photonView.RPC("UpdateHealth", RpcTarget.AllBuffered, _currentHealth);
        }
        if (_currentEnergy < _maxEnergy)
        {
            _currentEnergy += _energyRegain * Time.deltaTime;
        }
        if (_currentHealth > 50)
        {
            _healthbar.color = Color.green;
        }
        else
        {
            _healthbar.color = Color.red;
        }
        if (photonView.IsMine)
        {
            if (!_animator.GetBool("JumpCheck") && _animator.GetInteger("Jump") > 0)
            {
                GroundCheck();
            }
            if (_currentHealth > 0)
            {
                _orientation.forward = transform.position - new Vector3(_followCamera.transform.position.x, transform.position.y, _followCamera.transform.position.z);
                if (_combatMode)
                {
                    TouchCliked();
                    TouchControl();
                }
                else
                {
                    TouchControl();
                }
                JoyStickControl();
            }
            else
            {
                _animator.SetBool("Death", true);
            }
        }
        foreach (PlayerMovement player in FindObjectsByType<PlayerMovement>(FindObjectsSortMode.None))
        {
            scores.Add(player._score);
        }
        scores.Sort();
        scores.Reverse();
        _rank = scores.IndexOf(_score) + 1;
        scores.Clear();
        _playerRank.text = _rank.ToString();
        if (_rank == 1)
        {
            if (ColorUtility.TryParseHtmlString(_numberOneRank, out Color thecolor))
            {
                _starColor.color = thecolor;
            }
        }
        else if (_rank == 2)
        {
            if (ColorUtility.TryParseHtmlString(_numberTwoRank, out Color thecolor))
            {
                _starColor.color = thecolor;
            }
        }
        else
        {
            if (ColorUtility.TryParseHtmlString(_normalRankColor, out Color thecolor))
            {
                _starColor.color = thecolor;
            }
        }
        HealthBarRotation();
    }

    public void AfterDeath()
    {
        if (photonView.IsMine)
        {
            //FindFirstObjectByType<UiManager>()._reSpawn = 1;
            _animator.SetBool("Death", false);
            transform.position = FindFirstObjectByType<UiManager>()._playerPosition[Random.Range(0, FindFirstObjectByType<UiManager>()._playerPosition.Length - 1)];
            _followCamera.gameObject.SetActive(false);
            ReplaceAfterDeath();
        }
    }

    public void ReplaceAfterDeath()
    {
        if (photonView.IsMine)
        {
            _currentHealth = _maxHealth;
            _currentEnergy = _maxEnergy;
            UpdateHealth(_currentHealth);
            _followCamera.gameObject.SetActive(true);
        }
    }

    void DeathAnimation()
    {
        _animator.SetBool("Death", true);
        _dashEffect.SetActive(false);
    }

    //Mode Switches;
    public void NormalsMode()
    {
        if (photonView.IsMine)
        {
            _state = 0;
            photonView.RPC("ObjectTurnOff", RpcTarget.AllBuffered, _leftHand.GetComponent<PhotonView>().ViewID);
            photonView.RPC("ObjectTurnOff", RpcTarget.AllBuffered, _rightHand.GetComponent<PhotonView>().ViewID);
            photonView.RPC("ObjectTurnOff", RpcTarget.AllBuffered, _knife1.GetComponent<PhotonView>().ViewID);
            photonView.RPC("ObjectTurnOff", RpcTarget.AllBuffered, _knife2.GetComponent<PhotonView>().ViewID);
            _animator.SetBool("Strike", false);
            _hitCounter = 0;
            _attackAdd = 20;
            _animator.SetInteger("StrikeNumber", _hitCounter);
            _combatMode = false;
            transform.localScale = new Vector3(1, 1, 1);
            _orientation.localPosition = new Vector3(0, 0.6f, 0);
            _followCamera.Radius = 10;
        }
    }

    public void MeleeMode()
    {
        if (photonView.IsMine)
        {
            if (_state != 1)
            {
                _state = 1;
                photonView.RPC("ObjectTurnOn", RpcTarget.AllBuffered, _leftHand.GetComponent<PhotonView>().ViewID);
                photonView.RPC("ObjectTurnOn", RpcTarget.AllBuffered, _rightHand.GetComponent<PhotonView>().ViewID);
                photonView.RPC("ObjectTurnOff", RpcTarget.AllBuffered, _knife1.GetComponent<PhotonView>().ViewID);
                photonView.RPC("ObjectTurnOff", RpcTarget.AllBuffered, _knife2.GetComponent<PhotonView>().ViewID);
                _animator.SetBool("Strike", true);
                _hitCounter = 0;
                _attackAdd = 20;
                _animator.SetInteger("StrikeNumber", _hitCounter);
                _combatMode = true;
                transform.localScale = new Vector3(1, 1, 1);
                _orientation.localPosition = new Vector3(0, 0.6f, 0);
                _followCamera.Radius = 10;
            }
            else
            {
                _state = 0;
                NormalsMode();
            }
        }
    }

    public void SwordMode()
    {
        if (photonView.IsMine)
        {
            if (_state != 2)
            {
                _state = 2;
                photonView.RPC("ObjectTurnOff", RpcTarget.AllBuffered, _leftHand.GetComponent<PhotonView>().ViewID);
                photonView.RPC("ObjectTurnOff", RpcTarget.AllBuffered, _rightHand.GetComponent<PhotonView>().ViewID);
                photonView.RPC("ObjectTurnOn", RpcTarget.AllBuffered, _knife1.GetComponent<PhotonView>().ViewID);
                photonView.RPC("ObjectTurnOn", RpcTarget.AllBuffered, _knife2.GetComponent<PhotonView>().ViewID);
                _animator.SetBool("Strike", true);
                _hitCounter = 0;
                _animator.SetInteger("StrikeNumber", _hitCounter);
                _combatMode = true;
                _attackAdd = 20;
                transform.localScale = new Vector3(1, 1, 1);
                _orientation.localPosition = new Vector3(0, 0.6f, 0);
                _followCamera.Radius = 10;
            }
            else
            {
                _state = 0;
                NormalsMode();
            }
        }
    }

    public void DevilFruitMode()
    {
        if (photonView.IsMine)
        {
            if (_state != 3)
            {
                _state = 3;
                photonView.RPC("ObjectTurnOff", RpcTarget.AllBuffered, _leftHand.GetComponent<PhotonView>().ViewID);
                photonView.RPC("ObjectTurnOff", RpcTarget.AllBuffered, _rightHand.GetComponent<PhotonView>().ViewID);
                photonView.RPC("ObjectTurnOff", RpcTarget.AllBuffered, _knife1.GetComponent<PhotonView>().ViewID);
                photonView.RPC("ObjectTurnOff", RpcTarget.AllBuffered, _knife2.GetComponent<PhotonView>().ViewID);
                _animator.SetBool("Strike", true);
                _hitCounter = 0;
                _animator.SetInteger("StrikeNumber", _hitCounter);
                _combatMode = true;
                if (_devilFruit == 3)
                {
                    transform.localScale = new Vector3(3, 3, 3);
                    _orientation.localPosition = new Vector3(0, 0.1f, 0);
                    _attackAdd = 40;
                    _followCamera.Radius = 20;
                    photonView.RPC("ObjectTurnOn", RpcTarget.AllBuffered, _leftHand.GetComponent<PhotonView>().ViewID);
                    photonView.RPC("ObjectTurnOn", RpcTarget.AllBuffered, _rightHand.GetComponent<PhotonView>().ViewID);
                }
                else
                {
                    _attackAdd = 20;
                    _followCamera.Radius = 10;
                    transform.localScale = new Vector3(1, 1, 1);
                    _orientation.localPosition = new Vector3(0, 0.6f, 0);
                }
            }
            else
            {
                _state = 0;
                NormalsMode();
            }
        }
    }

    // Attack and damage taken section
    private void OnTriggerStay(Collider other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            if (_attack)
            {
                if (_state != 3 || _devilFruit == 3)
                {
                    other.gameObject.GetComponent<PlayerMovement>().TakeDamage(_damage);
                }
                _attack = false;
            }
        }
        if (other.gameObject.CompareTag("Enemy"))
        {
            if (_devilFruit == 3)
            {
                if (other.gameObject.transform.localScale.y < 3)
                {
                    other.GetComponent<Enemy>()._agent.stoppingDistance = 2.5f;
                }
            }
            if (_attack)
            {
                if (_state != 3 || _devilFruit == 3)
                {
                    if (other.gameObject.GetComponent<Enemy>())
                    {
                        other.gameObject.GetComponent<Enemy>().TakeDamage(_damage, gameObject);
                    }
                }
                _attack = false;
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.CompareTag("Enemy"))
        {
            if (other.gameObject.transform.localScale.y < 3)
            {
                other.GetComponent<Enemy>()._agent.stoppingDistance = 1.5f;
            }
        }
    }
    public void TouchCliked()
    {
        if (Input.touchCount > 0 && !EventSystem.current.IsPointerOverGameObject() && _currentHealth > 0)
        {
            foreach (Touch touch in Input.touches)
            {
                if (!EventSystem.current.IsPointerOverGameObject(touch.fingerId))
                {
                    if (touch.phase == TouchPhase.Began)
                    {
                        if (_canHit)
                        {
                            _canHit = false;
                            StrikeCounterUp(_hitCounter);
                            if (_state == 1 || _state == 3)
                            {
                                _animator.SetBool("Melee", true);
                            }
                            else if (_state == 2)
                            {
                                _animator.SetBool("Sword", true);
                            }
                        }
                    }
                }
            }
        }
    }

    public void FruitPower(int _fruit)
    {
        if (photonView.IsMine)
        {
            if (_fruit == 1)
            {
                GameObject _fruitContainer = PhotonNetwork.Instantiate(_devilFruit1.name, _spawnLocation.transform.position, transform.rotation);
                _fruitContainer.GetComponent<ParticleHandler>().PlayerSetUp(gameObject);
            }
            else if (_fruit == 2)
            {
                GameObject _fruitContainer = PhotonNetwork.Instantiate(_devilFruit2.name, _spawnLocation.transform.position, transform.rotation);
                _fruitContainer.GetComponent<ParticleHandler>().PlayerSetUp(gameObject);
            }
        }
    }

    void StrikeCounterUp(int value)
    {
        _animator.SetInteger("StrikeNumber", value);
    }

    public void Attack()
    {
        if (photonView.IsMine)
        {
            _attack = true;
            _soundPlaying = true;
            _audioSource.Stop();
            if (_state == 1)
            {
                _audioSource.PlayOneShot(_punchSound);
            }
            else if (_state == 2)
            {
                _audioSource.PlayOneShot(_swordSound);
            }
            if (_state == 3)
            {
                FruitPower(_devilFruit);
            }
        }
    }

    public void AttackDone()
    {
        _hitCounter += 1;
        if (_hitCounter >= 4)
        {
            _hitCounter = 0;
        }
        _attack = false;
        _soundPlaying = false;
        _animator.SetBool("Melee", false);
        _animator.SetBool("Sword", false);
        _canHit = true;
    }

    public void TakeDamage(int damage)
    {
        if (_currentHealth > 0)
        {
            _currentHealth -= damage;
            photonView.RPC("UpdateHealth", RpcTarget.AllBuffered, _currentHealth);
        }
    }

    [PunRPC]
    void UpdateHealth(float newHealth)
    {
        if (photonView.IsMine)
        {
            _currentHealth = newHealth;
        }
    }

    [PunRPC]
    void UpdateScore(int score)
    {
        _score = score;
    }

    void ExpUpdate(int expUpdate)
    {
        if (photonView.IsMine)
        {
            _exp = expUpdate;
        }
    }

    //Land/Water switch;
    public void WaterSwitch()
    {
        _animator.SetBool("InWater", false);
        _animator.SetInteger("Swim", 0);
    }

    public void LandSwitch()
    {
        _animator.SetBool("InWater", true);
        _animator.SetInteger("Swim", 1);
    }

    //Camera and Joystick Section;
    void TouchControl()
    {
        if (Input.touchCount > 0 && !EventSystem.current.IsPointerOverGameObject())
        {
            foreach (Touch touch in Input.touches)
            {
                if (!EventSystem.current.IsPointerOverGameObject(touch.fingerId))
                {
                    if (touch.phase == TouchPhase.Began || touch.phase == TouchPhase.Moved)
                    {
                        _followCamera.HorizontalAxis.Value += touch.deltaPosition.x * _sensitivity * Time.deltaTime;
                        if (_followCamera.VerticalAxis.Value < _verticalDown && _followCamera.VerticalAxis.Value > _verticalUp)
                        {
                            _followCamera.VerticalAxis.Value -= touch.deltaPosition.y * _sensitivity * Time.deltaTime;
                        }
                        else if (_followCamera.VerticalAxis.Value >= _verticalDown && Mathf.Clamp(Mathf.Atan(touch.deltaPosition.y), -10f, 45f) > 0)
                        {
                            _followCamera.VerticalAxis.Value -= touch.deltaPosition.y * _sensitivity * Time.deltaTime;
                        }
                        else if (_followCamera.VerticalAxis.Value <= _verticalUp && Mathf.Clamp(Mathf.Atan(touch.deltaPosition.y), -10f, 45f) < 0f)
                        {
                            _followCamera.VerticalAxis.Value -= touch.deltaPosition.y * _sensitivity * Time.deltaTime;
                        }
                    }
                }
            }
        }
    }

    void JoyStickControl()
    {
        if (_currentHealth > 0)
        {
            _inputDir = _orientation.forward * _joyStick.Vertical + _orientation.right * _joyStick.Horizontal;
            if (_inputDir.magnitude != 0)
            {
                _rb.AddForce(_inputDir.normalized * _speed * _runForce * Time.deltaTime, ForceMode.Force);
                transform.forward = Vector3.Lerp(transform.forward, _inputDir.normalized, _rotationValue * Time.deltaTime);
                if (_animator.GetBool("InWater"))
                {
                    _animator.SetInteger("Swim", 2);
                    if (!_audioSource.isPlaying)
                    {
                        _audioSource.PlayOneShot(_swimSound);
                    }
                }
                else
                {
                    if (_run)
                    {
                        _animator.SetBool("Run", true);
                        if (!_audioSource.isPlaying)
                        {
                            _audioSource.PlayOneShot(_runSound);
                        }
                    }
                    else
                    {
                        _animator.SetBool("Walk", true);
                        if (!_audioSource.isPlaying)
                        {
                            _audioSource.PlayOneShot(_walkSound);
                        }
                    }
                }
            }
            else
            {
                if (_jumpCount == 0 && !_soundPlaying)
                {
                    _audioSource.Stop();
                }
                if (_animator.GetBool("InWater"))
                {
                    _animator.SetInteger("Swim", 1);
                }
                _animator.SetBool("Run", false);
                _animator.SetBool("Walk", false);
            }
        }
    }


    //Dash and Jump section
    public void DashForward()
    {
        if (photonView.IsMine)
        {
            if (_currentEnergy > _energyDeduction && _canDash)
            {
                _canDash = false;
                _animator.SetBool("DashForward", true);
                photonView.RPC("ObjectTurnOn", RpcTarget.AllBuffered, _dashEffect.GetComponent<PhotonView>().ViewID);
            }
        }
    }

    public void DashForce()
    {
        _soundPlaying = true;
        _audioSource.Stop();
        _audioSource.PlayOneShot(_dashSound);
        _rb.AddForce(transform.forward * _speed * _dashForce * Time.deltaTime, ForceMode.Impulse);
        _currentEnergy -= _energyDeduction;
    }

    public void DashForwardReset()
    {
        _soundPlaying = false;
        _canDash = true;
        photonView.RPC("ObjectTurnOff", RpcTarget.AllBuffered, _dashEffect.GetComponent<PhotonView>().ViewID);
        _dashEffect.SetActive(false);
        _animator.SetBool("DashForward", false);
    }

    public void Jump()
    {
        if (_currentHealth > 0)
        {
            if (Physics.Raycast(_orientation.position, Vector3.down, 0.2f, _ground))
            {
                _animator.SetBool("JumpCheck", true);
                _jumpCount = 1;
                _animator.SetInteger("Jump", _jumpCount);
                _canJump = false;
            }
            else if (_jumpCount == 1 && _currentEnergy > _energyDeduction)
            {
                _jumpCount = 2;
                _animator.SetInteger("Jump", _jumpCount);
            }
            else
            {
                _jumpCount = 0;
            }
        }
    }

    public void JumpForce()
    {
        _rb.AddForce(Vector3.up * _speed * _jumpForce * Time.deltaTime, ForceMode.Impulse);
        if (_jumpCount == 1)
        {
            _soundPlaying = true;
            _audioSource.Stop();
            _audioSource.PlayOneShot(_jumpSound);
        }
        else if (_jumpCount == 2)
        {
            _currentEnergy -= _energyDeduction;
            _soundPlaying = true;
            _audioSource.Stop();
            _audioSource.PlayOneShot(_doubleJumpSound);
            _currentEnergy -= _energyDeduction;
        }
    }

    public void GroundCheck()
    {
        if (Physics.Raycast(_orientation.position, Vector3.down, 0.2f))
        {
            JumpReset();
            _canJump = true;
        }
        else
        {
            _animator.SetBool("JumpCheck", false);
        }
    }

    public void JumpReset()
    {
        _soundPlaying = false;
        _jumpCount = 0;
        _animator.SetBool("JumpCheck", false);
        _animator.SetInteger("Jump", 0);
    }

    //HealthBarRotation  UnderProgress
    void HealthBarRotation()
    {
        if (photonView.IsMine)
        {
            Transform _camerapostion = transform;
            _camerapostion = _followCamera.transform;
            foreach (GameObject _bar in GameObject.FindGameObjectsWithTag("HealthBar"))
            {
                _bar.transform.rotation = _camerapostion.transform.rotation;
            }
        }
    }

    //Object on/off on network
    [PunRPC]
    void ObjectTurnOn(int viewId)
    {
        PhotonView _pv = PhotonView.Find(viewId);
        _pv.gameObject.SetActive(true);
    }

    [PunRPC]
    void ObjectTurnOff(int viewId)
    {
        PhotonView _pv = PhotonView.Find(viewId);
        _pv.gameObject.SetActive(false);
    }
}
