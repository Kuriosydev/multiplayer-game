using Photon.Pun;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;
using static UnityEngine.ParticleSystem;

public class Enemy : MonoBehaviourPunCallbacks
{
    public NavMeshAgent _agent;
    PlayerMovement _player;
    Transform _target;
    public Vector3 _startPos;
    [SerializeField] int _range, _maxHealth, _attackDamage, _healthRegernation, _expIncrement;
    [SerializeField] float _damageFactor, _currentHealth;
    [SerializeField] AudioClip _run, _attack, _death;//gotHit
    public Image _healthbar;
    Animator _animator;
    public float _stoppingDistance = 1.5f;
    bool _canHit = true;
    int _hitCounter = 0;
    AudioSource _audioSource;
    [SerializeField] GameObject _deathEffect, _particle;

    void Start()
    {
        //reference of components
        transform.position = _startPos;
        photonView.RPC("UpdateStartPos", RpcTarget.AllBuffered, _startPos);
        _currentHealth = _maxHealth;
        UpdateHealth(_currentHealth);
        _agent = GetComponent<NavMeshAgent>();
        _animator = GetComponent<Animator>();
        _animator.SetBool("Death", false);
        _audioSource = GetComponent<AudioSource>();
        _agent.stoppingDistance = _stoppingDistance;
    }

    [PunRPC]
    void UpdateStartPos(Vector3 _startPosition)
    {
        //sync the start pos
        _startPos = _startPosition;
    }

    void Update()
    {
        //hp and other things update
        if(_startPos == Vector3.zero)
        {
            _startPos = transform.position;
        }
        _healthbar.fillAmount = _currentHealth / _maxHealth;
        if (_currentHealth < _maxHealth && _currentHealth > 0)
        {
            _currentHealth += _healthRegernation * Time.deltaTime;
            UpdateHealth(_currentHealth);
        }
        if (_currentHealth > 50)
        {
            _healthbar.color = Color.green;
        }
        else
        {
            _healthbar.color = Color.red;
        }
        if (_currentHealth > 0)
        {
            if (_target != null)
            {
                if (_target.gameObject.GetComponent<PlayerMovement>()._currentHealth > 0)
                {
                    _agent.SetDestination(_target.position);
                    if (Vector3.Distance(transform.position, _target.position) <= _agent.stoppingDistance)
                    {
                        _agent.velocity = Vector3.zero;
                        _animator.SetBool("Run", false);
                        _animator.SetBool("Attack", true);
                        Attack();
                    }
                    else
                    {
                        _animator.SetBool("Run", true);
                        _animator.SetBool("Attack", false);
                        if (!_audioSource.isPlaying)
                        {
                            _audioSource.PlayOneShot(_run);
                        }
                    }
                }
                else
                {
                    _target = null;
                }
                if (_target != null)
                {
                    if (Vector3.Distance(transform.position, _target.position) > _range && _currentHealth >= _maxHealth)
                    {
                        _target = null;
                        _animator.SetBool("Attack", false);
                    }
                }
            }
            else
            {
                foreach (PlayerMovement player in FindObjectsByType<PlayerMovement>(FindObjectsSortMode.None))
                {
                    if (Vector3.Distance(transform.position, player.gameObject.transform.position) <= _range)
                    {
                        if (_target != null)
                        {
                            if (Vector3.Distance(transform.position, player.gameObject.transform.position) < Vector3.Distance(transform.position, _target.position))
                            {
                                _target = player.gameObject.transform;
                            }
                        }
                        else
                        {
                            _target = player.gameObject.transform;
                        }
                    }
                }
                _agent.SetDestination(_startPos);
                if (Vector3.Distance(transform.position, _startPos) <= _agent.stoppingDistance)
                {
                    _agent.velocity = Vector3.zero;
                    _animator.SetBool("Run", false);
                    _audioSource.Stop();
                }
                else
                {
                    _animator.SetBool("Run", true);
                    if (!_audioSource.isPlaying)
                    {
                        _audioSource.PlayOneShot(_run);
                    }
                }
            }
        }
        else
        {
            if (!_animator.GetBool("Death"))
            {
                _audioSource.Stop();
                _particle = Instantiate(_deathEffect, transform.position, Quaternion.identity);
                _animator.SetBool("Death", true);
                _audioSource.PlayOneShot(_death);
            }
        }
    }

