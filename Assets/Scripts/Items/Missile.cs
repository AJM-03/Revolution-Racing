using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Missile : MonoBehaviour
{
    public bool homing, firstPlace;
    public float speed, turnStrength, homingDistance, slowdown;
    public int strength;
    public ParticleSystem destroyParticles;
    public GameObject rocket, sender, target;
    public AudioClip lockOnAudio, explosionAudio;
    private Rigidbody rb;
    public Transform nextWaypoint;
    private float aiTurnMultiplier;
    private bool targetLock = false;

    private void Start()
    {
        rb = gameObject.GetComponent<Rigidbody>();
    }



    private void Update()
    {
        rb.velocity = transform.forward * speed;

        if (homing && nextWaypoint != null && target != null)
        {
            float distance = Vector3.Distance(target.transform.position, transform.position);

            if (distance < homingDistance)
            {
                nextWaypoint = target.transform;
                targetLock = true;
            }

            Quaternion lookRotation = Quaternion.LookRotation(nextWaypoint.position - transform.position);
            transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, turnStrength * Time.deltaTime / 2 * aiTurnMultiplier);  // Turn the car
        }
    }



    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.GetComponent<CarCollider>() && other.gameObject.GetComponent<CarCollider>().car != null)
        {
            if (other.gameObject.GetComponent<CarCollider>().car.gameObject != sender)
            {
                CarController car = other.gameObject.GetComponent<CarCollider>().car;
                car.HitByItem(strength, slowdown, true, false);
                if (firstPlace && car.gameObject == target)
                    Destroy(this.gameObject);
                else if (!firstPlace || !homing)
                    Destroy(this.gameObject);
            }
        }
        else if (other.gameObject.layer != 3 && other.gameObject.layer != 7 && other.isTrigger == false)
        {
            destroyParticles.Play();
            SoundManager.Instance.PlaySound(explosionAudio, 0.4f);
            StartCoroutine(DestroyObject());
        }
        else if (other.isTrigger == true && other.gameObject.GetComponent<OutOfBounds>())
        {
            destroyParticles.Play();
            SoundManager.Instance.PlaySound(explosionAudio, 0.5f);
            StartCoroutine(DestroyObject());
        }
    }



    public void GetNextWaypoint(GameObject waypoints, float turnMultiplier)
    {
        if (nextWaypoint == null && firstPlace)
            SoundManager.Instance.PlaySound(lockOnAudio, 0.9f);
        else if (nextWaypoint == null)
            SoundManager.Instance.PlaySound(lockOnAudio, 0.6f);

        if (targetLock == false && homing)
        {
            List<Transform> options = new List<Transform>();
            foreach (Transform child in waypoints.transform)
            {
                options.Add(child.transform);
            }
            //nextWaypoint = options[Random.Range(0, options.Count)];
            nextWaypoint = options[0];

            aiTurnMultiplier = turnMultiplier;
        }
    }


    private IEnumerator DestroyObject()
    {
        rocket.SetActive(false);
        gameObject.GetComponent<CapsuleCollider>().enabled = false;
        gameObject.GetComponent<AudioSource>().enabled = false;
        yield return new WaitForSeconds(3);
        Destroy(this.gameObject);
    }
}
