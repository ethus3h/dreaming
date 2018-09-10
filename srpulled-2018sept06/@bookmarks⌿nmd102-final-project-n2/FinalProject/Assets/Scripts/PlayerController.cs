using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityStandardAssets.Vehicles.Car;

// TODO: UNDERWATER support

//FIXME: wake generator and nitro are broken; nitro turns on wake generator; camera clipping
public class PlayerController : MonoBehaviour
{
    /* Things that get set per scene */
    public int maxScore = 0;
    public bool allowFlight = false;
    public GameObject startPad;
    public GameObject car;

    /* Things that get set by the prefab */
    public GameObject pcamera;
    public GameObject scoreLabel;
    public GameObject crossHairObject;
    public GameObject bullet;
    public ParticleSystem waterWakeRenderer;
    public ParticleSystem waterSplashRenderer;
    private float ndrag = 1; // drag in normal conditions
    private float nadrag = 1; // angular drag in normal conditions

    /* SETTINGS */
    private bool air2dMovement = true; // set to false to make vertical axis also follow the camera - no UI for this yet: TODO
    private bool iAir2dMovement;

    private bool crossHair = false;
    private bool iCrossHair;

    private bool useFirstPerson = false; // alt. modes only
    private bool iUseFirstPerson;

    private bool glide = false;
    private bool iGlide;

    private float turnSpeed = 0.05f;
    private float iTurnSpeed;
    private float guiTurnSpeed;

    private float cameraDistance = 2f;
    private float iCameraDistance;
    private float guiCameraDistance;

    public float playerSize = 0.1f; // Allow the scene to override the base player size to avoid having to rescale the DreamMountain scenes
    private float nativePlayerSize = 0.1f;
    private float iPlayerSize;
    private float guiPlayerSize;

    private float speed = 1.75f;
    private float iSpeed;
    private float guiSpeed;

    /* Internal values: togglable states */
    private bool paused = false;
    private bool useGravity = true;

    /* Internal values */
    private Rigidbody rb;
    private RaycastHit hit;
    private GameObject groundObject;
    private ContactPoint groundCollisionPoint;
    private GameObject triggerObject;
    private LayerMask CamOcclusion;
    private CarController m_Car;
    private GameObject camTarget;
    private GameObject carBody;
    private GameObject carComponents;

    private float jumpCooldown = 0;
    private float floatForce = 18.0f;
    private float nitroMod = 8;
    private bool helpStatus = false;
    private bool isGrounded;
    private bool inWater;
    private bool levelWon;
    private bool camFollow;
    private bool fireTimeout;
    private int score = 0;
    private int currentScene;
    private int gameMode = 1; // 1 = marble, 2 = auto, 3 = human
    private int mouseMode = 0; // 0 = disabled, 1 = normal, 2 = negative x, 3 = negative y, 4 = negative x and y
    private int kbdModifier = 1;
    private float vDelta = 30;
    private float hDelta;
    private float tSpeed;
    private float fpCameraDistance;
    private float carCameraDistance;
    private float playerCameraDistance;
    private float playerVDelta;
    private float carVDelta;
    private float lastTargetDirection;

    private bool guiCancelling;
    private bool guiResetting;
    private bool guiDefaulting;
    private bool resumeOnHelpClose;

    private AudioSource waterSplash;
    private AudioSource waterMoveSound;
    private AudioSource waterExitSound;
    private AudioSource hitSound;
    private AudioSource jumpSound;
    private AudioSource pickUpSound;
    private AudioSource oobSound;
    private AudioSource victorySound;
    private AudioSource underwaterSound;
    private AudioSource gunShotSound;

    private AudioSource airMoveSound;
    private AudioSource groundMoveSound;

