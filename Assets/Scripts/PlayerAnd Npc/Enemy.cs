using Photon.Pun;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;

public class Enemy : MonoBehaviourPunCallbacks
{
    public NavMeshAgent _agent;
    PlayerMovement _player;
    Transform _target;
    public Vector3 _startPos;
    [SerializeField] int _range, _maxHealth, _attackDamage, _healthRegernation, _expIncrement;
    [SerializeField] float _damageFactor, _currentHealth;
    public Image _healthbar;
    Animator _animator;
    public float _stoppingDistance = 1.5f;
    bool _canHit = true;
    int _hitCounter = 0;

    void Start()
    {
        transform.position = _startPos;
        photonView.RPC("UpdateStartPos", RpcTarget.AllBuffered, _startPos);
        _currentHealth = _maxHealth;
        UpdateHealth(_currentHealth);
        _agent = GetComponent<NavMeshAgent>();
        _animator = GetComponent<Animator>();
        _animator.SetBool("Death", false);
        _agent.stoppingDistance = _stoppingDistance;
    }

    [PunRPC]
    void UpdateStartPos(Vector3 _startPosition)
    {
        _startPos = _startPosition;
    }

    void Update()
    {
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
                    }
                }
                else
                {
                    _target = null;
                }
                if (Vector3.Distance(transform.position, _target.position) > _range && _currentHealth >= _maxHealth)
                {
                    _target = null;
                    _animator.SetBool("Attack", false);
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
                }
                else
                {
                    _animator.SetBool("Run", true);
                }
            }
        }
        else
        {
            _animator.SetBool("Death", true);
        }
    }

    public void AfterDeath()
    {
        if (photonView.IsMine)
        {
            UpdateHealth(_maxHealth);
            //FindAnyObjectByType<UiManager>()._localPlayer.GetComponent<PlayerMovement>().ExpIncrease(_expIncrement);
            if (_target != null)
            {
                if (_target.CompareTag("Player"))
                {
                    _target.GetComponent<PlayerMovement>().ExpIncrease(_expIncrement);
                    ObjectTurnOff(photonView.ViewID);
                    _target = null;
                }
            }
        }
        FindAnyObjectByType<UiManager>().RespawnEnemies(photonView.ViewID);
        ObjectTurnOff(photonView.ViewID);
    }

    [PunRPC]
    void ObjectTurnOff(int viewId)
    {
        PhotonView _pv = PhotonView.Find(viewId);
        _pv.gameObject.SetActive(false);
    }

    private void FixedUpdate()
    {
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
        if (_canHit)
        {
            _target.gameObject.GetComponent<PlayerMovement>().TakeDamage(_attackDamage);
            _animator.SetInteger("StrikeNumber", _hitCounter);
            _canHit = false;
        }
    }

    public void AttackDone()
    {
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
        if (_currentHealth > 0)
        {
            //_agent.velocity = Vector3.zero;
            _target = _theplayer.transform;
            _currentHealth -= damage;// * _damageFactor;
            photonView.RPC("UpdateHealth", RpcTarget.AllBuffered, _currentHealth);
            _animator.SetBool("GotHit", true);
        }
    }

    public void GotHitReset()
    {
        _animator.SetBool("GotHit", false);
    }

    [PunRPC]
    void UpdateHealth(float newHealth)
    {
        _currentHealth = newHealth;
    }

    void faceTarget(Vector3 _lookAt)
    {
        Vector3 dir = (_lookAt - transform.position).normalized;
        Quaternion lookRotation = Quaternion.LookRotation(new Vector3(dir.x, transform.position.y, dir.z));
        transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, 5f * Time.deltaTime);
    }
}
