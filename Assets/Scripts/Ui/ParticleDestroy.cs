using UnityEngine;

public class ParticleDestroy : MonoBehaviour
{
    void Start()
    {
        //destroy the particle
        Destroy(gameObject,1f);
    }
}
