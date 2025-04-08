using Photon.Pun;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class PlayerMovement : MonoBehaviourPun, IPunObservable
{
    [SerializeField] Transform _orientation;
    public Image _healthbar;
    //[SerializeField] AudioClip _woodstep, _grassstep, _stonestep, _jumpSound;
    FixedJoystick _joyStick;
    [SerializeField] CinemachineOrbitalFollow _followCamera;
    [SerializeField] CinemachineThirdPersonFollow _fixedCamera;
    public AudioSource _audioSource;
    Animator _animator;
    [SerializeField] LayerMask _ground;
    Rigidbody _rb;
    public int _currentHealth, _maxHealth = 100;
    float _speed = 8f, _rotationValue = 6f;
    Vector3 _inputDir;
    RaycastHit _hit;
    public AudioClip _footStepSound;
    bool _canJump = true, _run = true, _attack = false, _combatMode = false;

    private void Awake()
    {
        _currentHealth = _maxHealth;
        _animator = GetComponent<Animator>();
        _audioSource = GetComponent<AudioSource>();
        //_footStepSound = _stonestep;
        _rb = GetComponent<Rigidbody>();
        _rb.freezeRotation = true;
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
        if (photonView.IsMine)
        {
            _healthbar.fillAmount = _currentHealth / 100;
            _orientation.forward = transform.position - new Vector3(_followCamera.transform.position.x, transform.position.y, _followCamera.transform.position.z);
            GroundCheck();
            if (_combatMode)
            {             
                TouchCliked();
            }
            else
            {
                TouchControl();
            }
            JoyStickControl();
            if(_currentHealth <= 0)
            {
                FindFirstObjectByType<UiManager>()._reSpawn = 1;
                Destroy(gameObject, 0.2f);
            }
        }
        else
        {
            HealthBarRotation(_healthbar.gameObject.transform);
        }
    }

    // function which turn on off camera for fighting and normal mode
    public void CombatModeOnOff()
    {
        if (photonView.IsMine)
        {
            if (_followCamera.gameObject.activeSelf)
            {
                _followCamera.gameObject.SetActive(false);
                _fixedCamera.gameObject.SetActive(true);
                _combatMode = true;
                _rotationValue = 5f;
            }
            else
            {
                _followCamera.gameObject.SetActive(true);
                _fixedCamera.gameObject.SetActive(false);
                _combatMode = false;
                _rotationValue = 6f;
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            if (_attack)
            {
                other.gameObject.GetComponent<PlayerMovement>().TakeDamage(-20);
                _attack = false;
            }
        }
    }

    private void OnTriggerStay(Collider other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            if (_attack)
            {
                Debug.Log("called");
                other.gameObject.GetComponent<PlayerMovement>().TakeDamage(-20);
                _attack = false;
            }
        }
    }

    public void TouchCliked()
    {
        if (Input.touchCount > 0 && !EventSystem.current.IsPointerOverGameObject())
        {
            foreach (Touch touch in Input.touches)
            {
                if (!EventSystem.current.IsPointerOverGameObject(touch.fingerId))
                {
                    if (touch.phase == TouchPhase.Began)
                    {
                        _attack = true;
                        _animator.SetBool("Melee",true);
                    }else if (touch.phase == TouchPhase.Moved)
                    {
                        _attack = true;
                    }
                    else if (touch.phase == TouchPhase.Ended)
                    {
                        _attack = false;
                        _animator.SetBool("Melee", false);
                    }
                }
            }
        }
    }

    public void RunAndWalkSwitch()
    {
        if (_run)
        {
            _run = false;
            _speed = 5f;
            _animator.SetBool("Run", false);
        }
        else
        {
            _run = true;
            _speed = 8f;
            _animator.SetBool("Walk", false);
        }
    }

    void GroundCheck()
    {
        if (Physics.Raycast(_orientation.position, Vector3.down, 0.2f))
        {
            if (_canJump)
            {
                //if (_hit.collider.gameObject.layer == 7)
                //{
                //    if (_footStepSound != _woodstep)
                //    {
                //        _audioSource.Stop();
                //        _footStepSound = _woodstep;
                //    }
                //}
            }
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
        _inputDir = _orientation.forward * _joyStick.Vertical + _orientation.right * _joyStick.Horizontal;
        if (_inputDir.magnitude != 0)
        {
            if (!_audioSource.isPlaying)
            {
                _audioSource.PlayOneShot(_footStepSound);
            }
            _rb.AddForce(_inputDir.normalized * _speed * 400f * Time.deltaTime, ForceMode.Force);
            transform.forward = Vector3.Lerp(transform.forward, _inputDir.normalized, _rotationValue * Time.deltaTime);
            if (_run)
            {
                _animator.SetBool("Run", true);
            }
            else
            {
                _animator.SetBool("Walk", true);
            }
        }
        else
        {
            if (_canJump)
            {
                _audioSource.Stop();
                _animator.SetBool("Run", false);
                _animator.SetBool("Walk", false);
            }
        }
    }


    public void Jump()
    {
        if (_canJump)
        {
            if (Physics.Raycast(_orientation.position, Vector3.down, 0.2f, _ground))
            {
                _audioSource.Stop();
                //_audioSource.PlayOneShot(_jumpSound);
                _rb.AddForce(Vector3.up * _speed * 50f * Time.deltaTime, ForceMode.Impulse);
                _animator.SetBool("Jump", _canJump);
                _canJump = false;
                Invoke("JumpReset", 0.8f);
            }
        }
    }


    public void JumpReset()
    {
        _animator.SetBool("Jump", _canJump);
        _canJump = true;
    }

    public void TakeDamage(int _damage)
    {
        if (!photonView.IsMine) return;

        _currentHealth -= _damage;
        //_currentHealth = Mathf.Max(_currentHealth, 0);
    }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            stream.SendNext(_currentHealth);
        }
        else
        {
            _currentHealth = (int)stream.ReceiveNext();
        }
    }

    void HealthBarRotation(Transform _bar)
    {
        Vector3 _dir = (transform.position - _bar.transform.position).normalized;
        Quaternion lookRotation = Quaternion.LookRotation(new Vector3(_bar.transform.position.x, _dir.y, _dir.z));
        _healthbar.transform.rotation = Quaternion.Slerp(_bar.transform.rotation, lookRotation, 8f * Time.deltaTime);
    }
}
