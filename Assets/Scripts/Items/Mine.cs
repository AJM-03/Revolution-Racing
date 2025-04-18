using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Mine : MonoBehaviour
{
    public bool capsule, tornado;
    public int strength;
    public float slowdown;
    public ParticleSystem hitParticles;
    public GameObject colouredHalf, transparentHalf;
    public GameObject sender;
    public GameObject[] oilPuddles;
    public AudioClip oilSplatSound, oilSlipSound, capsuleExplosionSound;
    public Animator anim;
    private bool waitDelay;


    private void Start()
    {
        StartCoroutine(SpawnDelay());

        if (!capsule && !tornado)
        {
            int rand = Random.Range(0, oilPuddles.Length);
            oilPuddles[rand].SetActive(true);
            SoundManager.Instance.PlaySound(oilSplatSound, 0.5f);
        }
    }



    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.GetComponent<CarCollider>() && other.gameObject.GetComponent<CarCollider>().car != null)
        {
            if (other.gameObject.GetComponent<CarCollider>().car.gameObject == sender && waitDelay) { }  // If it is not hitting your car

            else
            {
                CarController car = other.gameObject.GetComponent<CarCollider>().car;
                car.HitByItem(strength, slowdown, false, tornado);


                if (capsule)
                {
                    hitParticles.Play();
                    colouredHalf.GetComponent<Rigidbody>().isKinematic = false;
                    transparentHalf.GetComponent<Rigidbody>().isKinematic = false;
                    colouredHalf.GetComponent<Rigidbody>().AddForce(new Vector3(24, 18, 0), ForceMode.Impulse);
                    transparentHalf.GetComponent<Rigidbody>().AddForce(new Vector3(-24, 16, 0), ForceMode.Impulse);
                    gameObject.GetComponent<SphereCollider>().enabled = false;
                    SoundManager.Instance.PlaySound(capsuleExplosionSound, 0.75f);
                    StartCoroutine(DestroyObject());
                }

                else if (tornado)
                {
                    gameObject.GetComponent<CapsuleCollider>().enabled = false;
                    gameObject.transform.parent = car.transform;
                    gameObject.transform.localPosition = new Vector3(0, -1, 0);
                    gameObject.GetComponent<AudioSource>().volume = 1;
                    StartCoroutine(DestroyObject());
                }

                else
                {
                    hitParticles.Play();
                    gameObject.GetComponent<BoxCollider>().enabled = false;
                    SoundManager.Instance.PlaySound(oilSlipSound, 0.4f);
                    SoundManager.Instance.PlaySound(oilSplatSound, 0.15f);
                    anim.SetTrigger("Shrink");
                    StartCoroutine(DestroyObject());
                }
            }
        }
    }

    private IEnumerator DestroyObject()
    {
        yield return new WaitForSeconds(3);
        if (capsule)
            Destroy(transform.parent.gameObject);
        else
            Destroy(this.gameObject);
    }

    private IEnumerator SpawnDelay()
    {
        waitDelay = true;
        yield return new WaitForSeconds(1);
        waitDelay = false;
    }
}
