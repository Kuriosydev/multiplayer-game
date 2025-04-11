using Photon.Pun;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class PlayerMovement : MonoBehaviourPunCallbacks
{
    [SerializeField] Transform _orientation;
    [SerializeField] GameObject _leftHand , _rightHand, _dashEffect, _knife1, _knife2;
    public Image _healthbar;
    [SerializeField] AudioClip _punchSound, _walkSound, _jumpSound, _doubleJumpSound, _runSound, _swordSound, _dashSound;
    FixedJoystick _joyStick;
    [SerializeField] CinemachineOrbitalFollow _followCamera;
    [SerializeField] CinemachineThirdPersonFollow _fixedCamera;
    public AudioSource _audioSource;
    Animator _animator;
    [SerializeField] LayerMask _ground;
    Rigidbody _rb;
    public int _currentHealth, _maxHealth = 100, _jumpForce = 200, _dashForce = 500;
    float _speed = 15f, _rotationValue = 6f, _turnOffStrike = 1f;
    public float _walkSpeed;
    Vector3 _inputDir;
    RaycastHit _hit;
    int _state = 0, _hitCounter = 0, _jumpCount = 0;
    bool _canJump = true, _run = true, _attack = false, _combatMode = false, _canHit = true, _soundPlaying =false;

    private void Awake()
    {
        _currentHealth = _maxHealth;
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
        _joyStick = FindFirstObjectByType<FixedJoystick>();
        if (photonView.IsMine)
        {
            _followCamera.gameObject.SetActive(true);
            _healthbar.gameObject.SetActive(false);
        }
    }

    void Update()
    {
        _healthbar.fillAmount = _currentHealth / 100f;
        if (photonView.IsMine)
        {
            //GroundCheck();
            if (_currentHealth > 0)
            {
                if (_combatMode)
                {
                    _orientation.forward = transform.position - new Vector3(_fixedCamera.transform.position.x, transform.position.y, _fixedCamera.transform.position.z);
                    TouchCliked();
                }
                else
                {
                    _orientation.forward = transform.position - new Vector3(_followCamera.transform.position.x, transform.position.y, _followCamera.transform.position.z);
                    TouchControl();
                }
                JoyStickControl();
            }
            else
            {
                _animator.SetBool("Death", true);
            }
            
        }
        else
        {
            HealthBarRotation(_healthbar.gameObject.transform);
        }
    }

    // function which turn on off camera for fighting and normal mode
    //public void CombatModeOnOff()
    //{
    //    if (photonView.IsMine)
    //    {
    //        if (_followCamera.gameObject.activeSelf)
    //        {
    //            _followCamera.gameObject.SetActive(false);
    //            _fixedCamera.gameObject.SetActive(true);
    //            _combatMode = true;
    //            _rotationValue = 5f;
    //        }
    //        else
    //        {
    //            _followCamera.gameObject.SetActive(true);
    //            _fixedCamera.gameObject.SetActive(false);
    //            _combatMode = false;
    //            _rotationValue = 6f;
    //        }
    //    }
    //}

    public void AfterDeath()
    {
        FindFirstObjectByType<UiManager>()._reSpawn = 1;
        PhotonNetwork.Destroy(gameObject);
    }

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
            _followCamera.gameObject.SetActive(true);
            _fixedCamera.gameObject.SetActive(false);
            _combatMode = false;
            _rotationValue = 6f;
        }
    }

    public void MeleeMode()
    {
        if (photonView.IsMine)
        {
            _state = 1;
            photonView.RPC("ObjectTurnOn", RpcTarget.AllBuffered, _leftHand.GetComponent<PhotonView>().ViewID);
            photonView.RPC("ObjectTurnOn", RpcTarget.AllBuffered, _rightHand.GetComponent<PhotonView>().ViewID);
            photonView.RPC("ObjectTurnOff", RpcTarget.AllBuffered, _knife1.GetComponent<PhotonView>().ViewID);
            photonView.RPC("ObjectTurnOff", RpcTarget.AllBuffered, _knife2.GetComponent<PhotonView>().ViewID);
            _animator.SetBool("Strike",true);
            _hitCounter = 0;
            _followCamera.gameObject.SetActive(false);
            _fixedCamera.gameObject.SetActive(true);
            _combatMode = true;
            _rotationValue = 5f;
        }
    }

    public void SwordMode()
    {
        if (photonView.IsMine)
        {
            _state = 2;
            photonView.RPC("ObjectTurnOff", RpcTarget.AllBuffered, _leftHand.GetComponent<PhotonView>().ViewID);
            photonView.RPC("ObjectTurnOff", RpcTarget.AllBuffered, _rightHand.GetComponent<PhotonView>().ViewID);
            photonView.RPC("ObjectTurnOn", RpcTarget.AllBuffered, _knife1.GetComponent<PhotonView>().ViewID);
            photonView.RPC("ObjectTurnOn", RpcTarget.AllBuffered, _knife2.GetComponent<PhotonView>().ViewID);
            _animator.SetBool("Strike", true);
            _hitCounter = 0;
            _followCamera.gameObject.SetActive(false);
            _fixedCamera.gameObject.SetActive(true);
            _combatMode = true;
            _rotationValue = 5f;
        }
    }

    void StrikeCounterUp(int value)
    {
        _animator.SetInteger("StrikeNumber", value);
    }

    private void OnTriggerStay(Collider other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            if (_attack)
            {
                Debug.Log("called");
                other.gameObject.GetComponent<PlayerMovement>().TakeDamage(10);
                _attack = false;
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
                            _attack = true;
                            _canHit = false;
                            StrikeCounterUp(_hitCounter);
                            _soundPlaying = true;
                            _audioSource.Stop();
                            if (_state == 1)
                            {
                                _animator.SetBool("Melee",true);
                                _audioSource.PlayOneShot(_punchSound);
                            }
                            else if (_state == 2)
                            {
                                _animator.SetBool("Sword", true);
                                _audioSource.PlayOneShot(_swordSound);
                            }
                        }
                    }
                    else if (touch.phase == TouchPhase.Ended)
                    {
                        _attack = false;
                    }
                }
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
        _soundPlaying = false;
        _animator.SetBool("Melee", false);
        _animator.SetBool("Sword", false);
        _canHit = true;
    }

    public void RunAndWalkSwitch()
    {
        if (_run)
        {
            _run = false;
            _speed = 12f;
            _animator.SetBool("Run", false);
        }
        else
        {
            _run = true;
            _speed = 15f;
            _animator.SetBool("Walk", false);
        }
    }

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
                        _followCamera.HorizontalAxis.Value += touch.deltaPosition.x * 10f * Time.deltaTime;
                        if (_followCamera.VerticalAxis.Value < 10f && _followCamera.VerticalAxis.Value > 45)
                        {
                            _followCamera.VerticalAxis.Value -= touch.deltaPosition.y * 10f * Time.deltaTime;
                        }
                        else if (_followCamera.VerticalAxis.Value >= 10f && Mathf.Clamp(Mathf.Atan(touch.deltaPosition.y), -10f, 45f) > 0)
                        {
                            _followCamera.VerticalAxis.Value -= touch.deltaPosition.y * 10f * Time.deltaTime;
                        }
                        else if (_followCamera.VerticalAxis.Value <= 45f && Mathf.Clamp(Mathf.Atan(touch.deltaPosition.y), -10f, 45f) < 0f)
                        {
                            _followCamera.VerticalAxis.Value -= touch.deltaPosition.y * 10f * Time.deltaTime;
                        }
                    }
                }
            }
        }
    }

    void JoyStickControl()
    {
        if (_currentHealth > 0) {
            _inputDir = _orientation.forward * _joyStick.Vertical + _orientation.right * _joyStick.Horizontal;
            if (_inputDir.magnitude != 0)
            {
                _rb.AddForce(_inputDir.normalized * _speed * _walkSpeed * Time.deltaTime, ForceMode.Force);
                transform.forward = Vector3.Lerp(transform.forward, _inputDir.normalized, _rotationValue * Time.deltaTime);
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
            else
            {
                if(_jumpCount == 0 && !_soundPlaying)
                {
                    _audioSource.Stop();
                }
                _animator.SetBool("Run", false);
                _animator.SetBool("Walk", false);
            }
        }
    }

    public void DashForward()
    {
        _soundPlaying = true;
        _audioSource.Stop();
        _audioSource.PlayOneShot(_dashSound);
        _animator.SetBool("DashForward", true);
        photonView.RPC("ObjectTurnOn", RpcTarget.AllBuffered, _dashEffect.GetComponent<PhotonView>().ViewID);
        _rb.AddForce(transform.forward * _speed * _dashForce * Time.deltaTime, ForceMode.Impulse);
    }

    public void GroundCheck()
    {
        if (Physics.Raycast(_orientation.position, Vector3.down, 0.2f))
        {
            JumpReset();
            _canJump = true;
            _animator.SetBool("JumpCheck", false);
        }
        else
        {
            _animator.SetBool("JumpCheck", false);
        }
    }

    public void Jump()
    {
        if (_currentHealth > 0)
        {
            if (Physics.Raycast(_orientation.position, Vector3.down, 0.2f, _ground))
            {
                _animator.SetBool("JumpCheck", true);
                _soundPlaying = true;
                _audioSource.Stop();
                _audioSource.PlayOneShot(_jumpSound);
                _jumpCount = 1;
                _animator.SetInteger("Jump", _jumpCount);
                _rb.AddForce(Vector3.up * _speed * _jumpForce * Time.deltaTime, ForceMode.Impulse);
                _canJump = false;
            }
            else if (_jumpCount == 1)
            {
                _soundPlaying = true;
                _audioSource.Stop();
                _audioSource.PlayOneShot(_doubleJumpSound);
                _jumpCount = 2;
                _animator.SetInteger("Jump", _jumpCount);
                _rb.AddForce(Vector3.up * _speed * _jumpForce * Time.deltaTime, ForceMode.Impulse);
            }
            else
            {
                _jumpCount = 0;
            }
        }
    }

    public void JumpReset()
    {
        _soundPlaying = false;
        _jumpCount = 0;
        _animator.SetBool("JumpCheck", false);
        _animator.SetInteger("Jump", 0);
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
    void UpdateHealth(int newHealth)
    {
        _currentHealth = newHealth;
    }

    void HealthBarRotation(Transform _bar)
    {
        Vector3 _dir = (transform.position - _bar.transform.position).normalized;
        Quaternion lookRotation = Quaternion.LookRotation(new Vector3(_bar.transform.position.x, _dir.y, _dir.z));
        _healthbar.transform.rotation = Quaternion.Slerp(_bar.transform.rotation, lookRotation, 8f * Time.deltaTime);
    }

    public void DashForwardReset()
    {
        _soundPlaying = false;
        photonView.RPC("ObjectTurnOff", RpcTarget.AllBuffered, _dashEffect.GetComponent<PhotonView>().ViewID);
        _dashEffect.SetActive(false);
        _animator.SetBool("DashForward", false);
    }

    void DeathAnimation()
    {
        _animator.SetBool("Death", true);
        _dashEffect.SetActive(false);
    }


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