    void Start()
    {
        float sizeScalingFactor = playerSize / nativePlayerSize;
        vDelta = vDelta * sizeScalingFactor;
        cameraDistance = cameraDistance * sizeScalingFactor;
        speed = speed * sizeScalingFactor;

        // Set initial settings values
        iAir2dMovement = air2dMovement;
        iCrossHair = crossHair;
        iUseFirstPerson = useFirstPerson;
        iGlide = glide;

        iTurnSpeed = turnSpeed;
        guiTurnSpeed = turnSpeed;

        iCameraDistance = cameraDistance;
        guiCameraDistance = cameraDistance;

        iPlayerSize = playerSize;
        guiPlayerSize = playerSize;

        iSpeed = speed;
        guiSpeed = speed;

        // Locate audio clips
        foreach (AudioSource aSource in GetComponents<AudioSource>())
        {
            switch(aSource.clip.name) {
                case "water_splash_out":
                    waterSplash = aSource;
                    break;
                case "JustGore_AddOn_Splatter_Splat_014":
                    hitSound = aSource;
                    break;
                case "rain_medium":
                    waterMoveSound = aSource;
                    break;
                case "JustGore_AddOn_Splatter_Splat_070":
                    jumpSound = aSource;
                    break;
                case "Xylo_13":
                    pickUpSound = aSource;
                    break;
                case "Laser_01":
                    oobSound = aSource;
                    break;
                case "JustImpacts_PROCESSED_295":
                    victorySound = aSource;
                    break;
                case "WaterSplash_In_Short5":
                    waterExitSound = aSource;
                    break;
                case "FireExplosion2":
                    gunShotSound = aSource;
                    break;
                case "in_water":
                    underwaterSound = aSource;
                    break;
                case "FlightWind":
                    if (aSource.pitch < 1)
                    {
                        groundMoveSound = aSource;
                    }
                    else
                    {
                        airMoveSound = aSource;
                    }
                    break;
            }
        }

        camTarget = gameObject;
        currentScene = SceneManager.GetActiveScene().buildIndex;
        getCarComponents();
        m_Car = car.GetComponent<CarController>();
        score = 0;
        tSpeed = speed;
        stopCar();
        rb = GetComponent<Rigidbody>();
        rb.drag = ndrag;
        rb.angularDrag = nadrag;
        rb.maxAngularVelocity = 500;
        lastTargetDirection = camTarget.transform.eulerAngles.y;

        scoreLabel.GetComponent<TextMesh>().text = "Finish on the far red pad.";
        crossHairObject.SetActive(crossHair);
        Invoke("ScoreUpdate", 5);
        transform.position = new Vector3(startPad.transform.position.x, startPad.transform.position.y + 10, startPad.transform.position.z);
        paused = false;
        Cursor.lockState = CursorLockMode.Locked;
        airMoveSound.Play();

        playerCameraDistance = cameraDistance;
        carCameraDistance = cameraDistance + 6;
        playerVDelta = vDelta;
        carVDelta = vDelta;
    }

