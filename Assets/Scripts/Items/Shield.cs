using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Shield : MonoBehaviour
{
    public bool shockwave, invincible;
    public float duration;
    public int strength;
    public float slowdown;
    public GameObject sender;
    public ParticleSystem particles, finishParticles;
    public AudioClip hitWithShieldAudio;
    public Animator anim;


    private void Start()
    {
        StartCoroutine(Timer());

        if (shockwave)
            StartCoroutine(Shockwave());
        else
            StartCoroutine(AudioFade(gameObject.GetComponent<AudioSource>(), 1, 0.5f));
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.GetComponent<CarCollider>() && other.gameObject.GetComponent<CarCollider>().car != null)
        {
            CarController car = other.gameObject.GetComponent<CarCollider>().car;

            if (car.gameObject != sender && car.spinningOut == false)
            {
               if (!shockwave)
                    SoundManager.Instance.PlaySound(hitWithShieldAudio, 0.5f);


                if (shockwave || invincible)
                    car.HitByItem(strength, slowdown, false, false);

                else
                {
                    car.HitByItem(strength, slowdown, false, false);
                    StartCoroutine(DestroyObject());
                }
            }
        }

        if (other.gameObject.GetComponent<Missile>())
        {
            Destroy(other.gameObject.GetComponent<Missile>().gameObject);
            StartCoroutine(DestroyObject());
        }

        else if (other.gameObject.GetComponent<Mine>())
        {
            Destroy(other.gameObject.GetComponent<Mine>().gameObject);
            StartCoroutine(DestroyObject());
        }
    }


    private IEnumerator DestroyObject()
    {
        if (!shockwave) StartCoroutine(AudioFade(gameObject.GetComponent<AudioSource>(), 2, 0));
        gameObject.GetComponent<CapsuleCollider>().enabled = false;
        if (finishParticles != null) finishParticles.Play();
        particles.Stop();
        sender.GetComponent<CarController>().currentShield = null;
        yield return new WaitForSeconds(3);
        Destroy(this.gameObject);
    }


    private IEnumerator Timer()
    {
        yield return new WaitForSeconds(duration);
        StartCoroutine(DestroyObject());
    }

    private IEnumerator Shockwave()
    {
        gameObject.GetComponent<CapsuleCollider>().enabled = false;
        yield return new WaitForSeconds(0.3f);
        gameObject.GetComponent<CapsuleCollider>().enabled = true;
    }

    private static IEnumerator AudioFade(AudioSource audioSource, float duration, float targetVolume)
    {
        float currentTime = 0;
        float start = audioSource.volume;
        while (currentTime < duration)
        {
            currentTime += Time.deltaTime;
            audioSource.volume = Mathf.Lerp(start, targetVolume, currentTime / duration);
            yield return null;
        }
        yield break;
    }
}
