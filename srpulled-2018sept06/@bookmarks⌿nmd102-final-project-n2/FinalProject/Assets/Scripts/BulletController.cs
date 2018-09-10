using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BulletController : MonoBehaviour {
    public GameObject fireball;

    private AudioSource hitSound;
    private AudioSource explodeSound;
    private GameObject groundObject;

    private void OnTriggerEnter(Collider collider)
    {
        // Locate audio clips
        foreach (AudioSource aSource in GetComponents<AudioSource>())
        {
            aSource.enabled = true;
            switch (aSource.clip.name)
            {
                case "FireExplosion1":
                    hitSound = aSource;
                    break;
                case "FireExplosion5":
                    explodeSound = aSource;
                    break;
            }
        }
        if (!collider.gameObject.CompareTag("Player") && !collider.CompareTag("WaterTrigger") && !collider.CompareTag("OobTrigger"))
        {
            groundObject = collider.gameObject;
            hitSound.Play();
            GameObject fb;
            fb = Instantiate(fireball, transform.position, transform.rotation) as GameObject;
            fb.GetComponent<ParticleSystem>().Play();
            Destroy(fb, 10);
            if (groundObject.CompareTag("DestructibleEnvironment"))
            {
                Destroy(groundObject);
                explodeSound.Play();
            }
            Destroy(gameObject);
        }
    }
}