    void Update()
    {
        if (cameraDistance == 0)
        {
            //change distance if it's zero because the direction from the camera to the player can't be calculated if it's 0
            cameraDistance = 0.01f;
        }

        transform.localScale = new Vector3(playerSize, playerSize, playerSize);
        waterSplashRenderer.transform.position = new Vector3(transform.position.x, transform.position.y, transform.position.z);
        waterWakeRenderer.transform.position = transform.position;
        waterSplashRenderer.transform.rotation = Quaternion.Euler(Vector3.zero);
        waterWakeRenderer.transform.rotation = Quaternion.Euler(Vector3.zero);
        var shape = waterSplashRenderer.shape;
        shape.radius = transform.localScale.y;
        shape = waterWakeRenderer.shape;
        shape.radius = (transform.localScale.y / 2);

        rb = camTarget.GetComponent<Rigidbody>();
        if (Input.GetButtonDown("TogglePaused"))
        {
            togglePaused();
        }
        if (Input.GetButtonDown("ToggleGlide"))
        {
            if (allowFlight)
            {
                glide = !glide;
            }
        }
        if (jumpCooldown > 0)
        {
            jumpCooldown -= Time.deltaTime;
        }
        else
        {
            if (Input.GetButton("Jump") && isGrounded)
            {
                Vector3 normal = Vector3.zero;
                normal = -(transform.position - groundCollisionPoint.point).normalized;
                float mult = 100;
                if(allowFlight)
                {
                    mult = mult * 1.75f;
                }
                normal.x = -System.Math.Min(mult * normal.x, 1000f);
                normal.y = 200f;
                normal.z = -System.Math.Min(mult * normal.z, 1000f);
                rb.AddForce(normal);
                jumpSound.Play();
                jumpCooldown = 0.1f;
            }
        }
        if (Input.GetButton("Float"))
        {
            if (paused)
            {
                ContinueGame();
                updateStatusColor();
                SceneManager.LoadScene("SceneSelector");
            }
            else
            {
                if (allowFlight)
                {
                    float tFloatForce = floatForce;
                    if(Input.GetButton("Nitro"))
                    {
                        tFloatForce = floatForce + nitroMod;
                    }
                    rb.AddForce(new Vector3(0.0f, tFloatForce, 0.0f));
                }
            }
        }
        if (Input.GetButtonDown("ToggleGravity"))
        {
            if (paused)
            {
                ContinueGame();
                updateStatusColor();
                reloadLevel();
            }
            else
            {
                if (allowFlight)
                {
                    if (useGravity)
                    {
                        useGravity = false;
                        rb.useGravity = false;
                        rb.AddForce(new Vector3(0.0f, 3.0f, 0.0f));
                    }
                    else
                    {
                        useGravity = true;
                        rb.useGravity = true;
                    }
                    updateStatusColor();
                }
            }
        }
        if (allowFlight && glide)
        {
            rb.AddForce(new Vector3(0.0f, -(Physics.gravity.y * (2 / 3)), 0.0f));
        }
        if (Input.GetButtonDown("CycleMode"))
        {
            if(gameMode >= 2)
            {
                gameMode = 1;
            }
            else
            {
                gameMode = gameMode + 1;
            }
            switch (gameMode)
            {
                case 1:
                    camTarget = gameObject;
                    camTarget.transform.position = camTarget.transform.position + (2 * Vector3.up);
                    iCameraDistance = playerCameraDistance;
                    camFollow = false;
                    vDelta = playerVDelta;
                    //show player
                    if (!useFirstPerson)
                    {
                        cameraDistance = iCameraDistance;
                        GetComponent<MeshRenderer>().enabled = true;
                    }
                    stopCar();
                    break;
                case 2:
                    stopCar();
                    car.transform.position = transform.position;
                    car.transform.LookAt(pcamera.transform);
                    car.transform.Rotate(Vector3.up, 180);
                    iCameraDistance = carCameraDistance;
                    if (!useFirstPerson)
                    {
                        cameraDistance = iCameraDistance;
                    }
                    camFollow = true;
                    vDelta = carVDelta;
                    camTarget = car;
                    lastTargetDirection = camTarget.transform.eulerAngles.y;
                    //hide player
                    GetComponent<MeshRenderer>().enabled = false;
                    startCar();
                    break;
            }
        }
        if (Input.GetButtonDown("ToggleCrossHair"))
        {
            crossHair = !crossHair;
            crossHairObject.SetActive(crossHair);
        }
        if (Input.GetButtonDown("Toggle3DAirMvmt"))
        {
            air2dMovement = !air2dMovement;
        }
        if (Input.GetButtonDown("ToggleFirstPerson"))
        {
            useFirstPerson = !useFirstPerson;
            if (useFirstPerson)
            {
                switch (gameMode)
                {
                    case 1:
                        cameraDistance = 0.01f;
                        break;
                    case 2:
                        cameraDistance = 0.01f;
                        break;
                }
                fpCameraDistance = cameraDistance;
                GetComponent<MeshRenderer>().enabled = false;
                carBody.GetComponent<MeshRenderer>().enabled = false;
                carComponents.GetComponent<MeshRenderer>().enabled = false;
            }
            else
            {
                cameraDistance = iCameraDistance + fpCameraDistance;
                switch (gameMode)
                {
                    case 1:
                        GetComponent<MeshRenderer>().enabled = true;
                        break;
                    case 2:
                        carBody.GetComponent<MeshRenderer>().enabled = true;
                        carComponents.GetComponent<MeshRenderer>().enabled = true;
                        break;
                }
            }
        }
        if (gameMode != 1)
        {
            // Put the player between the cam and the target
            transform.position = Vector3.MoveTowards(pcamera.transform.position, camTarget.transform.position, 5);
        }
        waterSplashRenderer.transform.position = transform.position;
        waterWakeRenderer.transform.position = transform.position;
        if (Input.GetButtonDown("Fire"))
        {
            fire();
        }
        if (Input.GetButtonDown("Help"))
        {
            toggleHelp();
        }
        if (Input.GetButtonDown("Escape"))
        {
            togglePaused();
            if (paused)
            {
                resumeOnHelpClose = true;
            }
            toggleHelp();
        }
    }

