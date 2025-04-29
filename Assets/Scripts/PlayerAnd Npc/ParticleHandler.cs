using Photon.Pun;
using UnityEngine;

public class ParticleHandler : MonoBehaviour
{
    GameObject _player;
    PhotonView _photonView;

    private void Start()
    {
        //assign references 
        _photonView = GetComponent<PhotonView>();
        Invoke("DestroyParticle", 3f);
        if (_player == null)
        {
            _player = FindAnyObjectByType<PlayerMovement>().gameObject;
        }
    }

    private void Update()
    {
        //makes the object go forward
        GetComponent<Rigidbody>().AddForce(transform.forward * 15f * 1000f, ForceMode.Force);
    }

    private void OnCollisionEnter(Collision other)
    {
        //checks which things devil ball hits
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
                    other.collider.gameObject.GetComponent<Enemy>().TakeDamage(_player.GetComponent<PlayerMovement>()._damage,_player);
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
        //assign player obj the player
        _player = _playerobj;
    }

    public void DestroyParticle()
    {
        // make the fireball destroy after some time

        if (_photonView.IsMine)
        {
            PhotonNetwork.Destroy(gameObject);
        }
    }
}
