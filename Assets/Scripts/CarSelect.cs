using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class CarSelect : MonoBehaviour
{
    private GameObject spawnedCar, selectedCar;
    private CarProfile spawnedCarProfile;
    public GameObject defaultCar;
    public TMPro.TextMeshProUGUI colourText;
    public Slider speedSlider, accelSlider, handlingSlider, offroadSlider;


    private void Awake()
    {
        selectedCar = defaultCar;
        SpawnCar();
    }

    public void ChangeSelectedCar()  // When left or right is pressed
    {
        StartCoroutine(ChangeCar());
    }

    private IEnumerator ChangeCar()
    {
        yield return new WaitForSeconds(0.1f);
        selectedCar = null;
        Destroy(spawnedCar);
        spawnedCarProfile = null;
        selectedCar = EventSystem.current.currentSelectedGameObject.GetComponent<MenuCard>().car;
        SpawnCar();
    }


    private void SpawnCar()
    {
        spawnedCar = Instantiate(selectedCar, transform.position, transform.rotation, transform);
        spawnedCarProfile = spawnedCar.GetComponent<CarProfile>();

        spawnedCarProfile.carCollisionRB.gameObject.SetActive(false);

        speedSlider.value = spawnedCarProfile.forwardSpeed;
        accelSlider.value = -spawnedCarProfile.accelerationTime;
        handlingSlider.value = spawnedCarProfile.handling;
        offroadSlider.value = spawnedCarProfile.offroad;

        spawnedCarProfile.skidMarkTrail.gameObject.SetActive(false);
        spawnedCarProfile.airTrail.gameObject.SetActive(false);
        spawnedCarProfile.offroadTrail.gameObject.SetActive(false);

        spawnedCar.transform.position += spawnedCarProfile.spawnOffset;

        colourText.gameObject.transform.parent.gameObject.GetComponent<Image>().color = spawnedCarProfile.variantColour;
        colourText.text = (spawnedCarProfile.variantNumber + "/" + spawnedCarProfile.variants.Length);

        MenuManager.Instance.chosenCar = selectedCar;
    }


    public void ChangeSelectedColour(bool up)  // When up or down is pressed
    {
        StartCoroutine(ChangeCarColour(up));
    }

    private IEnumerator ChangeCarColour(bool up)
    {
        if (spawnedCarProfile.variants.Length > 1)
        {
            yield return new WaitForSeconds(0.1f);

            if (!up)
            {
                if (spawnedCarProfile.variantNumber == spawnedCarProfile.variants.Length)
                    selectedCar = spawnedCarProfile.variants[0];

                else
                    selectedCar = spawnedCarProfile.variants[spawnedCarProfile.variantNumber];
            }

            else
            {
                if (spawnedCarProfile.variantNumber == 1)
                    selectedCar = spawnedCarProfile.variants[spawnedCarProfile.variants.Length - 1];

                else
                    selectedCar = spawnedCarProfile.variants[spawnedCarProfile.variantNumber - 2];
            }

            Destroy(spawnedCar);
            spawnedCarProfile = null;
            SpawnCar();
        }
    }
}
