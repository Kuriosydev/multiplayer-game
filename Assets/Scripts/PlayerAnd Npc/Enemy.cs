using Photon.Pun;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;

public class Enemy : MonoBehaviourPunCallbacks
{
    NavMeshAgent _agent;
    PlayerMovement _player;
    Transform _target;
    Vector3 _startPos;
    [SerializeField] int _range = 5, _currentHealth, _maxHealth = 100, _attackDamage = 10;
    public Image _healthbar;
    Animator _animator;
    bool _canHit = true;
    int _hitCounter = 0;

    void Start()
    {
        _currentHealth = _maxHealth;
        _startPos = transform.position;
        _agent = GetComponent<NavMeshAgent>();
        _animator = GetComponent<Animator>();
    }

    void Update()
    {
        _healthbar.fillAmount = _currentHealth / 100f;
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
            if (_target != null)
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
                if (Vector3.Distance(transform.position, _target.position) > _range)
                {
                    _target = null;
                    _animator.SetBool("Attack", false);
                }
            }
            else
            {
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
            PhotonNetwork.Destroy(gameObject);
        }
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

    public void TakeDamage(int damage)
    {
        if (_currentHealth > 0)
        {
            _currentHealth -= damage;
            photonView.RPC("UpdateHealth", RpcTarget.AllBuffered, _currentHealth);
            _animator.SetBool("GotHit", true);
        }
    }

    public void GotHitReset()
    {
        _animator.SetBool("GotHit", false);
    }

    [PunRPC]
    void UpdateHealth(int newHealth)
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
