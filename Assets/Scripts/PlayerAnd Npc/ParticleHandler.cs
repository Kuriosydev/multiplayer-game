using Photon.Pun;
using UnityEngine;

public class ParticleHandler : MonoBehaviour
{
    GameObject _player;
    PhotonView _photonView;

    private void Start()
    {
        _photonView = GetComponent<PhotonView>();
        Invoke("DestroyParticle", 3f);
        if (_player == null)
        {
            _player = FindAnyObjectByType<PlayerMovement>().gameObject;
        }
    }

    private void Update()
    {
        GetComponent<Rigidbody>().AddForce(transform.forward * 15f * 1000f, ForceMode.Force);
    }

    private void OnCollisionEnter(Collision other)
    {
        try
        {
            if (other.collider.gameObject != _player)
            {
                if (other.collider.gameObject.CompareTag("Player"))
                {
                    other.collider.gameObject.GetComponent<PlayerMovement>().TakeDamage(_player.GetComponent<PlayerMovement>()._damage);
                    if (_photonView.IsMine)
                    {
                        PhotonNetwork.Destroy(gameObject);
                    }
                }
                if (other.collider.gameObject.CompareTag("Enemy"))
                {
                    other.collider.gameObject.GetComponent<Enemy>().TakeDamage(_player.GetComponent<PlayerMovement>()._damage);
                    if (_photonView.IsMine)
                    {
                        PhotonNetwork.Destroy(gameObject);
                    }
                }
            }
        }
        catch
        {

        }
    }

    public void PlayerSetUp(GameObject _playerobj)
    {
        _player = _playerobj;
    }

    public void DestroyParticle()
    {
        if (_photonView.IsMine)
        {
            PhotonNetwork.Destroy(gameObject);
        }
    }
}
