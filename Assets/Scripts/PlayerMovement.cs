using Photon.Pun;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.EventSystems;

public class PlayerMovement : MonoBehaviourPunCallbacks
{
    public Transform _orientation;
    [SerializeField] AudioClip _woodstep, _grassstep, _stonestep, _jumpSound;
    FixedJoystick _joyStick;
    public CinemachineOrbitalFollow _followCamera;
    public AudioSource _audioSource;
    Animator _animator;
    [SerializeField] LayerMask _ground;
    Rigidbody _rb;
    float _speed = 8f;
    Vector3 _inputDir;
    RaycastHit _hit;
    public AudioClip _footStepSound;
    bool _canJump = true;

    private void Awake()
    {
        _animator = GetComponent<Animator>();
        _audioSource = GetComponent<AudioSource>();
        _footStepSound = _stonestep;
        _rb = GetComponent<Rigidbody>();
    }

    private void Start()
    {
        _joyStick = FindFirstObjectByType<FixedJoystick>();
        if (photonView.IsMine)
        {
            _followCamera.gameObject.SetActive(true);
        }
    }

    void Update()
    {
        if (photonView.IsMine)
        {
            _orientation.forward = transform.position - new Vector3(_followCamera.transform.position.x, transform.position.y, _followCamera.transform.position.z);
            GroundCheck();
            TouchControl();
            JoyStickControl();
        }
    }

    void GroundCheck()
    {
        if (Physics.Raycast(_orientation.position, Vector3.down, out _hit, 0.2f))
        {
            if (_canJump)
            {
                if (_hit.collider.gameObject.layer == 7)
                {
                    if (_footStepSound != _woodstep)
                    {
                        _audioSource.Stop();
                        _footStepSound = _woodstep;
                    }
                }
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
            transform.forward = Vector3.Lerp(transform.forward, _inputDir.normalized, 6f * Time.deltaTime);
            _animator.SetBool("Run", true);
        }
        else
        {
            if (_canJump)
            {
                _audioSource.Stop();
                _animator.SetBool("Run", false);
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
                _audioSource.PlayOneShot(_jumpSound);
                _rb.AddForce(Vector3.up * _speed * 25f * Time.deltaTime, ForceMode.Impulse);
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
}
