using UnityEngine;
using UnityEngine.EventSystems;
using Unity.Cinemachine;
using System.Collections;
using UnityEngine.UI;

public class PlayerTutorial : MonoBehaviour
{
    [SerializeField] 
    Transform _orientation;
    [SerializeField]
    AudioSource _audioSource;
    [SerializeField]
    Animator _animator;
    [SerializeField]
    Rigidbody _rb;
    [SerializeField]
    FixedJoystick _joyStick;
    [SerializeField] 
    CinemachineOrbitalFollow _followCamera;
    [SerializeField] 
    LayerMask _ground;
    [SerializeField]
    GameObject _jumpBtn, _swordBtn,_dashBtn,_quizPanel,_swordClick,_swordPanel;

    [SerializeField]
    GameObject _swordRight, _swordLeft;

    [SerializeField] 
    AudioClip _walkSound, _jumpSound, _doubleJumpSound, _runSound, _swordSound, _dashSound;

    public int _jumpForce = 200, _verticalUp = 45, _verticalDown = 10, _sensitivity = 10,_dashForce = 500;
    float _speed = 15f, _rotationValue = 6f;
    public float _walkSpeed;
    Vector3 _inputDir;
    RaycastHit _hit;
    int _state = 0, _hitCounter = 0, _jumpCount = 0;
    bool _canJump = true, _run = true, _attack = false, _combatMode = false, _canHit = true, _soundPlaying = false, _canMove = true;

    private void Awake()
    {
        _rb.freezeRotation = true;
        StartCoroutine(Joystick());
    }


    // Coroutine to Active Joystick
    IEnumerator Joystick()
    {
        yield return new WaitForSeconds(2f);
        _joyStick.gameObject.SetActive(true);
    }

    //touch ckick player attack
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

    //joystickcontrol
    void JoyStickControl()
    {
            _inputDir = _orientation.forward * _joyStick.Vertical + _orientation.right * _joyStick.Horizontal;
            if (_inputDir.magnitude != 0)
            {
                _joyStick.GetComponent<Animator>().enabled = false;
                _joyStick.transform.GetChild(1).gameObject.SetActive(false);

                if(_canMove)
                StartCoroutine(JumpControl());

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
                if (_jumpCount == 0 && !_soundPlaying)
                {
                    _audioSource.Stop();
                }
                _animator.SetBool("Run", false);
                _animator.SetBool("Walk", false);

                //JumpControl();
            }
    }

    // jumpcontrol
    IEnumerator JumpControl()
    {
        _canMove = false;
       yield return new WaitForSeconds(5f);
        _inputDir = Vector3.zero;
        _joyStick.gameObject.SetActive(false);
       // _rb.isKinematic = true;
        _animator.SetBool("Run", false);
        _animator.SetBool("Walk", false);
        _jumpBtn.SetActive(true);
    }

    //dash control
    IEnumerator DashControl()
    {
        _jumpBtn.GetComponent<Animator>().enabled = false;
        _jumpBtn.GetComponent<Button>().interactable = false;
        yield return new WaitForSeconds(2f);
        _jumpBtn.SetActive(false);
        _dashBtn.SetActive(true);
    }

    //sword control
    IEnumerator SwordControl()
    {
        _dashBtn.GetComponent<Animator>().enabled = false;
        _dashBtn.GetComponent<Button>().interactable = false;
        yield return new WaitForSeconds(2f);
        _dashBtn.SetActive(false);
        _swordBtn.SetActive(true);
    }

    //ground check
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

    //jump reset
    public void JumpReset()
    {
        _soundPlaying = false;
        _jumpCount = 0;
        _animator.SetBool("JumpCheck", false);
        _animator.SetInteger("Jump", 0);
    }

    //normal updates
    void Update()
    {
        
      if (!_animator.GetBool("JumpCheck") && _animator.GetInteger("Jump") > 0)
      {
         GroundCheck();
      }
            
       _orientation.forward = transform.position - new Vector3(_followCamera.transform.position.x, transform.position.y, _followCamera.transform.position.z);
      if (_combatMode)
      {
         TouchControl();
         TouchCliked();
      }
      else
      {
         TouchControl();
      }

      if(_joyStick.gameObject.activeSelf == true)
         JoyStickControl();
    }