    void FixedUpdate()
    {
        float nonGroundMotionModifier = GetComponent<Rigidbody>().velocity.magnitude / 30;
        waterMoveSound.volume = nonGroundMotionModifier * 1.5f;
        airMoveSound.volume = nonGroundMotionModifier / 3;
        airMoveSound.pitch = (nonGroundMotionModifier / 16) + 0.5f;
        float groundMotionModifier;
        try
        {
            groundMotionModifier = (GetComponent<Rigidbody>().velocity.magnitude - groundObject.GetComponent<Rigidbody>().velocity.magnitude) / 30;
        }
        catch
        {
            // Collider doesn't appear to have  a RigidBody, so assume it's stationary
            groundMotionModifier = nonGroundMotionModifier;
        }
        groundMoveSound.volume = groundMotionModifier + 0.5f;
        groundMoveSound.pitch = (groundMotionModifier / 16);
        var wakeEmission = waterWakeRenderer.emission;
        wakeEmission.rateOverTime = nonGroundMotionModifier * 1000;
        if (-rb.velocity != Vector3.zero)
        {
            waterWakeRenderer.transform.forward = -rb.velocity;
        }
        tSpeed = speed;
        float moveLeftRight = -Input.GetAxis("LeftRight");
        float moveFwdBack = Input.GetAxis("FwdBack");
        if (mouseMode != 0)
        {
            moveLeftRight = -Input.GetAxis("HorizCam");
            moveFwdBack = Input.GetAxis("VertCam");
        }
        if (gameMode == 2)
        {
            float brakeValue = 0f;
            if (Input.GetButton("Brake"))
            {
                brakeValue = Input.GetAxis("Brake");
            }
            if (Input.GetButton("Nitro"))
            {
                car.GetComponent<Rigidbody>().AddForce(-Vector3.MoveTowards(car.GetComponent<Rigidbody>().position,pcamera.transform.position,5000));
                GetComponent<ParticleSystem>().Play();
            }
            else
            {
                GetComponent<ParticleSystem>().Stop();
            }
            m_Car.Move(-moveLeftRight, moveFwdBack, moveFwdBack, brakeValue);
        }
        else
        {
            float airMoveLeftRight = -moveLeftRight;
            float airMoveFwdBack = moveFwdBack;
            float airMoveUpDown = Input.GetAxis("UpDown");
            Vector3 movement = new Vector3(moveFwdBack, 0, moveLeftRight) / 5;
            Vector3 airMovement = new Vector3(airMoveLeftRight, 0, airMoveFwdBack);
            airMovement = airMovement * 6;
            if (!allowFlight)
            {
                airMovement = airMovement / 4;
            }
            else
            {
                airMovement = airMovement * 2;
            }
            movement = Camera.main.transform.TransformDirection(movement);
            airMovement = Camera.main.transform.TransformDirection(airMovement);
            movement = new Vector3(movement.x, 0f, movement.z);
            if (air2dMovement)
            {
                airMovement = new Vector3(airMovement.x, 0f, airMovement.z);
            }
            if (Input.GetButton("Brake"))
            {
                tSpeed = tSpeed - 1;
                if (tSpeed < 0)
                {
                    tSpeed = 0;
                }
                rb.drag = rb.drag + 0.3f;
                rb.angularDrag = rb.angularDrag + 0.3f;
            }
            else
            {
                if (!(isGrounded || useGravity))
                {
                    rb.drag = ndrag / 3;
                }
                if (tSpeed < speed)
                {
                    tSpeed = speed;
                }
                if (rb.drag > ndrag)
                {
                    rb.drag = ndrag;
                }
                if (rb.angularDrag > nadrag)
                {
                    rb.angularDrag = nadrag;
                }
            }
            movement = movement * 2;
            airMovement = airMovement * (tSpeed / iSpeed);
            float tNitroMod = nitroMod;
            bool nitro = Input.GetButton("Nitro");
            if (inWater)
            {
                rb.drag = 2f;
                rb.angularDrag = 2f;
                if (nitro)
                {
                    tNitroMod = nitroMod * 2;
                }
            }
            if (Input.GetButton("Nitro"))
            {
                tSpeed = tSpeed * tNitroMod;
                GetComponent<ParticleSystem>().Play();
            }
            else
            {
                GetComponent<ParticleSystem>().Stop();
            }
            rb.AddTorque(movement * tSpeed);
            if (!isGrounded)
            {
                rb.AddForce(airMovement);
            }
            if (allowFlight)
            {
                rb.AddForce(new Vector3(0, airMoveUpDown, 0));
            }
        }
        if (Input.GetButton("Chaingun"))
        {
            fire();
        }
    }

