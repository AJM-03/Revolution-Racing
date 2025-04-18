using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class CarController : MonoBehaviour
{
    public Rigidbody sphereRB, carCollisionRB;
    public Transform turnAimer;
    public float gravityForce, lowOffroadEffect, extremeOffroadEffect;


    private float forwardSpeed, reverseSpeed, accelerationTime, currentAcceleration, turnStrength, offroad;
    private float speedInput, dragOnGround;
    [HideInInspector] public float currentOffroadEffect;

    [HideInInspector] public bool allowedToDrive = false, grounded, respawning;
    [HideInInspector] public GameObject car;
    private CarProfile profile;
    private bool landingAudio;
    public GameObject[] carsInRace;

    [Header("Ground Check")]
    public float groundRayLength = 0.5f;
    public Transform groundRayPoint;
    public LayerMask groundLayer;

    [Header("Wheels")]
    private Transform lfWheel, rfWheel, lbWheel, rbWheel;
    public float wheelSpinSpeed;
    public float maxWheelTurn;
    private bool turningLeft, turningRight, flipLWheel, flipRWheel;

    public GameObject[] cars;
    [HideInInspector] public GameObject spawnedCarPrefab;
    private AudioSource motorAudio;

    [Header("Lap")]
    public GameObject respawnPoints;
    public Transform nextCheckpoint;
    public int lapNumber, keyCheckpointIndex, checkpointIndex, carPosition, carNumber;
    [HideInInspector] public bool finished, mirrorMode, boostStartTime, boostStart;

    [Header("Items")]
    public RaceManager.Item currentItem = RaceManager.Item.None;
    public int itemLevel;
    public GameObject[] missileItems;
    public GameObject[] mineItems;
    public GameObject[] shieldItems;
    private float spinOutSlowdown;
    [HideInInspector] public GameObject currentShield;

    [Header("AI")]
    public bool aiControlled, spinningOut;
    public float aiDifficulty;
    public Transform nextAIWaypoint;
    private float aiTurnMultiplier, lastCheckTime;
    private Vector3 lastCheckPosition;

    [Header("Audio")]
    public AudioClip offroadSounds;
    public AudioClip boostingSounds, carLandingAudio, explosionAudio;
    public AudioClip newItemAudio, itemUpgradeAudio, superItemAudio;

    [Header("Particles")]
    public ParticleSystem destroyCarParticles;
    public ParticleSystem spinOutParticles;
    public ParticleSystem startSmokeParticles, boostStartParticles;

    [Header("UI")]
    public Image itemUIColour;
    public GameObject[] boostUIImages;
    public GameObject[] missileUIImages;
    public GameObject[] mineUIImages;
    public GameObject[] shieldUIImages;
    public ParticleSystem itemUpgradeParticles;
    public GameObject aimArrow;

    public InputActionAsset inputMap;
    private Vector2 steerInput = Vector2.zero;
    private bool accelInput, reverseInput, directionInputting, boosting;

    private bool paused;
    public GameObject pauseMenu;


    void Start()
    {
        sphereRB.transform.parent = null;
        turnAimer.transform.parent = null;
        respawning = false;
        dragOnGround = sphereRB.drag;
        currentAcceleration = 0;
        lapNumber = 1;
        itemLevel = 0;
        keyCheckpointIndex = 0;
        finished = false;
        landingAudio = false;

        motorAudio = gameObject.GetComponent<AudioSource>();
        if (!aiControlled)  // Makes your car louder if you are a player
            motorAudio.volume = 0.225f;
        else
            motorAudio.volume = 0.1f;

        if (!aiControlled)  // Enables the input system if you are a player
            gameObject.GetComponent<PlayerInput>().enabled = true;
        else if (aiControlled && gameObject.GetComponent<PlayerInput>())
            gameObject.GetComponent<PlayerInput>().enabled = false;

        CreateCar();
    }


    void Update()
    {
        if (allowedToDrive && !aiControlled)
        {
            if (boostStart)  // When the race starts, if boost start is true then do it
                StartCoroutine(BoostStart());

            if (boosting && accelInput)  // Increase speed for boost
                speedInput = (forwardSpeed + 5) * 1000f;

            else if (spinningOut)  // Car has been hit by an item
            {
                car.transform.Rotate(0, Random.Range(450, 700) * Time.deltaTime, 0);  // Spin the car around
                if (currentAcceleration > 0) currentAcceleration = currentAcceleration - (spinOutSlowdown / accelerationTime * Time.deltaTime);  // Slow down with the slowdown variable
                speedInput = forwardSpeed * 500f / currentOffroadEffect * currentAcceleration;
            }

            else
            {
                if (accelInput && speedInput >= -750)  // Accelerate
                {
                    if (currentAcceleration < 1 && grounded)
                        currentAcceleration = currentAcceleration + (10 / accelerationTime * (1 - currentAcceleration) * Time.deltaTime);

                    float rubberbanding = (carPosition / 35.01f) + 1;  // Change speed depending on position

                    speedInput = forwardSpeed * 1000f / currentOffroadEffect * currentAcceleration * rubberbanding;
                }

                else if (reverseInput && speedInput <= 500)  // Reverse
                {
                    if (currentAcceleration < 1 && grounded)
                        currentAcceleration = currentAcceleration + (10 / accelerationTime * (1 - currentAcceleration) * Time.deltaTime);

                    speedInput = -reverseSpeed * 1000f / currentOffroadEffect * currentAcceleration;
                }

                else if (((accelInput && speedInput < 0) || (reverseInput && speedInput > 0)) && currentAcceleration > 0 && grounded)  // Stop quickly
                {
                    currentAcceleration = currentAcceleration - (15 / accelerationTime * Time.deltaTime);
                    if (speedInput > 0)
                        speedInput = forwardSpeed * 400f / currentOffroadEffect * currentAcceleration;
                    else if (speedInput < 0)
                        speedInput = -reverseSpeed * 250f / currentOffroadEffect * currentAcceleration;
                }

                else if (currentAcceleration > 0 && grounded)  // Slow deccelerate
                {
                    currentAcceleration = currentAcceleration - (5 / accelerationTime * Time.deltaTime);
                    if (speedInput > 0)
                        speedInput = forwardSpeed * 500f / currentOffroadEffect * currentAcceleration;
                    else if (speedInput < 0)
                        speedInput = -reverseSpeed * 250f / currentOffroadEffect * currentAcceleration;
                }

                else
                    speedInput = 0f;
            }



            if (directionInputting && grounded && sphereRB.velocity.magnitude > 4 && steerInput != Vector2.zero && currentAcceleration > 0.01f)  // If steering
                TurnCar();
            else  // If not steering
            {
                turningLeft = false;
                turningRight = false;
            }



            if (lfWheel != null && profile.dontTurnWheels == false)
                RotateWheels();



            transform.position = sphereRB.transform.position;  // Moves the car body to the sphere's position
            turnAimer.position = transform.position;  // Moves the turn aimer to the sphere's position
            carCollisionRB.MoveRotation(transform.rotation);  // Moves the collision to the sphere's position
        }

        else if (allowedToDrive && aiControlled)  // If it is an AI car
            AIUpdate();

        else if (!allowedToDrive && !aiControlled)  // If the race hasn't started and you are a player
        {
            if (boostStart && accelInput && motorAudio.pitch != 1.2)  // Charged, Still inputting
            {
                boostStartParticles.gameObject.SetActive(true);
                startSmokeParticles.gameObject.SetActive(false);
                motorAudio.pitch = 1.2f;
                motorAudio.volume = 0.13f;
            }

            else if (!boostStart && accelInput && motorAudio.pitch != 0.8)  // Not charged, inputting
            {
                boostStartParticles.gameObject.SetActive(false);
                startSmokeParticles.gameObject.SetActive(true);
                motorAudio.pitch = 0.8f;
                motorAudio.volume = 0.12f;
            }

            else if (!boostStart && !accelInput && motorAudio.pitch != 0)  // Not charged, not inputting
            {
                boostStartParticles.gameObject.SetActive(false);
                startSmokeParticles.gameObject.SetActive(false);
                motorAudio.pitch = 0;
                motorAudio.volume = 0.175f;
            }

            else if (boostStart && !accelInput)  // Charged but not inputting
            {
                boostStart = false;
                motorAudio.volume = 0.175f;
            }
        }

    }


    private void FixedUpdate()
    {
        if (allowedToDrive)
        {
            grounded = false;
            RaycastHit hit;

            if (Physics.Raycast(groundRayPoint.position, -transform.up, out hit, groundRayLength, groundLayer))  // If raycast hits the ground
            {
                if (hit.transform.gameObject.CompareTag("Sticky Ground") || hit.normal.y > 0.1f)  // Sticky Road or not vertical
                {
                    if (landingAudio == true) SoundManager.Instance.PlaySound(carLandingAudio, 0.6f);  // Play landing sound if you have been in the air long enough

                    grounded = true;
                    landingAudio = false;
                    transform.rotation = Quaternion.FromToRotation(transform.up, hit.normal) * transform.rotation;  // Rotate the car to the ground normal

                    if (currentShield != null && currentShield.GetComponent<Shield>().invincible == true)  // If you have a L3 shield, don't slow down offroad
                        currentOffroadEffect = 1;


                    else if (hit.transform.gameObject.CompareTag("Low Offroad"))  // Slow down offroad
                    {
                        currentOffroadEffect = lowOffroadEffect / (offroad / 10);  // Slow down
                        startSmokeParticles.gameObject.SetActive(true);  // Play particles
                        if (motorAudio.clip == profile.motorSounds)  // Change engine sound
                        {
                            motorAudio.clip = offroadSounds;
                            motorAudio.Play();
                        }
                    }

                    else if (hit.transform.gameObject.CompareTag("Extreme Offroad"))  // Slow down a lot offroad
                    {
                        currentOffroadEffect = extremeOffroadEffect / (offroad / 10);  // Slow down
                        startSmokeParticles.gameObject.SetActive(true);  // Play particles
                        if (motorAudio.clip == profile.motorSounds)  // Change engine sound
                        {
                            motorAudio.clip = offroadSounds;
                            motorAudio.Play();
                        }
                    }

                    else  // Normal Road
                    {
                        currentOffroadEffect = 1;
                        startSmokeParticles.gameObject.SetActive(false);
                        if (motorAudio.clip == offroadSounds)  // Play normal engine sounds
                        {
                            motorAudio.clip = profile.motorSounds;
                            motorAudio.Play();
                        }
                    }
                }
            }


            if (grounded)  // If grounded
            {
                sphereRB.drag = dragOnGround;  // Set the physics drag

                if (Mathf.Abs(speedInput) > 0 && speedInput != Mathf.Infinity)
                {
                    sphereRB.AddForce(transform.forward * speedInput);  // Move the car's sphere with the ammount calculated in update ^
                }

                if (!boosting)  // If not currently boosting
                    motorAudio.pitch = currentAcceleration;
            }


            else  // If not grounded
            {
                sphereRB.drag = 0.01f;  // No drag in the air
                sphereRB.AddForce(Vector3.up * -gravityForce * 100);  // Apply gravity when in the air


                Vector3 currentAngle = transform.eulerAngles;  // Fix rotation in the air
                currentAngle = new Vector3(Mathf.LerpAngle(currentAngle.x, 0, Time.deltaTime), currentAngle.y, Mathf.LerpAngle(currentAngle.z, 0, Time.deltaTime));
                transform.eulerAngles = currentAngle;

                startSmokeParticles.gameObject.SetActive(false);

                if (!boosting) motorAudio.pitch = 0;  // Mute motor if not boosting

                if ((Time.time - lastCheckTime) > 1 && !aiControlled && !respawning)  // Repeat every few seconds
                {
                    if ((transform.position - lastCheckPosition).magnitude < 1.5f)  // If the car gets stuck somewhere
                        StartCoroutine(RespawnCar());
                    lastCheckPosition = transform.position;
                    lastCheckTime = Time.time;
                    landingAudio = true;  // Play car landing sound when it hits the ground
                }
            }
        }
    }



    private void TurnCar()
    {
        Vector3 turnAim = new Vector3(0, 0, 0);

        if (!mirrorMode)
            turnAim = new Vector3(steerInput.x, 0, steerInput.y);  // Work out the direction inputted
        else
            turnAim = new Vector3(-steerInput.x, 0, steerInput.y);  // Flip the direction inputted

        if (accelInput)
            turnAimer.forward = turnAim;  // Rotate the aimer
        else if (reverseInput)
            turnAimer.forward = -turnAim;


        Quaternion lookRotation = Quaternion.LookRotation(turnAimer.GetChild(0).transform.position - transform.position);
        transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, turnStrength * Time.deltaTime / 6);  // Turn the car




        if (transform.rotation.y * 10 < (lookRotation.y * 10) + 1 && transform.rotation.y * 10 > (lookRotation.y * 10) - 1)  // If already reached direction of aimer
        {
            turningLeft = false;
            turningRight = false;
        }

        else if (Vector3.Dot(transform.forward, -Vector3.forward) < 0)  // If pointing north
        {
            if (transform.position.x < turnAimer.GetChild(0).transform.position.x)  // Turning left
            {
                turningLeft = true;
                turningRight = false;
            }
            else if (transform.position.x > turnAimer.GetChild(0).transform.position.x)  // Turning right
            {
                turningLeft = false;
                turningRight = true;
            }
        }

        else if (Vector3.Dot(transform.forward, -Vector3.forward) > 0)  // If pointing south
        {
            if (transform.position.x > turnAimer.GetChild(0).transform.position.x)  // Turning left
            {
                turningLeft = true;
                turningRight = false;
            }
            else if (transform.position.x < turnAimer.GetChild(0).transform.position.x)  // Turning right
            {
                turningLeft = false;
                turningRight = true;
            }
        }
    }



    public void TurnInput(InputAction.CallbackContext context)  // WASD / Stick input
    {
        if (!aiControlled)
            steerInput = context.ReadValue<Vector2>();
    }

    public void AccelInput(InputAction.CallbackContext context)  // Acellerate button input
    {
        if (context.performed && !aiControlled)
            accelInput = true;
        if (context.canceled && !aiControlled)
            accelInput = false;


        if (context.performed && !aiControlled && boostStartTime)  // If button is pressed on 1
            boostStart = true;

        else if (context.performed && !aiControlled && !boostStartTime)
            boostStart = false;
    }

    public void ReverseInput(InputAction.CallbackContext context)  // Reverse button input
    {
        if (context.performed && !aiControlled)
            reverseInput = true;
        if (context.canceled && !aiControlled)
            reverseInput = false;
    }

    public void ItemInput(InputAction.CallbackContext context)  // Item button input
    {
        if (!aiControlled)
            UseItem();
    }

    public void RespawnInput(InputAction.CallbackContext context)  // Respawn button input
    {
        if (!aiControlled && !respawning && allowedToDrive && respawnPoints != null)
            StartCoroutine(RespawnCar());
    }

    public void DirectionInputting(InputAction.CallbackContext context)  // If the player is inputting with WASD or the Stick
    {
        if (context.performed && !aiControlled)
        {
            directionInputting = true;
            aimArrow.SetActive(true);
        }
        if (context.canceled && !aiControlled)
        {
            directionInputting = false;
            aimArrow.SetActive(false);
        }
    }



    private void RotateWheels()
    {
        if (turningLeft)  // Turn wheels to the left
        {
            lfWheel.localRotation = Quaternion.Euler(lfWheel.localRotation.eulerAngles.x, maxWheelTurn, lfWheel.localRotation.eulerAngles.z);
            if (flipLWheel)
                lfWheel.localRotation = Quaternion.Euler(lfWheel.localRotation.eulerAngles.x, maxWheelTurn + 180, lfWheel.localRotation.eulerAngles.z);
            rfWheel.localRotation = Quaternion.Euler(rfWheel.localRotation.eulerAngles.x, maxWheelTurn, rfWheel.localRotation.eulerAngles.z);
            if (flipRWheel)
                rfWheel.localRotation = Quaternion.Euler(rfWheel.localRotation.eulerAngles.x, maxWheelTurn + 180, rfWheel.localRotation.eulerAngles.z);
        }

        else if (turningRight)  // Turn wheels to the right
        {
            lfWheel.localRotation = Quaternion.Euler(lfWheel.localRotation.eulerAngles.x, -maxWheelTurn, lfWheel.localRotation.eulerAngles.z);
            if (flipLWheel)
                lfWheel.localRotation = Quaternion.Euler(lfWheel.localRotation.eulerAngles.x, -maxWheelTurn + 180, lfWheel.localRotation.eulerAngles.z);
            rfWheel.localRotation = Quaternion.Euler(rfWheel.localRotation.eulerAngles.x, -maxWheelTurn, rfWheel.localRotation.eulerAngles.z);
            if (flipRWheel)
                rfWheel.localRotation = Quaternion.Euler(rfWheel.localRotation.eulerAngles.x, -maxWheelTurn + 180, rfWheel.localRotation.eulerAngles.z);
        }

        else  // Straighten wheels
        {
            lfWheel.localRotation = Quaternion.Euler(lfWheel.localRotation.eulerAngles.x, 0, lfWheel.localRotation.eulerAngles.z);
            if (flipLWheel)
                lfWheel.localRotation = Quaternion.Euler(lfWheel.localRotation.eulerAngles.x, 180, lfWheel.localRotation.eulerAngles.z);
            rfWheel.localRotation = Quaternion.Euler(rfWheel.localRotation.eulerAngles.x, 0, rfWheel.localRotation.eulerAngles.z);
            if (flipRWheel)
                rfWheel.localRotation = Quaternion.Euler(rfWheel.localRotation.eulerAngles.x, 180, rfWheel.localRotation.eulerAngles.z);
        }


        if (grounded && accelInput && lbWheel != null && rbWheel != null)  // Back wheels spin while driving forwards
        {
            lbWheel.Rotate(wheelSpinSpeed, 0f, 0f, Space.Self);
            rbWheel.Rotate(wheelSpinSpeed, 0f, 0f, Space.Self);
        }  

        else if (grounded && reverseInput && lbWheel != null && rbWheel != null)  // Back wheels spin while reversing
        {
            lbWheel.Rotate(-wheelSpinSpeed / 0.5f, 0f, 0f, Space.Self);
            rbWheel.Rotate(-wheelSpinSpeed / 0.5f, 0f, 0f, Space.Self);
        }
    }



    public IEnumerator RespawnCar()
    {
        if (respawnPoints != null && !spinningOut)
        {
            respawning = true;
            List<Transform> points = new List<Transform>();
            foreach (Transform child in respawnPoints.transform)  // Create a list of possible respawn points
            {
                points.Add(child.transform);
            }

            Transform chosenPoint = points[Random.Range(0, points.Count)];  // Choose a random respawn point
            SoundManager.Instance.PlaySound(explosionAudio, 0.7f);
            destroyCarParticles.Play();
            allowedToDrive = false;
            car.SetActive(false);  // Hide the car's body

            yield return new WaitForSeconds(2);  // Respawn time

            sphereRB.transform.position = chosenPoint.position + new Vector3(0, 0.3f, 0);  // Move the car to the position
            transform.rotation = chosenPoint.rotation;
            currentAcceleration = 0;
            allowedToDrive = true;
            car.SetActive(true);  // Show the car's body and let you move

            yield return new WaitForSeconds(2);
            respawning = false;
        }
    }


    public void HitByItem(int strength, float slowdown, bool explode, bool tornado)  // Can't call a coroutine from another script
    {
        if (currentShield == null)
            StartCoroutine(SpinOut(strength, slowdown, explode, tornado));
    }

    private IEnumerator SpinOut(int strength, float slowdown, bool explode, bool tornado)
    {
        spinningOut = true;
        spinOutSlowdown = slowdown;
        boosting = false;
        spinOutParticles.gameObject.SetActive(true);
        if (explode)  // If the item explodes
        {
            destroyCarParticles.Play();
            SoundManager.Instance.PlaySound(explosionAudio, 0.7f);
        }
        if (tornado)  // If the item was a tornado
        {
            transform.rotation = Quaternion.Euler(0, Random.Range(-360, 360), 0);  // Spin the car
            car.transform.localPosition += new Vector3(0, 1.5f, 0);  // Lift the car up in the air
            profile.skidMarkTrail.gameObject.SetActive(false);
        }
        yield return new WaitForSeconds(strength);  // How long you spin out for

        spinningOut = false;
        car.transform.Rotate(0, 2.5f, 0);
        car.transform.localRotation = new Quaternion(0, 0, 0, 1);
        spinOutParticles.gameObject.SetActive(false);

        if (tornado)  // If the item was a tornado
        {
            Quaternion lookRotation = Quaternion.LookRotation(nextCheckpoint.position - transform.position);  // Aim in the right direction
            transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, 0.1f);  // Turn the car
            sphereRB.transform.position = car.transform.position;
            car.transform.localPosition += new Vector3(0, -1.5f, 0);  // Drop the car
            profile.skidMarkTrail.gameObject.SetActive(true);
        }
    }

    private void UseItem()
    {
        if (!spinningOut)
        {
            if (currentItem == RaceManager.Item.Boost)
                StartCoroutine(BoostItem(1f * itemLevel));

            else if (currentItem == RaceManager.Item.Missile)
                MissileItem();

            else if (currentItem == RaceManager.Item.Mine && grounded)
                MineItem();

            else if (currentItem == RaceManager.Item.Shield)
                ShieldItem();
        }
    }

    private IEnumerator BoostItem(float time)
    {
        boosting = true;

        if (itemLevel == 1)
        {
            time = time * 1.15f;  // Level 1 boost buff
            profile.boostFireEffect[0].SetActive(true);  // Show orange particles
            if (profile.boostFireEffect[3] != null) profile.boostFireEffect[3].SetActive(true);
        }
        else if (itemLevel == 2)
        {
            profile.boostFireEffect[1].SetActive(true);  // Show blue particles
            if (profile.boostFireEffect[4] != null) profile.boostFireEffect[4].SetActive(true);
        }
        else if (itemLevel == 3)
        {
            profile.boostFireEffect[2].SetActive(true);  // Show purple particles
            if (profile.boostFireEffect[5] != null) profile.boostFireEffect[5].SetActive(true);
        }

        motorAudio.clip = boostingSounds;
        motorAudio.Play();
        motorAudio.pitch = 1;

        itemLevel = 0;
        currentItem = RaceManager.Item.None;
        UpdateItemUI("Shrink");

        yield return new WaitForSeconds(time);  // Wait until boost ends

        boosting = false;

        motorAudio.clip = profile.motorSounds;
        motorAudio.Play();

        for (int x = 0; x < profile.boostFireEffect.Length; x++)
        {
            profile.boostFireEffect[x].SetActive(false);  // Disable all boost particles
        }
    }


    private void MissileItem()
    {
        GameObject missile = Instantiate(missileItems[itemLevel - 1], transform.position, transform.rotation, null);
        GameObject newTarget = null;
        missile.GetComponent<Missile>().sender = gameObject;
        if (nextCheckpoint != null) missile.GetComponent<Missile>().GetNextWaypoint(nextCheckpoint.GetComponent<Checkpoint>().respawnPoints, 5f);  // Find the next checkpoint
        int level = itemLevel;
        
        itemLevel = 0;
        currentItem = RaceManager.Item.None;
        UpdateItemUI("Shrink");

        for (int x = 0; x < carsInRace.Length; x++)
        {
            if (carsInRace[x] != null && carsInRace[x].GetComponent<CarController>().carPosition == carPosition - 1 && level == 2)  // Find the car in front of you
                newTarget = carsInRace[x];

            if (carsInRace[x] != null && carsInRace[x].GetComponent<CarController>().carPosition == 1 && level == 3)  // Find the car in 1st place
                newTarget = carsInRace[x];
        }

        if (carPosition == 1 && level == 3)  // If L3 and you are in 1st
            newTarget = null;

        missile.GetComponent<Missile>().target = newTarget;
    }


    private void MineItem()
    {
        GameObject mine = Instantiate(mineItems[itemLevel - 1], transform.position, transform.rotation, null);  // Instantiate the mine
        if (itemLevel == 1 || itemLevel == 3) mine.GetComponent<Mine>().sender = gameObject;
        else if (itemLevel == 2) mine.transform.GetComponentInChildren<Mine>().sender = gameObject;
        
        itemLevel = 0;
        currentItem = RaceManager.Item.None;
        UpdateItemUI("Shrink");
    }


    private void ShieldItem()
    {
        GameObject shield = Instantiate(shieldItems[itemLevel - 1], transform.position, transform.rotation, transform);  // Instatiate the shield
        shield.GetComponent<Shield>().sender = gameObject;
        currentShield = shield;  // Set the car's shield
        
        itemLevel = 0;
        currentItem = RaceManager.Item.None;
        UpdateItemUI("Shrink");
    }


    private void CreateCar()  // Spawn the car body at the start of a race
    {
        if (!aiControlled && MenuManager.Instance.chosenCar != null)
            spawnedCarPrefab = MenuManager.Instance.chosenCar;  // Use the selected car for player
        else
            spawnedCarPrefab = cars[Random.Range(0, cars.Length)];  // Pick a random car for AI

        car = Instantiate(spawnedCarPrefab, transform.position, transform.rotation, transform);  // Instantiate the car
        profile = car.GetComponent<CarProfile>();


        carCollisionRB = profile.carCollisionRB;  // Set up all of the variables in the car's profile to use later
        lfWheel = profile.lfWheel;
        rfWheel = profile.rfWheel;
        lbWheel = profile.lbWheel;
        rbWheel = profile.rbWheel;
        flipLWheel = profile.flipLWheel;
        flipRWheel = profile.flipRWheel;

        forwardSpeed = profile.forwardSpeed;
        reverseSpeed = profile.reverseSpeed;
        accelerationTime = profile.accelerationTime;
        turnStrength = profile.handling;
        offroad = profile.offroad;

        profile.skidMarkTrail.carScript = this;
        profile.airTrail.carScript = this;
        profile.offroadTrail.carScript = this;

        car.transform.position += profile.spawnOffset;

        carCollisionRB.transform.parent = null;
        carCollisionRB.transform.GetComponent<FixedJoint>().connectedBody = sphereRB;
        carCollisionRB.transform.gameObject.GetComponent<CarCollider>().car = this;

        motorAudio.clip = profile.motorSounds;
        motorAudio.pitch = 0;
        motorAudio.Play();

        if (profile.maxWheelTurn != 0)
            maxWheelTurn = profile.maxWheelTurn;


        if (forwardSpeed == 0 || reverseSpeed == 0 || accelerationTime == 0 || turnStrength == 0 || offroad == 0)
            Debug.LogError("Stats are not filled in on the " + car);
    }






    void AIUpdate()  // Call every frame for an AI car
    {
        if (allowedToDrive)
        {
            if (boostStart)  // If boost start is true as the race starts
                StartCoroutine(BoostStart());

            speedInput = 0f;

            if (boosting)  // If the car is boosting
                speedInput = (forwardSpeed + 7) * 1000f;

            else if (spinningOut)  // If you are spinning out
            {
                car.transform.Rotate(0, Random.Range(450, 700) * Time.deltaTime, 0);  // Spin the car at a random speed
                if (currentAcceleration > 0) currentAcceleration = currentAcceleration - (spinOutSlowdown / accelerationTime * Time.deltaTime);  // Slow the car down
                speedInput = forwardSpeed * 500f / currentOffroadEffect * currentAcceleration;
            }

            else if (currentAcceleration < 1 && grounded)  // Drive forwards
                currentAcceleration = currentAcceleration + (10 / accelerationTime * (1 - currentAcceleration) * Time.deltaTime);

            float rubberbanding = (carPosition / 30.01f) + 1;  // Change speed based on position

            speedInput = forwardSpeed * 1000f / currentOffroadEffect * currentAcceleration * rubberbanding;  // Calculate speed




            if (grounded && sphereRB.velocity.magnitude > 4 && currentAcceleration > 0.01f && nextAIWaypoint != null && !spinningOut)  // If steering
                SteerAICar();
            else  // If not steering
            {
                turningLeft = false;
                turningRight = false;
            }


            if (lfWheel != null && maxWheelTurn != 0)
                RotateWheels();


            if ((Time.time - lastCheckTime) > 1 && !respawning && currentAcceleration > 0.5)  // Check if the car is stuck
            {
                if ((transform.position - lastCheckPosition).magnitude < 5)
                    StartCoroutine(RespawnCar());  // Respawn the car
                lastCheckPosition = transform.position;
                lastCheckTime = Time.time;
            }



            transform.position = sphereRB.transform.position;  // Move the car body to the sphere
            carCollisionRB.MoveRotation(transform.rotation);  // Move the collision to the sphere
        }
    }



    void SteerAICar()
    {
        Quaternion lookRotation = Quaternion.LookRotation(nextAIWaypoint.position - transform.position);  // Aim towards the chosen waypoint
        transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, turnStrength * Time.deltaTime / 2 * aiTurnMultiplier);  // Turn the car


        if (transform.rotation.y * 10 < (lookRotation.y * 10) + 1 && transform.rotation.y * 10 > (lookRotation.y * 10) - 1)  // If already reached direction of aimer
        {
            turningLeft = false;
            turningRight = false;
        }

        else if (Vector3.Dot(transform.forward, -Vector3.forward) < 0)  // If pointing north
        {
            if (transform.position.x < turnAimer.GetChild(0).transform.position.x)  // Turning left
            {
                turningLeft = true;
                turningRight = false;
            }
            else if (transform.position.x > turnAimer.GetChild(0).transform.position.x)  // Turning right
            {
                turningLeft = false;
                turningRight = true;
            }
        }

        else if (Vector3.Dot(transform.forward, -Vector3.forward) > 0)  // If pointing south
        {
            if (transform.position.x > turnAimer.GetChild(0).transform.position.x)  // Turning left
            {
                turningLeft = true;
                turningRight = false;
            }
            else if (transform.position.x < turnAimer.GetChild(0).transform.position.x)  // Turning right
            {
                turningLeft = false;
                turningRight = true;
            }
        }
    }

    public IEnumerator AIPowerup()
    {
        yield return new WaitForSeconds(Random.Range(1, 6));  // Wait a random ammount of time after passing a checkpoint
        if (currentItem != RaceManager.Item.None)
        {
            float rand = Random.Range(0, 10);

            if (rand > 7)  // Car has a 6/10 chance to use the item
                UseItem();
        }
    }

    public void GetNextWaypoint(GameObject waypoints, float turnMultiplier, bool useItem)
    {
        if (waypoints == null)
            Debug.LogError("Waypoints missing");

        else if (aiControlled)
        {
            List<Transform> options = new List<Transform>();
            foreach (Transform child in waypoints.transform)  // Create a list of possible waypoints
            {
                options.Add(child.transform);
            }

            nextAIWaypoint = options[Random.Range(0, options.Count)];  // Pick a random waypoint

            aiTurnMultiplier = turnMultiplier;  // Turn faster on some corners
            if (useItem)
                StartCoroutine(AIPowerup());
        }
    }


    private IEnumerator BoostStart()  // If boost start was succesful
    {
        boostStart = false;
        boosting = true;
        currentAcceleration = 1;

        boostStartParticles.gameObject.SetActive(false);
        profile.boostFireEffect[0].SetActive(true);  // Show orange particles
        if (profile.boostFireEffect[3] != null) profile.boostFireEffect[3].SetActive(true);
        motorAudio.clip = boostingSounds;
        motorAudio.Play();
        motorAudio.pitch = 1;


        yield return new WaitForSeconds(0.5f);  // Boost time

        boosting = false;

        motorAudio.clip = profile.motorSounds;
        motorAudio.Play();
        for (int x = 0; x < profile.boostFireEffect.Length; x++)
        {
            profile.boostFireEffect[x].SetActive(false);
        }
    }


    public void UpdateItemUI(string anim)
    {
        if (!aiControlled)
        {
            for (int x = 0; x < boostUIImages.Length; x++)
            {
                boostUIImages[x].SetActive(false);
                missileUIImages[x].SetActive(false);
                mineUIImages[x].SetActive(false);
                shieldUIImages[x].SetActive(false);
            }

            if (currentItem == RaceManager.Item.Boost)
            {
                boostUIImages[itemLevel - 1].SetActive(true);
                itemUIColour.color = new Color32(50, 130, 250, 255);
            }

            else if (currentItem == RaceManager.Item.Missile)
            {
                missileUIImages[itemLevel - 1].SetActive(true);
                itemUIColour.color = new Color32(210, 0, 0, 255);
            }

            else if (currentItem == RaceManager.Item.Mine)
            {
                mineUIImages[itemLevel - 1].SetActive(true);
                itemUIColour.color = new Color32(0, 140, 0, 255);
            }

            else if (currentItem == RaceManager.Item.Shield)
            {
                shieldUIImages[itemLevel - 1].SetActive(true);
                itemUIColour.color = new Color32(180, 0, 210, 255);
            }

            else
                itemUIColour.color = new Color32(128, 128, 128, 255);

            if (anim != null)
                itemUIColour.gameObject.GetComponent<Animator>().SetTrigger(anim);


            if (anim == "Grow" && itemLevel == 1)
                SoundManager.Instance.PlaySound(newItemAudio, 0.65f);
            else if (anim == "Grow" && itemLevel == 2)
                SoundManager.Instance.PlaySound(superItemAudio, 0.65f);
            else if (anim == "Wipe")
                SoundManager.Instance.PlaySound(itemUpgradeAudio, 0.65f);
        }

        else
        {
            if (anim == "Grow" && itemLevel == 1)
                SoundManager.Instance.PlaySound(newItemAudio, 0.2f);
            else if (anim == "Grow" && itemLevel == 2)
                SoundManager.Instance.PlaySound(superItemAudio, 0.2f);
            else if (anim == "Wipe")
                SoundManager.Instance.PlaySound(itemUpgradeAudio, 0.2f);
        }
    }

    public void PauseGame()
    {
        if (!aiControlled && allowedToDrive)
        {
            if (!paused)
            {
                paused = true;
                Time.timeScale = 0;
                pauseMenu.SetActive(true);
                pauseMenu.transform.GetChild(0).GetComponent<Button>().Select();
                SoundManager.Instance.gameObject.GetComponent<AudioSource>().Stop();
            }

            else
            {
                paused = false;
                Time.timeScale = 1;
                pauseMenu.SetActive(false);
                SoundManager.Instance.gameObject.GetComponent<AudioSource>().Play();
            }
        }
    }
}
