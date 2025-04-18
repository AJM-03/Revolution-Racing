using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemCapsule : MonoBehaviour
{
    public RaceManager.Item itemType;
    public float resetTime;
    public ParticleSystem openParticles, resetParticles;
    public GameObject colouredHalf, transparentHalf;

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.GetComponent<CarCollider>() && other.gameObject.GetComponent<CarCollider>().car != null)
        {
            CarController car = other.gameObject.GetComponent<CarCollider>().car;
            RaceManager.Item oldItem = car.currentItem;
            int oldItemLevel = car.itemLevel;

            if (itemType == oldItem && oldItemLevel < 3)
            {
                car.itemLevel++;
                car.UpdateItemUI("Wipe");
            }

            else if(itemType != oldItem)
            {
                car.currentItem = itemType;

                if (car.carPosition > 3)
                    car.itemLevel = 2;
                else
                    car.itemLevel = 1;

                car.UpdateItemUI("Grow");
            }

            openParticles.Play();

            colouredHalf.GetComponent<Rigidbody>().isKinematic = false;
            transparentHalf.GetComponent<Rigidbody>().isKinematic = false;
            colouredHalf.GetComponent<Rigidbody>().AddForce(new Vector3(8, 6, 0), ForceMode.Impulse);
            transparentHalf.GetComponent<Rigidbody>().AddForce(new Vector3(-8, 5, 0), ForceMode.Impulse);
            gameObject.GetComponent<SphereCollider>().enabled = false;

            StartCoroutine(Reset());
        }
    }


    private IEnumerator Reset()
    {
        yield return new WaitForSeconds(resetTime);
        resetParticles.Play();
        yield return new WaitForSeconds(0.1f);

        colouredHalf.GetComponent<Rigidbody>().isKinematic = true;
        transparentHalf.GetComponent<Rigidbody>().isKinematic = true;
        colouredHalf.transform.localPosition = new Vector3(0, 0, 0);
        transparentHalf.transform.localPosition = new Vector3(0, 0, 0);
        colouredHalf.transform.localRotation = new Quaternion(0, 0, 0, 1);
        transparentHalf.transform.localRotation = new Quaternion(0, 0, 1, 0);
        gameObject.GetComponent<SphereCollider>().enabled = true;
    }
}
