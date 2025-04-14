using UnityEngine;
using UnityEngine.UI;

public class PracticeDummy : MonoBehaviour
{
    [SerializeField] Image _healthBar;
    [SerializeField] int _health;
    [SerializeField] Animator _animator;

    public void TakeDamage(int damage)
    {
        if (_health > 10)
        {
            _health -= damage;
            _animator.SetBool("GotHit", true);
            _healthBar.fillAmount = _health / 100f;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void GotHitReset()
    {
        _animator.SetBool("GotHit", false);
    }
}
