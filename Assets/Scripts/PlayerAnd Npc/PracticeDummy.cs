using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.SceneManagement;

public class PracticeDummy : MonoBehaviour
{
    [SerializeField] Image _healthBar;
    [SerializeField] int _health;
    [SerializeField] Animator _animator;

    [SerializeField]
    GameObject TutorialFinished;

    public void TakeDamage(int damage)
    {
        //practice dummy take damage
        if (_health > 10)
        {
            _health -= damage;
            _animator.SetBool("GotHit", true);
            _healthBar.fillAmount = _health / 100f;
        }
        else
        {
            StartCoroutine(Restart());
        }
    }

    public void GotHitReset()
    {
        //gothit reset
        _animator.SetBool("GotHit", false);
    }

    IEnumerator Restart()
    {
        //after dummy die
        TutorialFinished.SetActive(true);
        yield return new WaitForSeconds(4f);
        PlayerPrefs.SetInt("Tutorial",1);
        PlayerPrefs.Save();
        SceneManager.LoadScene(0);
        Destroy(gameObject);
    }
}