    //Dash
    public void DashForward()
    {
        _soundPlaying = true;
        _audioSource.Stop();
        _audioSource.PlayOneShot(_dashSound);
        _animator.SetBool("DashForward", true);
        //_rb.AddForce(transform.forward * _speed * _dashForce * Time.deltaTime, ForceMode.Impulse);

        StartCoroutine(SwordControl());
    }

    //dash force
    public void DashForce()
    {
        _rb.AddForce(transform.forward * _speed * _dashForce * Time.deltaTime, ForceMode.Impulse);
    }

    //dash reset
    public void DashForwardReset()
    {
        _soundPlaying = false;
        //_dashEffect.SetActive(false);
        _animator.SetBool("DashForward", false);
    }

    //jump
    public void Jump()
    {
            if (Physics.Raycast(_orientation.position, Vector3.down, 0.2f, _ground))
            {
                _animator.SetBool("JumpCheck", true);
                //_soundPlaying = true;
                //_audioSource.Stop();
               // _audioSource.PlayOneShot(_jumpSound);
                _jumpCount = 1;
                _animator.SetInteger("Jump", _jumpCount);
               // _rb.AddForce(Vector3.up * _speed * _jumpForce * Time.deltaTime, ForceMode.Impulse);
                _canJump = false;
            }
            else if (_jumpCount == 1)
            {
                //_soundPlaying = true;
                //_audioSource.Stop();
                //_audioSource.PlayOneShot(_doubleJumpSound);
                _jumpCount = 2;
                _animator.SetInteger("Jump", _jumpCount);
               // _rb.AddForce(Vector3.up * _speed * _jumpForce * Time.deltaTime, ForceMode.Impulse);
            }
            else
            {
                _jumpCount = 0;
            }

        StartCoroutine(DashControl());
    }

    //jump force
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
            _soundPlaying = true;
            _audioSource.Stop();
            _audioSource.PlayOneShot(_doubleJumpSound);
        }
    }

    //sword mode
    public void SwordMode()
    {
            this.transform.localPosition = new Vector3(40,58,29);
            this.transform.localRotation = Quaternion.identity;
            _state = 2;
            _animator.SetBool("Strike", true);
            _hitCounter = 0;
            _animator.SetInteger("StrikeNumber", _hitCounter);
            _combatMode = true;
            _quizPanel.SetActive(false);
            _swordLeft.SetActive(true);
            _swordRight.SetActive(true);
            _swordClick.SetActive(true);
            _swordPanel.SetActive(true);
            StartCoroutine(SwordAcquired());
    }

    //sword acquired
    IEnumerator SwordAcquired()
    {
        yield return new WaitForSeconds(2f);
        _swordPanel.SetActive(false);
    }

    //when quiz panel appear
    public void Quiz()
    {
        _swordBtn.SetActive(false);
        _quizPanel.SetActive(true);
    }

    // Attack and damage taken section
    private void OnTriggerStay(Collider other)
    {
        if (other.gameObject.CompareTag("Enemy"))
        {
            if (_attack)
            {
                if (other.gameObject.GetComponent<PracticeDummy>())
                {
                    other.gameObject.GetComponent<PracticeDummy>().TakeDamage(10);
                }
                _attack = false;
            }
        }
    }

    //stike count up
    void StrikeCounterUp(int value)
    {
        _animator.SetInteger("StrikeNumber", value);
    }

    //when player attack
    public void Attack()
    {
            _attack = true;
            _soundPlaying = true;
            _audioSource.Stop();
            _audioSource.PlayOneShot(_swordSound);
    }

    //when attack done
    public void AttackDone()
    {
        _hitCounter += 1;
        if (_hitCounter >= 4)
        {
            _hitCounter = 0;
        }
        _soundPlaying = false;
        _animator.SetBool("Sword", false);
        _canHit = true;
    }

    //when touch clicked attack happens
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
                        if (_canHit)
                        {
                            _canHit = false;
                            StrikeCounterUp(_hitCounter);
                            if (_state == 1)
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
}