    void LateUpdate()
    {
        if (!paused)
        {
            /* UPDATE CAMERA POSITION */
            int mouseModifierX = 0;
            int mouseModifierY = 0;
            switch (mouseMode)
            {
                case 0:
                    mouseModifierX = 0;
                    mouseModifierY = 0;
                    break;
                case 1:
                    mouseModifierX = 1;
                    mouseModifierY = 1;
                    break;
                case 2:
                    mouseModifierX = -1;
                    mouseModifierY = 1;
                    break;
                case 3:
                    mouseModifierX = 1;
                    mouseModifierY = -1;
                    break;
                case 4:
                    mouseModifierX = -1;
                    mouseModifierY = -1;
                    break;
            }
            hDelta = hDelta + (turnSpeed * (Input.GetAxis("HorizCam")) * kbdModifier) + ((turnSpeed / 10) * Input.GetAxis("HCamMouse") * mouseModifierX);

            vDelta = vDelta + (-1f * (Input.GetAxis("VertCam")) * kbdModifier) + (-0.1f * Input.GetAxis("VCamMouse") * mouseModifierY);
            // FIXME: for some reason, when it gets to 90 degrees (directly above the player), the rotation gets reversed. WTF?
            vDelta = Math.Min(Math.Max(vDelta, -89.9f), 89.9f);

            float camZoom = Input.GetAxis("CamZoom") + (Input.GetAxis("CamZoomMouse"));
            if (camZoom > 0)
            {
                cameraDistance = cameraDistance * (1.01f * (1 + (Input.GetAxis("CamZoomMouse") / 10)));
            }
            else if (camZoom < 0)
            {
                cameraDistance = cameraDistance / (1.01f * (1 - (Input.GetAxis("CamZoomMouse") / 10)));
            }
            if (Input.GetButton("Brake") && paused)
            {
                resetSettings();
            }
            if (camFollow)
            {
                // based on https://forum.unity3d.com/threads/delta-rotation.137458/#post-937483
                float targetDirectionDelta = camTarget.transform.eulerAngles.y - lastTargetDirection;
                if (targetDirectionDelta > 180) targetDirectionDelta -= 360;
                if (targetDirectionDelta < -180) targetDirectionDelta += 360;
                hDelta = hDelta + (targetDirectionDelta / 58f);
            }
            lastTargetDirection = camTarget.transform.eulerAngles.y;

            /* Find the base desired position for the camera, relative to the player.
             * This position is a point on the semicircle from directly above the player
             * to directly below the player, that goes through the camera's old direction. */
            pcamera.transform.position = camTarget.transform.position + ((new Vector3(Mathf.Sin(hDelta), 0, Mathf.Cos(hDelta)) * cameraDistance));
            pcamera.transform.RotateAround(camTarget.transform.position, Vector3.Cross(pcamera.transform.position - camTarget.transform.position, Vector3.up), vDelta);
            pcamera.transform.LookAt(camTarget.transform.position);
            if (gameMode == 2)
            {
                pcamera.transform.position = pcamera.transform.position + (Vector3.up);
            }
            else if (gameMode == 3)
            {
                pcamera.transform.position = pcamera.transform.position + (Vector3.up * 2);
            }
        }
    }

