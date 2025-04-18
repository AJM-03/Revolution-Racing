using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class RaceManager : MonoBehaviour
{
    public TrackProfile trackProfile;

    public enum Item { Boost, Mine, Missile, Shield, None }
    public enum Track { Garden, Beach}

    public enum Mode {TimeTrials, Race, Elimination};
    public Mode currentMode;
    public bool mirrorMode;

    public int carsInRace;
    public bool racing;
    public Track currentTrack;
    public GameObject gardenTrack, beachTrack;
    public Material daySkybox, nightSkybox;

    public GameObject[] cars;

    public float raceTime;
    public float lapTime;
    public TTGhostRecord ttGhostRecord;
    public GameObject ttGhostCar, currentTTGhostCar;

    private float eliminationCountdown;
    public TMPro.TextMeshProUGUI eliminationText;
    private string eliminationPositionSuffix;
    private int carsLeftInRace;

    public GameObject finishUICamera, racePanel;
    public TMPro.TextMeshProUGUI lapTimeText, raceTimeText, bestLapText;
    public TMPro.TextMeshProUGUI maxLapsText;
    public TMPro.TextMeshProUGUI positionText, positionSuffixText;
    public TMPro.TMP_ColorGradient[] positionTextGradients;
    public GameObject itemBox, lapCountText;
    public GameObject racePositionBox;
    public GameObject[] racePositions;
    public GameObject finishTrophy;
    public Material[] trophyColours;
    public GameObject countdownText;
    public Animator countdownAnim;
    public GameObject transitionOut;
    private int positionShown;

    public AudioClip countdownAudio, raceStartAudio, lapFinishAudio, raceFinishAudio;



    void Start()
    {
        if (MenuManager.Instance.chosenCar == null)  // Load the menu if starting game in editor
            SceneManager.LoadScene(0);

        currentMode = MenuManager.Instance.chosenMode;  // Set up the chosen menu options
        currentTrack = MenuManager.Instance.chosenTrack;
        mirrorMode = MenuManager.Instance.mirror;
        carsInRace = MenuManager.Instance.carsInRace;

        SpawnTrack();  // Enable the correct track
        SpawnRaceCars();  // Disable unused cars
        cars = trackProfile.cars;

        lapTimeText.text = lapTime.ToString("F2");
        raceTimeText.text = lapTime.ToString("F2");


        if (mirrorMode)  // Enable mirror mode
        {
            Camera.main.GetComponent<MirrorFlipCamera>().FlipHorizontal = true;
            finishUICamera.GetComponent<MirrorFlipCamera>().FlipHorizontal = true;
        }
        else  // Disable mirror mode
        {
            Camera.main.GetComponent<MirrorFlipCamera>().FlipHorizontal = false;
            finishUICamera.GetComponent<MirrorFlipCamera>().FlipHorizontal = false;
        }

        racePanel.SetActive(false);

        if (currentMode == Mode.TimeTrials)  // Set up all variables for time trial mode
        {
            lapTimeText.enabled = false;
            carsInRace = 1;
            trackProfile.raceObjects.SetActive(false);
            trackProfile.timeTrialObjects.SetActive(true);
            itemBox.SetActive(true);
            lapCountText.SetActive(false);
            positionText.transform.parent.parent.gameObject.SetActive(false);
            lapTimeText.gameObject.SetActive(true);
            raceTimeText.gameObject.SetActive(false);
            eliminationText.gameObject.SetActive(false);
            ttGhostRecord = cars[0].GetComponent<TTGhostRecord>();

            if (currentTrack == RaceManager.Track.Garden && trackProfile.trackGhost.lapTime == 0 && SaveManager.Instance.gardenTTTime != 0)  // Load the saved times
                trackProfile.trackGhost.lapTime = SaveManager.Instance.gardenTTTime;
            else if (currentTrack == RaceManager.Track.Beach && trackProfile.trackGhost.lapTime == 0 && SaveManager.Instance.beachTTTime != 0)
                trackProfile.trackGhost.lapTime = SaveManager.Instance.beachTTTime;
        }

        else if (currentMode == Mode.Race)  // Set up all variables for race mode
        {
            maxLapsText.text = trackProfile.laps.ToString();
            SetCarPositions();
            trackProfile.raceObjects.SetActive(true);
            trackProfile.timeTrialObjects.SetActive(false);
            raceTimeText.gameObject.SetActive(true);
            lapTimeText.gameObject.SetActive(false);
            lapCountText.SetActive(true);
            eliminationText.gameObject.SetActive(false);
        }

        else if (currentMode == Mode.Elimination)  // Set up all variables for elimination mode
        {
            SetCarPositions();
            trackProfile.raceObjects.SetActive(true);
            trackProfile.timeTrialObjects.SetActive(false);
            raceTimeText.gameObject.SetActive(false);
            lapTimeText.gameObject.SetActive(false);
            eliminationText.gameObject.SetActive(true);
            lapCountText.SetActive(false);
            eliminationCountdown = 60;
            carsLeftInRace = carsInRace;

            if (carsLeftInRace == 2)
                eliminationPositionSuffix = "nd";
            else if (carsLeftInRace == 3)
                eliminationPositionSuffix = "rd";
            else if (carsLeftInRace == 4 || carsLeftInRace == 5 || carsLeftInRace == 6)
                eliminationPositionSuffix = "th";
        }

        StartCoroutine(StartCountdown());  // Start the race
    }



    void Update()
    {
        if (racing)
        {
            lapTime += Time.deltaTime;  // Set the lap and race time
            raceTime += Time.deltaTime;

            float minutes = Mathf.FloorToInt(lapTime / 60);  // Set the lap time UI text
            float seconds = Mathf.FloorToInt(lapTime % 60);
            float milliSeconds = (lapTime % 1) * 100;
            lapTimeText.text = string.Format("{0:00}:{1:00}:{2:00}", minutes, seconds, milliSeconds);

            minutes = Mathf.FloorToInt(raceTime / 60);  // Set the race time UI text
            seconds = Mathf.FloorToInt(raceTime % 60);
            raceTimeText.text = string.Format("{0:00}:{1:00}", minutes, seconds);
            
            CarPositionUpdate();  // Check the car race positions

            if (currentMode == Mode.Elimination)  // Countdown to elimination
                EliminationCountdown();
        }
    }



    void SetCarPositions()  // Set up the cars at the start of the race
    {
        for (int i = 0; i < carsInRace; i++)
        {
            cars[i].GetComponent<CarController>().carPosition = i + 1;
            cars[i].GetComponent<CarController>().carNumber = i;
            cars[i].GetComponent<CarController>().carsInRace = cars;
        }
    }


    /*
     *  - Loop through cars
     *      - If position is higher than car in front, move up
     *      - else if position is lower than car behind move down
     */

    void CarPositionUpdate()     
    {
        if (cars[0].GetComponent<CarController>().respawnPoints != null)
        {
            for (int i = 0; i < carsInRace; i++)
            {
                CarController thisCar = cars[i].GetComponent<CarController>();
                CarController carInFront = null;
                CarController carBehind = null;

                for (int x = 0; x < carsInRace; x++)
                {
                    if (cars[x].GetComponent<CarController>().carPosition == thisCar.carPosition - 1)  // Car in front
                        carInFront = cars[x].GetComponent<CarController>();
                    else if (cars[x].GetComponent<CarController>().carPosition == thisCar.carPosition + 1)  // Car behind
                        carBehind = cars[x].GetComponent<CarController>();
                }


                if (carInFront != null && ComparePosition(thisCar, carInFront) == "In front")
                {
                    thisCar.carPosition--;
                    carInFront.carPosition++;
                }

                else
                {
                    if (carBehind != null && ComparePosition(thisCar, carBehind) == "Behind")
                    {
                        thisCar.carPosition++;
                        carBehind.carPosition--;
                    }
                }

                if (thisCar.aiControlled == false)
                    UpdatePositionText(thisCar);
            }
        }
    }


    private string ComparePosition(CarController thisCar, CarController otherCar)  // Check: Lap - Key - Sub - Distance
    {
        if (thisCar.finished || otherCar.finished)
            return ("Finished");
        else
        {
            if (thisCar.lapNumber < otherCar.lapNumber)
                return ("Behind");
            else if (thisCar.lapNumber > otherCar.lapNumber)
                return ("In front");
            else
            {

                if (thisCar.keyCheckpointIndex < otherCar.keyCheckpointIndex)
                    return ("Behind");
                else if (thisCar.keyCheckpointIndex > otherCar.keyCheckpointIndex)
                    return ("In front");
                else
                {

                    if (thisCar.checkpointIndex < otherCar.checkpointIndex)
                        return ("Behind");
                    else if (thisCar.checkpointIndex > otherCar.checkpointIndex)
                        return ("In front");
                    else
                    {

                        if (thisCar.nextCheckpoint != null && otherCar.nextCheckpoint != null)
                        {
                            float thisDist = Vector3.Distance(thisCar.nextCheckpoint.position, thisCar.transform.position);
                            float otherDist = Vector3.Distance(otherCar.nextCheckpoint.position, otherCar.transform.position);
                            if (thisDist > otherDist)
                                return ("Behind");
                            else// (thisDist <= otherDist)
                                return ("In front");
                        }
                        else
                            return ("In front");
                    }
                }
            }
        }
    }


    public void TTLapComplete()
    {
        cars[0].GetComponent<CarController>().itemLevel = 0;
        cars[0].GetComponent<CarController>().currentItem = RaceManager.Item.None;
        cars[0].GetComponent<CarController>().UpdateItemUI("Shrink");


        if (lapTime < trackProfile.trackGhost.lapTime || trackProfile.trackGhost.lapTime == 0)
        {
            SaveManager.Instance.SaveRace(0, lapTime);

            if (ttGhostRecord.ghost.timeStamp.Count < 1500)
                ttGhostRecord.SaveGhost(trackProfile.trackGhost, lapTime);
            else
                Debug.Log("Unable to save ghost, too long");
        }

        ttGhostRecord.ghost.Clear();
        ttGhostRecord.timeValue = 0;
        ttGhostRecord.timer = 0;

        if (currentTTGhostCar != null)
            Destroy(currentTTGhostCar);

        TTSpawnCar();
    }

    public void TTSpawnCar()
    {
        float minutes = Mathf.FloorToInt(trackProfile.trackGhost.lapTime / 60);
        float seconds = Mathf.FloorToInt(trackProfile.trackGhost.lapTime % 60);
        float milliSeconds = (trackProfile.trackGhost.lapTime % 1) * 100;
        bestLapText.text = string.Format("{0:00}:{1:00}:{2:00}", minutes, seconds, milliSeconds);

        if (lapTimeText.enabled == false)
        {
            lapTimeText.enabled = true;
            lapTime = 0;
        }

        if (trackProfile.trackGhost.timeStamp.Count != 0)
        {
            GameObject spawnedGhostCar = Instantiate(ttGhostCar, new Vector3(0, -10, 0), transform.rotation, null);
            spawnedGhostCar.GetComponent<TTGhostReplay>().ghost = trackProfile.trackGhost;
            currentTTGhostCar = spawnedGhostCar;
        }
    }

    private void UpdatePositionText(CarController car)
    {
        if (car.carPosition != positionShown)
        {
            positionShown = car.carPosition;
            positionText.text = (car.carPosition.ToString() + "   ");
            if (car.carPosition == 1)
            {
                positionSuffixText.text = " st";
                positionText.colorGradientPreset = positionTextGradients[0];
                positionSuffixText.colorGradientPreset = positionTextGradients[0];
            }
            else if (car.carPosition == 2)
            {
                positionSuffixText.text = " nd";
                positionText.colorGradientPreset = positionTextGradients[1];
                positionSuffixText.colorGradientPreset = positionTextGradients[1];
            }
            else if (car.carPosition == 3)
            {
                positionSuffixText.text = " rd";
                positionText.colorGradientPreset = positionTextGradients[2];
                positionSuffixText.colorGradientPreset = positionTextGradients[2];
            }
            else if (car.carPosition == 4 || car.carPosition == 5 || car.carPosition == 6)
            {
                positionSuffixText.text = " th";
                positionText.colorGradientPreset = positionTextGradients[3];
                positionSuffixText.colorGradientPreset = positionTextGradients[3];
            }
            positionText.transform.parent.GetComponent<Animator>().SetTrigger("Change");
        }
    }

    private void SpawnTrack()
    {
        gardenTrack.gameObject.SetActive(false);
        beachTrack.gameObject.SetActive(false);
        GameObject trackToSpawn = gardenTrack;

        if (currentTrack == Track.Garden)
            trackToSpawn = gardenTrack;
        else if (currentTrack == Track.Beach)
            trackToSpawn = beachTrack;

        trackToSpawn.gameObject.SetActive(true);
        trackProfile = trackToSpawn.GetComponent<TrackProfile>();

        if (Random.Range(0, 11) >= 7)
        {
            RenderSettings.skybox = nightSkybox;
            RenderSettings.ambientIntensity = -8;
        }
        else
        {
            RenderSettings.skybox = daySkybox;
            RenderSettings.ambientIntensity = 0;
        }
    }

    public void SpawnRaceCars()
    {
        if (carsInRace < 6 || currentMode == Mode.TimeTrials)
            Destroy(trackProfile.cars[5]);
        if (carsInRace < 5 || currentMode == Mode.TimeTrials)
            Destroy(trackProfile.cars[4]);
        if (carsInRace < 4 || currentMode == Mode.TimeTrials)
            Destroy(trackProfile.cars[3]);
        if (carsInRace < 3 || currentMode == Mode.TimeTrials)
            Destroy(trackProfile.cars[2]);
        if (carsInRace < 2 || currentMode == Mode.TimeTrials)
            Destroy(trackProfile.cars[1]);

        if (mirrorMode)
            trackProfile.playerCarController.mirrorMode = true;
    }

    public IEnumerator StartCountdown()
    {
        TMPro.TextMeshProUGUI countdown = countdownText.GetComponent<TMPro.TextMeshProUGUI>();

        for (int i = 0; i < carsInRace; i++)
        {
            cars[i].GetComponent<CarController>().allowedToDrive = false;
        }

        yield return new WaitForSeconds(5);

        countdown.text = "3";
        countdownAnim.SetTrigger("Change");
        SoundManager.Instance.PlaySound(countdownAudio, 0.8f);
        yield return new WaitForSeconds(1);
        countdown.text = "2";
        countdownAnim.SetTrigger("Change");
        SoundManager.Instance.PlaySound(countdownAudio, 0.8f);
        yield return new WaitForSeconds(0.8f);

        cars[0].GetComponent<CarController>().boostStartTime = true;
        yield return new WaitForSeconds(0.2f);

        countdown.text = "1";
        countdownAnim.SetTrigger("Change");
        SoundManager.Instance.PlaySound(countdownAudio, 0.8f);

        if (currentMode != Mode.TimeTrials && carsInRace > 1)
        {
            for (int i = 1; i < carsInRace; i++)  // AI Boost start
            {
                if (Random.Range(0, 10) > 4)
                {
                    cars[i].GetComponent<CarController>().boostStart = true;
                    cars[i].GetComponent<CarController>().boostStartParticles.gameObject.SetActive(true);
                }
                else
                    cars[i].GetComponent<CarController>().startSmokeParticles.gameObject.SetActive(true);
            }
        }

        yield return new WaitForSeconds(0.2f);
        cars[0].GetComponent<CarController>().boostStartTime = false;

        yield return new WaitForSeconds(0.8f);

        SoundManager.Instance.PlaySound(raceStartAudio, 0.8f);
        countdownAnim.SetBool("Countdown", true);
        countdownAnim.SetTrigger("Grow");
        countdown.text = "Go!";

        racing = true;
        for (int i = 0; i < carsInRace; i++)
        {
            cars[i].GetComponent<CarController>().allowedToDrive = true;
        }

        SoundManager.Instance.gameObject.GetComponent<AudioSource>().enabled = true;
        SoundManager.Instance.gameObject.GetComponent<AudioSource>().clip = trackProfile.lap1Music;
        if (SoundManager.Instance.muteMusic == false)
            SoundManager.Instance.gameObject.GetComponent<AudioSource>().volume = trackProfile.musicVolume;
        else
            SoundManager.Instance.gameObject.GetComponent<AudioSource>().volume = 0;
        SoundManager.Instance.gameObject.GetComponent<AudioSource>().Play();

        if (currentTrack == Track.Garden)
            StartCoroutine(MusicIntro());

        racePanel.SetActive(true);

        yield return new WaitForSeconds(1);

        countdown.CrossFadeAlpha(0, 1, false);
    }

    public void FinishRace()
    {
        itemBox.SetActive(false);
        racePositionBox.SetActive(true);
        SoundManager.Instance.PlaySound(raceFinishAudio, 0.8f);
        SoundManager.Instance.gameObject.GetComponent<AudioSource>().volume = 0;
        racing = false;

        for (int i = 0; i < carsInRace; i++)
        {
            racePositions[i].SetActive(true);
            GameObject carToSpawn = null;


            for (int x = 0; x < carsInRace; x++)
            {
                if (cars[x].GetComponent<CarController>().carPosition == i + 1)
                {
                    carToSpawn = cars[x].GetComponent<CarController>().spawnedCarPrefab;
                }
            }


            GameObject car = Instantiate(carToSpawn, transform.position, transform.rotation, racePositions[i].transform);
            CarProfile profile = car.GetComponent<CarProfile>();
            car.transform.localScale = new Vector3(20, 20, 20);
            car.transform.localPosition = new Vector3(5, -15, -100);
            car.transform.localRotation = new Quaternion(-0.07f, -0.86f, 0.14f, 0.48f);

            profile.carCollisionRB.transform.gameObject.SetActive(false);

            profile.skidMarkTrail.gameObject.SetActive(false);
            profile.airTrail.gameObject.SetActive(false);
            profile.offroadTrail.gameObject.SetActive(false);
        }


        if (cars[0].GetComponent<CarController>().aiControlled)
        {
            if (cars[0].GetComponent<CarController>().carPosition == 1)
                finishTrophy.GetComponent<Renderer>().material = trophyColours[0];
            else if (cars[0].GetComponent<CarController>().carPosition == 2)
                finishTrophy.GetComponent<Renderer>().material = trophyColours[1];
            else if (cars[0].GetComponent<CarController>().carPosition == 3)
                finishTrophy.GetComponent<Renderer>().material = trophyColours[2];
        }


        if(cars[0].GetComponent<CarController>().carPosition <= 3)
            finishTrophy.transform.parent.gameObject.SetActive(true);


        if (cars[0].GetComponent<CarController>().carPosition == 1)
            SaveManager.Instance.SaveRace(3, 0);
        if (cars[0].GetComponent<CarController>().carPosition == 2)
            SaveManager.Instance.SaveRace(2, 0);
        if (cars[0].GetComponent<CarController>().carPosition == 3)
            SaveManager.Instance.SaveRace(1, 0);


        StartCoroutine(ReturnToMenu(12));
    }



    void EliminationCountdown()
    {
        eliminationCountdown -= Time.deltaTime;

        if (eliminationCountdown <= 0)
        {
            eliminationCountdown = carsLeftInRace * 10;
            StartCoroutine(EliminatePlayer());
        }

        if (cars[0].GetComponent<CarController>().carPosition == carsLeftInRace)
            eliminationText.colorGradientPreset = positionTextGradients[4];
        else
            eliminationText.colorGradientPreset = positionTextGradients[3];

        eliminationText.text = carsLeftInRace + eliminationPositionSuffix + " Place - " + Mathf.FloorToInt(eliminationCountdown % 60); ;
    }

    private IEnumerator EliminatePlayer()
    {
        for (int x = 0; x < carsInRace; x++)
        {
            if (cars[x].GetComponent<CarController>().carPosition == carsLeftInRace)
            {
                CarController car = cars[x].GetComponent<CarController>();
                car.destroyCarParticles.Play();
                SoundManager.Instance.PlaySound(car.explosionAudio, 1f);
                car.car.SetActive(false);
                car.spinningOut = true;
                carsLeftInRace -= 1;
                yield return new WaitForSeconds(1);
                car.sphereRB.gameObject.SetActive(false);
                car.carCollisionRB.gameObject.SetActive(false);
                car.turnAimer.gameObject.SetActive(false);
                car.gameObject.SetActive(false);

                if (carsLeftInRace == 2)
                    eliminationPositionSuffix = "nd";

                else if (carsLeftInRace == 3)
                    eliminationPositionSuffix = "rd";

                else if (carsLeftInRace == 4 || carsLeftInRace == 5 || carsLeftInRace == 6)
                    eliminationPositionSuffix = "th";


                if (carsLeftInRace == 1 || cars[0].activeSelf == false)
                {
                    eliminationCountdown = Mathf.Infinity;
                    eliminationText.gameObject.SetActive(false);
                    cars[0].GetComponent<CarController>().aiControlled = true;
                    FinishRace();
                }

                break;
            }
        }
    }

    public void MenuButton()
    {
        StartCoroutine(ReturnToMenu(0));
    }

    public void RestartButton()
    {
        StartCoroutine(ReloadRace());
    }

    private IEnumerator ReturnToMenu(int time)
    {
        if (time == 0)
        {
            for (int i = 0; i < carsInRace; i++)
            {
                cars[i].GetComponent<CarController>().allowedToDrive = false;
            }

            Time.timeScale = 1;
        }

        yield return new WaitForSeconds(time);

        transitionOut.SetActive(true);

        yield return new WaitForSeconds(6);

        SceneManager.LoadScene(0);
    }

    private IEnumerator ReloadRace()
    {
        for (int i = 0; i < carsInRace; i++)
        {
            cars[i].GetComponent<CarController>().allowedToDrive = false;
        }

        Time.timeScale = 1;

        transitionOut.SetActive(true);

        yield return new WaitForSeconds(6);

        SceneManager.LoadScene(1);
    }

    private IEnumerator MusicIntro()
    {
        yield return new WaitForSeconds(trackProfile.musicIntroTime);
        SoundManager.Instance.gameObject.GetComponent<AudioSource>().clip = trackProfile.lap2Music;
        SoundManager.Instance.gameObject.GetComponent<AudioSource>().Play();
    }

    public void NextLapMusic()
    {
        SoundManager.Instance.PlaySound(lapFinishAudio, 1.8f);
        if (currentTrack == Track.Beach)
        {
            SoundManager.Instance.gameObject.GetComponent<AudioSource>().clip = trackProfile.lap2Music;
            SoundManager.Instance.gameObject.GetComponent<AudioSource>().Play();
        }
    }
}
