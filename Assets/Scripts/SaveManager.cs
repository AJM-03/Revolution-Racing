using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SaveManager : MonoBehaviour
{
    public static SaveManager Instance;

    public int gardenRaceTrophy, beachRaceTrophy, gardenEliminationTrophy, beachEliminationTrophy;
    public int gardenMirrorRaceTrophy, beachMirrorRaceTrophy, gardenMirrorEliminationTrophy, beachMirrorEliminationTrophy;
    public float gardenTTTime, beachTTTime;
    public bool muteMusic;
    public TTGhost gardenGhost, beachGhost;


    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            LoadGame();
        }
        else
            Destroy(gameObject);

        if (SoundManager.Instance != null)
            SoundManager.Instance.muteMusic = muteMusic;
    }


    public void SaveRace(int trophy, float time)
    {
        if (MenuManager.Instance.chosenMode == RaceManager.Mode.Race)
        {
            if (MenuManager.Instance.chosenTrack == RaceManager.Track.Garden && trophy > gardenRaceTrophy)
            {
                if (MenuManager.Instance.mirror)
                {
                    gardenMirrorRaceTrophy = trophy;
                    PlayerPrefs.SetInt("gardenMirrorRaceTrophy", trophy);
                }
                else
                {
                    gardenRaceTrophy = trophy;
                    PlayerPrefs.SetInt("gardenRaceTrophy", trophy);
                }
            }

            else if (MenuManager.Instance.chosenTrack == RaceManager.Track.Beach && trophy > beachRaceTrophy)
            {
                if (MenuManager.Instance.mirror)
                {
                    beachMirrorRaceTrophy = trophy;
                    PlayerPrefs.SetInt("beachMirrorRaceTrophy", trophy);
                }
                else
                {
                    beachRaceTrophy = trophy;
                    PlayerPrefs.SetInt("beachRaceTrophy", trophy);
                }
            }
        }

        else if (MenuManager.Instance.chosenMode == RaceManager.Mode.Elimination)
        {
            if (MenuManager.Instance.chosenTrack == RaceManager.Track.Garden && trophy > gardenEliminationTrophy)
            {
                if (MenuManager.Instance.mirror)
                {
                    gardenMirrorEliminationTrophy = trophy;
                    PlayerPrefs.SetInt("gardenMirrorEliminationTrophy", trophy);
                }
                else
                {
                    gardenEliminationTrophy = trophy;
                    PlayerPrefs.SetInt("gardenEliminationTrophy", trophy);
                }
            }

            else if (MenuManager.Instance.chosenTrack == RaceManager.Track.Beach && trophy > beachEliminationTrophy)
            {
                if (MenuManager.Instance.mirror)
                {
                    beachMirrorEliminationTrophy = trophy;
                    PlayerPrefs.SetInt("beachMirrorEliminationTrophy", trophy);
                }
                else
                {
                    beachEliminationTrophy = trophy;
                    PlayerPrefs.SetInt("beachEliminationTrophy", trophy);
                }
            }
        }


        else if (MenuManager.Instance.chosenMode == RaceManager.Mode.TimeTrials)
        {
            if (MenuManager.Instance.chosenTrack == RaceManager.Track.Garden)
            {
                gardenTTTime = time;
                PlayerPrefs.SetFloat("gardenTime", time);
            }

            else if (MenuManager.Instance.chosenTrack == RaceManager.Track.Beach)
            {
                beachTTTime = time;
                PlayerPrefs.SetFloat("beachTime", time);
            }
        }
    }

    public void SaveSettings()
    {
        PlayerPrefs.SetFloat("volume", AudioListener.volume);
        if (SoundManager.Instance.muteMusic)
            PlayerPrefs.SetInt("muteMusic", 1);
        else
            PlayerPrefs.SetInt("muteMusic", 0);
    }


    public void LoadGame()
    {
        gardenRaceTrophy = PlayerPrefs.GetInt("gardenRaceTrophy");
        beachRaceTrophy = PlayerPrefs.GetInt("beachRaceTrophy");
        gardenEliminationTrophy = PlayerPrefs.GetInt("gardenEliminationTrophy");
        beachEliminationTrophy = PlayerPrefs.GetInt("beachEliminationTrophy");
        gardenMirrorRaceTrophy = PlayerPrefs.GetInt("gardenMirrorRaceTrophy");
        beachMirrorRaceTrophy = PlayerPrefs.GetInt("beachMirrorRaceTrophy");
        gardenMirrorEliminationTrophy = PlayerPrefs.GetInt("gardenMirrorEliminationTrophy");
        beachMirrorEliminationTrophy = PlayerPrefs.GetInt("beachMirrorEliminationTrophy");
        gardenTTTime = PlayerPrefs.GetFloat("gardenTime");
        beachTTTime = PlayerPrefs.GetFloat("beachTime");

        AudioListener.volume = PlayerPrefs.GetFloat("volume");
        if (PlayerPrefs.GetInt("muteMusic") == 1)
            muteMusic = true; 
        else
            muteMusic = false;
    }


    public void DeleteSave()
    {
        PlayerPrefs.SetInt("gardenRaceTrophy", 0);
        PlayerPrefs.SetInt("beachRaceTrophy", 0);
        PlayerPrefs.SetInt("gardenEliminationTrophy", 0);
        PlayerPrefs.SetInt("beachEliminationTrophy", 0);
        PlayerPrefs.SetInt("gardenMirrorRaceTrophy", 0);
        PlayerPrefs.SetInt("beachMirrorRaceTrophy", 0);
        PlayerPrefs.SetInt("gardenMirrorEliminationTrophy", 0);
        PlayerPrefs.SetInt("beachMirrorEliminationTrophy", 0);
        PlayerPrefs.SetFloat("gardenTime", 0);
        PlayerPrefs.SetFloat("beachTime", 0);
        PlayerPrefs.SetFloat("volume", 1);
        PlayerPrefs.SetInt("muteMusic", 0);
        gardenGhost.lapTime = 0;
        gardenGhost.car = null;
        gardenGhost.timeStamp.Clear();
        gardenGhost.position.Clear();
        gardenGhost.rotation.Clear();
        beachGhost.lapTime = 0;
        beachGhost.car = null;
        beachGhost.timeStamp.Clear();
        beachGhost.position.Clear();
        beachGhost.rotation.Clear();
        LoadGame();
    }
}