    void OnTriggerEnter(Collider collider)
    {
        triggerObject = collider.gameObject;
        float volume = 0;
        try
        {
            volume = Vector3.Magnitude(collider.GetComponent<Rigidbody>().velocity - GetComponent<Rigidbody>().velocity) / 50;
        }
        catch
        {
            // Object probably does not have a Rigidbody attached to it, so do nothing
            ;
        }
        if (triggerObject.CompareTag("WaterTrigger"))
        {
            inWater = true;
            waterSplash.volume = volume * 0.5f;
            waterSplash.Play();
            waterSplashRenderer.Play();
            waterMoveSound.Play();
            waterWakeRenderer.Play();
        }
        if (triggerObject.CompareTag("PickUp"))
        {
            pickUpSound.Play();
            triggerObject.SetActive(false);
            score = score + 1;
            ScoreUpdate();
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        isGrounded = true;
        groundObject = collision.collider.gameObject;
        groundCollisionPoint = collision.contacts[0];
        float volume = collision.relativeVelocity.magnitude / 50;
        airMoveSound.Stop();
        hitSound.volume = volume;
        hitSound.Play();
        groundMoveSound.Play();
        if (groundObject.CompareTag("FinishPad"))
        {
            // exit scene
            if (score >= maxScore)
            {
                scoreLabel.GetComponent<TextMesh>().text = "Level won";
                if (!levelWon)
                {
                    victorySound.Play();
                    levelWon = true;
                }
                Invoke("loadNextLevel", 2);
            }
            else
            {
                scoreLabel.GetComponent<TextMesh>().text = "Find all collectibles before winning!";
                Invoke("ScoreUpdate", 2);
            }
        }
        if (groundObject.CompareTag("sceneSelect"))
        {
            scoreLabel.GetComponent<TextMesh>().text = "Loading...";
            SceneManager.LoadScene(groundObject.GetComponent<SceneNameString>().SceneName);
        }
        else if (groundObject.CompareTag("OobTrigger"))
        {
            if (useGravity)
            {
                scoreLabel.GetComponent<TextMesh>().text = "Out of bounds!";
                oobSound.Play();
                Invoke("reloadLevel", 2);
            }
        }
    }

    private void OnCollisionExit(Collision collision)
    {
        isGrounded = false;
        groundObject = collision.collider.gameObject;
        groundMoveSound.Stop();
        airMoveSound.Play();
    }

    private void OnTriggerExit(Collider collider)
    {
        if (collider.gameObject.CompareTag("WaterTrigger"))
        {
            inWater = false;
            waterExitSound.Play();
            waterMoveSound.Stop();
            waterWakeRenderer.Stop();
        }
    }

    private void OnGUI()
    {
        if (helpStatus)
        {
            //set up scaling: from http://answers.unity3d.com/questions/169056/bulletproof-way-to-do-resolution-independant-gui-s.html
            float rx = Screen.width / 2000;
            float ry = Screen.height / 2000;
            GUI.matrix = Matrix4x4.TRS(new Vector3(0, 0, 0), Quaternion.identity, new Vector3(rx, ry, 1));
            GUIStyle style = new GUIStyle(GUI.skin.box);
            if (guiResetting)
            {
                if(guiSettingsUnsaved())
                {
                    GUI.Box(new Rect(100, 100, Screen.width - 200, Screen.height - 300), "You have made unapplied changes. What would you like to do?", style);
                    if (GUI.Button(new Rect(100, Screen.height - 190, (Screen.width / 2) - 105, 180), "Reset anyway"))
                    {
                        resetGuiSettings();
                        guiResetting = false;
                    }
                    if (GUI.Button(new Rect((Screen.width / 2) + 5, Screen.height - 190, (Screen.width / 2) - 105, 180), "Continue editing"))
                    {
                        guiResetting = false;
                    }
                }
                else
                {
                    GUI.Box(new Rect(100, 100, Screen.width - 200, Screen.height - 300), "You have made no changes, so there is nothing to reset.", style);
                    if (GUI.Button(new Rect(100, Screen.height - 190, Screen.width - 200, 180), "OK"))
                    {
                        guiResetting = false;
                    }
                }
            }
            else if (guiDefaulting)
            {
                if (guiSettingsUnsaved())
                {
                    GUI.Box(new Rect(100, 100, Screen.width - 200, Screen.height - 300), "You have made unapplied changes. What would you like to do?", style);
                    if (GUI.Button(new Rect(100, Screen.height - 190, (Screen.width / 2) - 105, 180), "Load defaults anyway"))
                    {
                        defaultGuiSettings();
                        guiDefaulting = false;
                    }
                    if (GUI.Button(new Rect((Screen.width / 2) + 5, Screen.height - 190, (Screen.width / 2) - 105, 180), "Continue editing"))
                    {
                        guiDefaulting = false;
                    }
                }
                else
                {
                    defaultGuiSettings();
                    guiDefaulting = false;
                }
            }
            else if (guiCancelling)
            {
                if (guiSettingsUnsaved())
                {
                    GUI.Box(new Rect(100, 100, Screen.width - 200, Screen.height - 300), "You have made unapplied changes. What would you like to do?", style);
                    if (GUI.Button(new Rect(100, Screen.height - 190, (Screen.width / 2) - 105, 180), "Cancel anyway"))
                    {
                        resetGuiSettings();
                        guiCancelling = false;
                        helpStatus = false;
                        guiConditionalResume();
                    }
                    if (GUI.Button(new Rect((Screen.width / 2) + 5, Screen.height - 190, (Screen.width / 2) - 105, 180), "Continue editing"))
                    {
                        guiCancelling = false;
                    }
                }
                else
                {
                    guiCancelling = false;
                    helpStatus = false;
                    guiConditionalResume();
                }
            }
            else
            {
                style.alignment = TextAnchor.MiddleLeft;
                string pauseMenuLeftContents = @"
                Default controls in QWERTY layout:
                    * Horizontal movement: arrow keys
                    * Vertical movement: down/up = 3/4
                    * Camera: fwd/back = d/e; left/right = s/f
                        (cam controls become movement
                            when mouse is enabled)
                    * Brake: t
                    * Pause: z
                    * Pause and show help: esc
                    * Toggle gravity: r
                    * Toggle glider: 2
                    * Nitro: x
                    * Hover: a
                    * Fire: w / left click
                    * Jump: space
                    * Show this help: h
                    * Zoom in/out: c/v
";
                string pauseMenuRightContents = @"
                    * Cycle mode: 5
                    * (not implemented) Winch: o
                    * Toggle crosshair: 6
                    * Toggle first-person view: i
                    * Toggle 3D air movement: 7
                    * Chaingun: q
                    * (when paused) restart level: r
                    * (when paused) restore default conditions: t
                Current state:
                    * Gravity: " + useGravity + @"
                    * Pausedness: " + paused + @"
                    * 3D air movement: " + !(air2dMovement) + @"
                    * Glider: " + glide + @"
                    * Mode (1=marble, 2=driving, (3=human, not imp.)): " + gameMode + @"
                    * 1st person mode (alt. modes only): " + useFirstPerson + @"
";

                GUI.Box(new Rect(100, 100, (Screen.width / 2) - 150, Screen.height - 200), pauseMenuLeftContents, style);
                GUI.Box(new Rect((Screen.width / 2) + 50, 100, (Screen.width / 2) - 150, Screen.height - 200), pauseMenuRightContents, style);

                /* BUTTONS */
                if (paused)
                {
                    if (GUI.Button(new Rect(0, 0, 200, 50), "Choose other level"))
                    {
                        loadSceneSelect();
                    }
                    String airMovementType;
                    if (air2dMovement)
                    {
                        airMovementType = "3D";
                    }
                    else
                    {
                        airMovementType = "2D";
                    }
                    if (GUI.Button(new Rect(210, 0, 50, 50), "Air " + airMovementType))
                    {
                        air2dMovement = !air2dMovement;
                    }
                    String glideDesc = "";
                    if (glide)
                    {
                        glideDesc = "Don't ";
                    }
                    else
                    {
                        glideDesc = "";
                    }
                    if (GUI.Button(new Rect(270, 0, 50, 50), glideDesc + "Glide"))
                    {
                        glide = !glide;
                    }
                    String mouseDesc = "";
                    switch (mouseMode)
                    {
                        case 0:
                            mouseDesc = "Mouse";
                            break;
                        case 1:
                            mouseDesc = "Normal";
                            break;
                        case 2:
                            mouseDesc = "-X";
                            break;
                        case 3:
                            mouseDesc = "-Y";
                            break;
                        case 4:
                            mouseDesc = "-X -Y";
                            break;
                    }
                    if (GUI.Button(new Rect(330, 0, 50, 50), mouseDesc))
                    {
                        if (mouseMode >= 4)
                        {
                            mouseMode = 0;
                        }
                        else
                        {
                            mouseMode = mouseMode + 1;
                        }
                        if (mouseMode != 0)
                        {
                            kbdModifier = 0;
                        }
                        else
                        {
                            kbdModifier = 1;
                        }
                    }
                    if (GUI.Button(new Rect(390, 0, 50, 50), "Defaults"))
                    {
                        guiDefaulting = true;
                    }
                    if (GUI.Button(new Rect(450, 0, 50, 50), "Reset"))
                    {
                        guiResetting = true;
                    }
                    if (GUI.Button(new Rect(510, 0, 50, 50), "Cancel"))
                    {
                        guiCancelling = true;
                    }
                    if (GUI.Button(new Rect(570, 0, 50, 50), "Apply"))
                    {
                        applyGuiSettings();
                    }
                    if (GUI.Button(new Rect(630, 0, 50, 50), "Done"))
                    {
                        applyGuiSettings();
                        helpStatus = false;
                        guiConditionalResume();
                    }
                }

                /* SETTINGS */
                GUI.Label(new Rect(0, Screen.height - 50, 100, 50), "Turn speed (" + iTurnSpeed.ToString() + ")");
                string turnSpeedString = GUI.TextField(new Rect(100, Screen.height - 50, 50, 50), guiTurnSpeed.ToString());
                guiTurnSpeed = float.Parse(turnSpeedString);

                GUI.Label(new Rect(160, Screen.height - 50, 100, 50), "Cam. dist. (" + iCameraDistance.ToString() + ")");
                string cameraDistanceString = GUI.TextField(new Rect(260, Screen.height - 50, 50, 50), guiCameraDistance.ToString());
                guiCameraDistance = float.Parse(cameraDistanceString);

                GUI.Label(new Rect(320, Screen.height - 50, 100, 50), "Player size (" + iPlayerSize.ToString() + ")");
                string playerSizeString = GUI.TextField(new Rect(420, Screen.height - 50, 50, 50), guiPlayerSize.ToString());
                guiPlayerSize = float.Parse(playerSizeString);

                GUI.Label(new Rect(480, Screen.height - 50, 100, 50), "Speed (" + iSpeed.ToString() + ")");
                string speedString = GUI.TextField(new Rect(580, Screen.height - 50, 50, 50), guiSpeed.ToString());
                guiSpeed = float.Parse(speedString);
            }
        }
    }

    private void guiConditionalResume()
    {
        if (paused && resumeOnHelpClose)
        {
            togglePaused();
            resumeOnHelpClose = false;
        }
    }

    private void applyGuiSettings()
    {
        turnSpeed = guiTurnSpeed;
        cameraDistance = guiCameraDistance;
        playerSize = guiPlayerSize;
        speed = guiSpeed;
    }

    private void resetGuiSettings()
    {
        guiTurnSpeed = turnSpeed;
        guiCameraDistance = cameraDistance;
        guiPlayerSize = playerSize;
        guiSpeed = speed;
    }

    private void defaultGuiSettings()
    {
        air2dMovement = iAir2dMovement;
        crossHair = iCrossHair;
        useFirstPerson = iUseFirstPerson;
        mouseMode = 0;
        glide = iGlide;
        guiTurnSpeed = iTurnSpeed;
        guiCameraDistance = iCameraDistance;
        guiPlayerSize = iPlayerSize;
        guiSpeed = iSpeed;
    }

    private bool guiSettingsUnsaved()
    {
        return !(
               turnSpeed        == guiTurnSpeed
            && cameraDistance   == guiCameraDistance
            && playerSize       == guiPlayerSize
            && speed            == guiSpeed
               );
    }

    private void resetSettings()
    {
        defaultGuiSettings();
        applyGuiSettings();
    }

    /* INTERNAL UTILITY METHODS */

    private void toggleHelp()
    {
        helpStatus = !helpStatus;
        if (helpStatus)
        {
            Cursor.lockState = CursorLockMode.Locked;
        }
        else
        {
            Cursor.lockState = CursorLockMode.Confined;
        }
    }

    private void togglePaused()
    {
        if (!paused)
        {
            pauseStatusColor();
            PauseGame();
            Cursor.lockState = CursorLockMode.Confined;
        }
        else
        {
            ContinueGame();
            updateStatusColor();
            Cursor.lockState = CursorLockMode.Locked;
        }
        resumeOnHelpClose = false;
    }

    private void stopCar()
    {
        m_Car.enabled = false;
        car.GetComponent<CarAudio>().enabled = false;
        car.SetActive(false);
    }

    private void startCar()
    {
        car.SetActive(true);
        m_Car.enabled = true;
        car.GetComponent<CarAudio>().enabled = true;
    }

    private void fire()
    {
        if (!fireTimeout)
        {
            gunShotSound.Play();
            GameObject temp;
            Vector3 bpos = transform.position;
            if(gameMode != 1)
            {
                bpos = pcamera.transform.position;
            }
            temp = Instantiate(bullet, transform.position, transform.rotation) as GameObject;
            Rigidbody rb;
            rb = temp.GetComponent<Rigidbody>();
            rb.AddForce(new Vector3(pcamera.transform.forward.x, pcamera.transform.forward.y, pcamera.transform.forward.z) * 100f);
            Destroy(temp, 1.0f);
            fireTimeout = true;
            Invoke("resetFireTimeout", 0.1f);
        }
    }

    private void resetFireTimeout()
    {
        fireTimeout = false;
    }

    private void pauseStatusColor()
    {
        scoreLabel.GetComponent<TextMesh>().color = Color.gray;
    }

    private void updateStatusColor()
    {
        Color targetColor = Color.red;
        Color orange = new Color(255, 165, 0);
        Color lavender = new Color(179, 120, 211);
        if (!useGravity)
        {
            targetColor = Color.cyan;
        }
        if (glide)
        {
            targetColor = orange;
            if (!useGravity)
            {
                targetColor = lavender;
            }
        }
        if (paused)
        {
            targetColor = Color.gray;
        }
        scoreLabel.GetComponent<TextMesh>().color = targetColor;
    }

    private void ScoreUpdate()
    {
        scoreLabel.GetComponent<TextMesh>().text = "Score: " + score + " / " + maxScore;
    }

    private void loadNextLevel()
    {
        scoreLabel.GetComponent<TextMesh>().text = "Loading...";
        if (currentScene == 3)
        {
            SceneManager.LoadScene(0);
        }
        else
        {
            SceneManager.LoadScene(currentScene + 1);
        }
    }

    internal void loadSceneSelect()
    {
        scoreLabel.GetComponent<TextMesh>().text = "Loading...";
        SceneManager.LoadScene("SceneSelector");
    }

    private void reloadLevel()
    {
        SceneManager.LoadScene(currentScene);
    }

    private void PauseGame()
    {
        paused = true;
        Time.timeScale = 0;
    }

    private void ContinueGame()
    {
        Time.timeScale = 1;
        paused = false;
    }

    private void getCarComponents()
    {
        // based on http://answers.unity3d.com/questions/905442/how-to-get-children-gameobjects.html
        for (int i = 0; i < car.transform.childCount; i++)
        {
            if (car.transform.GetChild(i).transform.name == "SkyCar")
            {
                for (int j = 0; j < car.transform.GetChild(i).transform.childCount; j++)
                {
                    switch (car.transform.GetChild(i).transform.GetChild(j).transform.name)
                    {
                        case "SkyCarBody":
                            carBody = car.transform.GetChild(i).transform.GetChild(j).gameObject;
                            break;
                        case "SkyCarComponents":
                            carComponents = car.transform.GetChild(i).transform.GetChild(j).gameObject;
                            break;
                    }
                }
            }
        }

    }
}
