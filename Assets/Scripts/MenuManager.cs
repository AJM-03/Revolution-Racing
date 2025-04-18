using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class MenuManager : MonoBehaviour
{
    public static MenuManager Instance;
    public enum MenuOptions { Track, Mode, Car, Mirror }

    public AudioClip moveSound;
    public GameObject transitionOut;
    public CarSelect carSelect;

    public MenuReturn returnTo;

    public RaceManager.Track chosenTrack;
    public RaceManager.Mode chosenMode;
    public GameObject chosenCar;
    public bool mirror;
    public int carsInRace;



    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
            Destroy(gameObject);
    }




    public void ReturnButtonPressed()
    {
        if (returnTo != null)
            returnTo.OnReturn();
    }

    public void DirectionPressed()
    {
        SoundManager.Instance.PlaySound(moveSound, 0.3f);
        if (carSelect != null && carSelect.transform.parent.gameObject.activeSelf)
            carSelect.ChangeSelectedCar();
    }

    public void UpDirectionPressed()
    {
        if (carSelect != null && carSelect.transform.parent.gameObject.activeSelf)
        {
            SoundManager.Instance.PlaySound(moveSound, 0.3f);
            carSelect.ChangeSelectedColour(true);
        }
    }

    public void DownDirectionPressed()
    {
        if (carSelect != null && carSelect.transform.parent.gameObject.activeSelf)
        {
            SoundManager.Instance.PlaySound(moveSound, 0.3f);
            carSelect.ChangeSelectedColour(false);
        }
    }

    public void StartRace()
    {
        StartCoroutine(LoadScene());
    }

    private IEnumerator LoadScene()
    {
        transitionOut.SetActive(true);

        yield return new WaitForSeconds(3);

        SceneManager.LoadScene(1);
    }



    public void EnableNewCards(GameObject[] enable)
    {
        StartCoroutine(EnableCards(enable));
    }

    private IEnumerator EnableCards(GameObject[] enable)
    {
        for (int i = 0; i < enable.Length; i++)
        {
            enable[i].gameObject.SetActive(true);
            if (enable[i].gameObject.GetComponent<Animator>())
                enable[i].gameObject.GetComponent<Animator>().SetBool("In", true);

            Transform[] child = enable[i].gameObject.transform.GetComponentsInChildren<Transform>();
            for (var x = 0; x < child.Length; x++)
            {
                if (child[x].gameObject.GetComponent<Animator>() && child[x].gameObject.activeInHierarchy)
                    child[x].gameObject.GetComponent<Animator>().SetBool("In", true);
            }
        }

        yield return new WaitForSeconds(0.25f);

        for (int i = 0; i < enable.Length; i++)
        {
            if (enable[i].gameObject.GetComponent<Animator>())
                enable[i].gameObject.GetComponent<Animator>().SetBool("In", false);

            Transform[] child = enable[i].gameObject.transform.GetComponentsInChildren<Transform>();
            for (var x = 0; x < child.Length; x++)
            {
                if (child[x].gameObject.GetComponent<Animator>())
                    child[x].gameObject.GetComponent<Animator>().SetBool("In", false);
            }
        }

        if (enable[0].transform.GetChild(0).GetComponent<Button>())
            enable[0].transform.GetChild(0).GetComponent<Button>().Select();
    }
}