    public void AfterDeath()
    {
        //after death
        UpdateHealth(_maxHealth);
        //FindAnyObjectByType<UiManager>()._localPlayer.GetComponent<PlayerMovement>().ExpIncrease(_expIncrement);
        if (_target != null)
        {
            if (_target.CompareTag("Player"))
            {
                //DestroyImmediate(_particle, true);
                ObjectTurnOff(photonView.ViewID);
                if (FindAnyObjectByType<UiManager>()._questAccepted)
                {
                    if (FindAnyObjectByType<UiManager>()._questType == 1)
                    {
                        FindAnyObjectByType<UiManager>()._kills += 1;
                        FindAnyObjectByType<UiManager>().QuestCompleteCheck();
                    }
                    else
                    {
                        if (gameObject.transform.localScale.x == 3)
                        {
                            FindAnyObjectByType<UiManager>()._kills += 1;
                            FindAnyObjectByType<UiManager>().QuestCompleteCheck();
                        }
                    }
                }
                _target.GetComponent<PlayerMovement>().ExpIncrease(_expIncrement);
                _target = null;

            }
        }
        FindAnyObjectByType<UiManager>().RespawnEnemies(photonView.ViewID);
        ObjectTurnOff(photonView.ViewID);
    }

    [PunRPC]
    void ObjectTurnOff(int viewId)
    {
        //object turn off
        PhotonView _pv = PhotonView.Find(viewId);
        _pv.gameObject.SetActive(false);
    }

    private void FixedUpdate()
    {
        //_enemy face target
        if (_target != null)
        {
            faceTarget(_target.position);
        }
        else
        {
            faceTarget(_startPos);
        }
    }

    public void Attack()
    {
        //when enemy attack start
        if (_canHit)
        {
            _target.gameObject.GetComponent<PlayerMovement>().TakeDamage(_attackDamage);
            _animator.SetInteger("StrikeNumber", _hitCounter);
            _canHit = false;
            _audioSource.Stop();
            _audioSource.PlayOneShot(_attack);
        }
    }

    public void AttackDone()
    {
        //when enemy attack finish
        _hitCounter += 1;
        if (_hitCounter >= 4)
        {
            _hitCounter = 0;
        }
        _animator.SetBool("Attack", false);
        _canHit = true;
    }

    public void TakeDamage(int damage, GameObject _theplayer)
    {
        // when enemy take damage
        if (_currentHealth > 0)
        {
            //_agent.velocity = Vector3.zero;
            _target = _theplayer.transform;
            _currentHealth -= damage;// * _damageFactor;
            photonView.RPC("UpdateHealth", RpcTarget.AllBuffered, _currentHealth);
            if (transform.localScale != new Vector3(3, 3, 3))
            {
                _animator.SetBool("GotHit", true);
            }
            //_audioSource.Stop();
            //_audioSource.PlayOneShot(_gothit);
        }
    }

    public void GotHitReset()
    {
        //after gothit reset
        _animator.SetBool("GotHit", false);
    }

    [PunRPC]
    void UpdateHealth(float newHealth)
    {
        //health update
        _currentHealth = newHealth;
    }

    void faceTarget(Vector3 _lookAt)
    {
        //faces target
        Vector3 dir = (_lookAt - transform.position).normalized;
        Quaternion lookRotation = Quaternion.LookRotation(new Vector3(dir.x, transform.position.y, dir.z));
        transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, 5f * Time.deltaTime);
    }
}
