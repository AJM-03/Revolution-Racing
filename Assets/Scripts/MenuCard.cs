using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MenuCard : MonoBehaviour
{
    public GameObject[] enable;
    public GameObject[] disable;

    public AudioClip selectSound;

    public GameObject lapTime, trophy;
    public Material[] trophyColours;

    public MenuManager.MenuOptions option;
    public RaceManager.Track track;
    public RaceManager.Mode mode;
    public GameObject car;
    public bool mirror;




    public void OnClick()
    {
        if (option != MenuManager.MenuOptions.Car)
            StartCoroutine(Clicked());
        else
        {
            SoundManager.Instance.PlaySound(selectSound, 0.8f);
            MenuManager.Instance.StartRace();
        }
    }

    private IEnumerator Clicked()
    {

        for (int i = 0; i < disable.Length; i++)  // Objects to disable
        {
            if (disable[i].gameObject.GetComponent<Animator>())
                disable[i].gameObject.GetComponent<Animator>().SetTrigger("Out");  // Play the dissapear animation

            Transform[] child = disable[i].gameObject.transform.GetComponentsInChildren<Transform>();
            for (var x = 0; x < child.Length; x++)
            {
                if (child[x].gameObject.GetComponent<Animator>())
                    child[x].gameObject.GetComponent<Animator>().SetTrigger("Out");  // Play the animation on it's children
            }
        }


        SoundManager.Instance.PlaySound(selectSound, 0.8f);  // Play the select sound

        yield return new WaitForSeconds(0.5f);


        if (option == MenuManager.MenuOptions.Track)  // If this is a track card
            MenuManager.Instance.chosenTrack = track;
        else if (option == MenuManager.MenuOptions.Mirror)  // If this is a mirror card
            MenuManager.Instance.mirror = mirror;
        else if (option == MenuManager.MenuOptions.Mode)  // If this is a mode card
        {
            MenuManager.Instance.chosenMode = mode;
            if (mode == RaceManager.Mode.Elimination)
                MenuManager.Instance.carsInRace = 5;  // If it is elimination, raise the number of cars in the race
            else
                MenuManager.Instance.carsInRace = 4;
        }


        MenuManager.Instance.returnTo = enable[0].GetComponent<MenuReturn>();  // Sets the return to screen

        MenuManager.Instance.EnableNewCards(enable);  // Enables all of the new cards

        for (int i = 0; i < disable.Length; i++)  // Disables the old cards
        {
            disable[i].gameObject.SetActive(false);
        }
    }



    private void OnEnable()
    {
        if (option == MenuManager.MenuOptions.Track)  // Setting the saved records on the track cards
        {
            trophy.gameObject.SetActive(true);
            lapTime.gameObject.SetActive(false);

            if (MenuManager.Instance.chosenMode == RaceManager.Mode.Race)  // Set trophies
            {
                if (track == RaceManager.Track.Garden)  
                {
                    if (MenuManager.Instance.mirror)
                        SetTrophyColour(SaveManager.Instance.gardenMirrorRaceTrophy);
                    else
                        SetTrophyColour(SaveManager.Instance.gardenRaceTrophy);
                }

                else if (track == RaceManager.Track.Beach)
                {
                    if (MenuManager.Instance.mirror)
                        SetTrophyColour(SaveManager.Instance.beachMirrorRaceTrophy);
                    else
                        SetTrophyColour(SaveManager.Instance.beachRaceTrophy);
                }
            }



            else if (MenuManager.Instance.chosenMode == RaceManager.Mode.Elimination)  // Set trophies
            {

                if (track == RaceManager.Track.Garden)
                {
                    if (MenuManager.Instance.mirror)
                        SetTrophyColour(SaveManager.Instance.gardenMirrorEliminationTrophy);
                    else
                        SetTrophyColour(SaveManager.Instance.gardenEliminationTrophy);
                }

                else if (track == RaceManager.Track.Beach)
                {
                    if (MenuManager.Instance.mirror)
                        SetTrophyColour(SaveManager.Instance.beachMirrorEliminationTrophy);
                    else
                        SetTrophyColour(SaveManager.Instance.beachEliminationTrophy);
                }
            }


            else if (MenuManager.Instance.chosenMode == RaceManager.Mode.TimeTrials)  // Set best lap time
            {
                trophy.gameObject.SetActive(false);
                lapTime.gameObject.SetActive(true);

                if (track == RaceManager.Track.Garden)
                {
                    float minutes = Mathf.FloorToInt(SaveManager.Instance.gardenTTTime / 60);
                    float seconds = Mathf.FloorToInt(SaveManager.Instance.gardenTTTime % 60);
                    float milliSeconds = (SaveManager.Instance.gardenTTTime % 1) * 100;
                    lapTime.GetComponent<TMPro.TextMeshProUGUI>().text = string.Format("{0:00}:{1:00}:{2:00}", minutes, seconds, milliSeconds);  // Format time
                }

                else if (track == RaceManager.Track.Beach)
                {
                    float minutes = Mathf.FloorToInt(SaveManager.Instance.beachTTTime / 60);
                    float seconds = Mathf.FloorToInt(SaveManager.Instance.beachTTTime % 60);
                    float milliSeconds = (SaveManager.Instance.beachTTTime % 1) * 100;
                    lapTime.GetComponent<TMPro.TextMeshProUGUI>().text = string.Format("{0:00}:{1:00}:{2:00}", minutes, seconds, milliSeconds);  // Format time
                }
            }
        }
    }

    private void SetTrophyColour(int colour)  // Sets the colour of the card's trophy
    {
        if (colour == 1)
            trophy.GetComponent<Renderer>().material = trophyColours[2];
        else if (colour == 2)
            trophy.GetComponent<Renderer>().material = trophyColours[1];
        else if (colour == 3)
            trophy.GetComponent<Renderer>().material = trophyColours[0];
        else
            trophy.GetComponent<Renderer>().material = trophyColours[3];
    }
}
