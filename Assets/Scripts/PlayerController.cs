
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    [Header("Control Settings")]
    public float MAX_PLAYER_SPEED = 4.8f;
    public float MAX_PLAYER_SPRINT = 9.0f;

    public float SPRINT_OXYGEN_DEPLETION_MULTIPLIER = 1.5f;

    public float ACCELERATION = 40.0f;
    public float DAMPING = 0.15f;

    public float MAX_SLOPE = 65.0f;
    public float JUMP_HEIGHT = 4.0f;
    
    public float MOUSE_SENSITIVITY = 0.8f;

    public float GRAB_DISTANCE = 5.0f;

    float noClipSpeed = 4.8f;

    [Header("Effect Settings")]
    public AudioClip[] footstepSounds;
    public float footstepRate = 0.5f;
    public float footstepVolumeMax = 1.0f;

    public AudioClip submergeSound;
    public float submergeSoundVolume;

    public AudioClip emergeSound;
    public float emergeSoundVolume;

    public AudioClip underwaterLoop;
    public float underwaterLoopVolume;

    public float underwaterEffectLerpRate;

    //--------------------------

    [Header("Underwater Effects")]
    public GameObject globalWater;

    public float waterAdjust;
    public float amplitude;
    public float waveSpeed;

    public Texture2D heightmapTexture;
    public float heightmapHeight;
    public float heightmapWidth;

    public Material screenMaterial;

    //-------------------------

    float animatorHeight;
    float waveOffset;

    bool submerged = false;
    WaterAnimator waterAnimator;

    //-------------------------

    [Header("Other")]
    public float casualOxygenDepletion = 0.0075f;
    public float standardOxygenDepletion = 0.01f;
    public float hardcoreOxygenDepletion = 0.015f;
    
    public bool playerLoaded;

    public UnderwaterRenderFeature renderFeature;

    //--------------------------

    float footstepTimeCheck = 0;
    int lastFootstepSound = 0;
    float oxygenDepletionRate = 0.01f;

    //--------------------------

    uint credits = 0;

    float oxygenLevel = 1.0f;

    bool oxygenDepleting = false;

    bool paused = true;

    float currentTime;

    public uint creditTarget = 0;

    //--------------------------

    public ActionItem selectedObject;

    //--------------------------

    // The main rigidbody associated with the player
    Rigidbody controllerBody;

    // The camera the player is currently using
    Camera camera;

    // The player's current movement velocity
    Vector3 currentMovement;
    
    // The player's main audioSource
    public AudioSource playerAudio;

    // The player's "Body Collider" and "Main Collider"
    Transform mainCollider, bodyCollider;
    CapsuleCollider mainColliderCapsule, bodyColliderCapsule;

    //--------------------------

    // Audio source for playing audio underwater, and the low-pass
    // filter for underwater music
    AudioSource underwaterSource;
    AudioLowPassFilter underwaterLowPass;
    float underwaterEffectLerp;

    //--------------------------

    // Boolean for checking that the backquote has been released
    bool backquoteReleased = true;

    // Accumulators for the mouse movement
    float xAccumulator, yAccumulator;

    // Boolean checks for mouse clicking
    bool leftMousePressed = false;

    // Checks if the player is dead
    bool dead = false;

    // Checks if the player has won
    bool victorious = false;

    // Check if the player is no-clipping
    bool noclip = false;
    
    //--------------------------

    // Checks for if the player has moved around/looked around
    bool playerMoved = false;
    bool playerLooked = false;
    bool playerSprinted = false;
    bool playerJumped = false;

    // Records the last action the player did
    string lastInteraction = "";

    // Check for whether the player is sprinting
    bool isSprinting = false;

    //--------------------------

    InputAction mouseInput;

    //--------------------------

    // Start is called before the first frame update
    void Start() {
        // Gets the Rigidbody component associated with the controller
        controllerBody = gameObject.GetComponent<Rigidbody>();

        // Gets the player's camera
        camera = gameObject.GetComponentInChildren<Camera>();

        // Gets the colliders associated with the player
        mainCollider = transform.Find("MainCollider");
        bodyCollider = transform.Find("BodyCollider");

        // Gets the capsules associated with the player 
        mainColliderCapsule = mainCollider.GetComponent<CapsuleCollider>();
        bodyColliderCapsule = bodyCollider.GetComponent<CapsuleCollider>();

        // Gets the character's audioSource
        playerAudio = gameObject.AddComponent<AudioSource>();

        // Initialises the currentMovement vector
        currentMovement = new Vector3(0.0f, 0.0f, 0.0f);

        // Sets the cursor lockstate, creates a mouse input action
        mouseInput = new InputAction(type: InputActionType.Value, binding: "<Mouse>/delta");
        mouseInput.Enable();
        Cursor.lockState = CursorLockMode.Locked;
        
        //--------------------------

        // Creates the underwater effects
        waterAnimator = globalWater.GetComponent<WaterAnimator>();
        underwaterSource = gameObject.AddComponent<AudioSource>();
        underwaterLowPass = gameObject.AddComponent<AudioLowPassFilter>();

        // Sets the initial settings for the underwater effects
        underwaterSource.volume = 0.0f;
        underwaterSource.clip = underwaterLoop;
        underwaterSource.loop = true;
        underwaterSource.Play();

        underwaterLowPass.cutoffFrequency = 500.0f;
        underwaterLowPass.enabled = false;

        //--------------------------

        // Sets the oxygen depletion based on the difficulty level
        string difficultyLevel = PlayerPrefs.GetString("Difficulty");

        if (difficultyLevel == "CASUAL")
            oxygenDepletionRate = casualOxygenDepletion;
        else if (difficultyLevel == "STANDARD")
            oxygenDepletionRate = standardOxygenDepletion;
        else if (difficultyLevel == "HARDCORE")
            oxygenDepletionRate = hardcoreOxygenDepletion;
    }

    // Update is called once per frame
    void Update() {
        if (!paused && !victorious) {
            CalculateOxygen();
            ProcessFootsteps();
            CheckEmerged();
        }

        currentTime += Time.deltaTime;
    }

    void FixedUpdate() {
        if (!dead && !paused && !victorious) {
            MoveCharacter();
        }
    }

    void LateUpdate() {
        if (!paused) {
            UpdateMouse();
        }
    }

    //--------------------------

    public float SampleHeightmap(Vector3 pos, float time) {
        // Gets the position on the heightmap corresponding to the position
        Color heightValue = heightmapTexture.GetPixel(
            (int) ((((pos.x) / heightmapWidth) + time * waveSpeed * 0.01) * heightmapTexture.width) % (int) heightmapTexture.width,
            (int) ((((pos.z) / heightmapHeight) + time * waveSpeed * 0.01) * heightmapTexture.height) % (int) heightmapTexture.height
        );

        return (heightValue.r * 2.0f - 1.0f) * amplitude;
    }
    
    void CheckEmerged() {
        // Gets the water offset calculation from the camerascript
        waveOffset = SampleHeightmap(camera.transform.position, currentTime);

        // Sets the animator height
        animatorHeight = waterAnimator.waterHeight;

        // Sets the pass variables
        renderFeature.SetMaterialVars(
            camera.transform.position.y,
            animatorHeight + waveOffset + waterAdjust + 0.05f,
            animatorHeight + waveOffset + waterAdjust - 0.05f
        );

        if (
            camera.transform.position.y <= animatorHeight + waveOffset + waterAdjust
            && submerged == false
        ) {
            // Submerges the camera, plays the submerge sound
            playerAudio.PlayOneShot(submergeSound, submergeSoundVolume);
            submerged = true;

            // Initialises the underwater lerp
            underwaterEffectLerp = 0.0f;

            // Sets the underwater SFX
            underwaterLowPass.enabled = true;
            underwaterSource.volume = underwaterLoopVolume;
        }
        else if (
            camera.transform.position.y > animatorHeight + waveOffset + waterAdjust
            && submerged == true
        ) {
            // Stops the camera from being submerged, plays the emerging sound
            playerAudio.PlayOneShot(emergeSound, emergeSoundVolume);
            submerged = false;

            // Sets the underwater SFX
            underwaterLowPass.enabled = false;
            underwaterSource.volume = 0.0f;
        }

        // If the player is submerged, then the underwater lerp + lowPass should be set
        if (submerged) {
            // Sets the underwater lerp based on the lerp rate
            underwaterEffectLerp += Time.deltaTime * underwaterEffectLerpRate;

            // Lerps the low-pass effect so that we can hear the initial submerge
            underwaterLowPass.cutoffFrequency = Mathf.Lerp(22000.0f, 500.0f, underwaterEffectLerp);
        }
    }

    void CalculateOxygen() {
        if (oxygenDepleting) {
            // Depletes the player's oxygen level
            if (!isSprinting)
                oxygenLevel = Mathf.Clamp(oxygenLevel - oxygenDepletionRate * Time.deltaTime, 0.0f, 1.0f);
            else
                oxygenLevel = Mathf.Clamp(oxygenLevel - oxygenDepletionRate * Time.deltaTime * SPRINT_OXYGEN_DEPLETION_MULTIPLIER, 0.0f, 1.0f);

            // Sets the shader global for oxygen depletion
            Shader.SetGlobalFloat("PlayerOxygen", oxygenLevel);

            // If the oxygen level hits zero, kill the player
            if (oxygenLevel == 0 && !dead) {
                // Unset the constraints on the player
                controllerBody.constraints = RigidbodyConstraints.None;

                // Kills the player
                dead = true;
            }
        }
    }

    void ProcessFootsteps() {
        // If the character isn't grounded and isn't dead, check their movement on the X and Z axes
        if (IsGrounded() && !dead) {
            // Gets the magnitude of the current movement divided by the max speed (with sprinting)
            float moveMagnitude = currentMovement.magnitude / MAX_PLAYER_SPRINT;

            // Re-calculates the footstep timecheck.
            footstepTimeCheck += moveMagnitude * footstepRate * Time.deltaTime;

            // If the footstep timecheck is over one, we play a sound
            if (footstepTimeCheck >= 1.0f) {
                footstepTimeCheck = footstepTimeCheck % 1.0f;

                // Inits the new index
                int newIndex = Random.Range(0, footstepSounds.Length);

                while (newIndex == lastFootstepSound) {
                    newIndex = Random.Range(0, footstepSounds.Length);
                }

                // Sets the old index to the new footstep index
                lastFootstepSound = newIndex;
 
                AudioClip audio = footstepSounds[newIndex];
                playerAudio.PlayOneShot(audio, footstepVolumeMax * moveMagnitude);
            }
        }
    }

    //--------------------------

    void UpdateMouse() {
        // Gets the x and y delta of the mouse.
        float xDelta = Mouse.current.delta.x.ReadValue() * 0.125f * MOUSE_SENSITIVITY;
        float yDelta = Mouse.current.delta.y.ReadValue() * 0.125f * MOUSE_SENSITIVITY;

        Vector2 delta = mouseInput.ReadValue<Vector2>() * 0.125f;

        // If the xDelta or yDelta are above 0, then the player has moved their mouse
        if (!playerLooked && (Mathf.Abs(xDelta) > 0.0f || Mathf.Abs(yDelta) > 0.0f))
            playerLooked = true;

        // Updates the rotation of the player
        controllerBody.rotation *= Quaternion.Euler(
            0.0f,
            xDelta,
            0.0f
        );

        // Updates the camera rotation
        Quaternion mouseRotComponent = Quaternion.Euler(
            -yDelta,
            0.0f,
            0.0f
        );

        // Calculates the modulo of the camera and mouse's resultant rotation angles so that
        // we can lock the camera correctly.
        float calculatedCameraRot = (camera.transform.rotation.eulerAngles.x + 180.0f) % 360.0f - 180.0f;
        float calculatedMouseRot = (mouseRotComponent.eulerAngles.x + 180.0f) % 360.0f - 180.0f;

        // Calculates the final camera rotation
        float calculatedTotalRot = calculatedCameraRot + calculatedMouseRot;

        // Updates the rotation of the camera
        if (calculatedTotalRot > -85.0f && calculatedTotalRot < 85.0f)
            camera.transform.rotation *= mouseRotComponent;
    }

    void MoveCharacter() {
        // Sets a max speed depending on whether the player is sprinting or not
        float currentSpeed = MAX_PLAYER_SPEED;
        if (noclip)
        {
            currentSpeed = noClipSpeed;
        }
        
        // Multiplier for NoClip Speed 
        float NO_CLIP_ACC = 1.1f;

        // Sprinting 
        if (Keyboard.current[Key.LeftShift].IsPressed()) {
            // Sets the current speed to the sprint speed
            currentSpeed = MAX_PLAYER_SPRINT;

            // Sets it so that the player is sprinting
            isSprinting = true;

            if (!playerSprinted)
                playerSprinted = true;
        }
        else {
            // Sets it so that the player isn't sprinting
            isSprinting = false;
        }

        // Set Speed of NoClip Camera 
        if (noclip)
        {
            if (Keyboard.current[Key.E].IsPressed())
            {
                noClipSpeed = noClipSpeed * NO_CLIP_ACC;
                currentSpeed = noClipSpeed;
            }
            if (Keyboard.current[Key.Q].IsPressed())
            {
                noClipSpeed = noClipSpeed / NO_CLIP_ACC;
                currentSpeed = noClipSpeed;
            }
        }

        // Calculates the acceleration
        float deltaMove = ACCELERATION * Time.fixedDeltaTime;

        // Initialises a new movement vector
        Vector3 newMovement = new Vector3(0.0f, 0.0f, 0.0f);

        if (Keyboard.current[Key.W].IsPressed())
            newMovement += new Vector3(0.0f, 0.0f, 1.0f);
        if (Keyboard.current[Key.S].IsPressed())
            newMovement += new Vector3(0.0f, 0.0f, -1.0f);
        if (Keyboard.current[Key.A].IsPressed())
            newMovement += new Vector3(-1.0f, 0.0f, 0.0f);
        if (Keyboard.current[Key.D].IsPressed())
            newMovement += new Vector3(1.0f, 0.0f, 0.0f);

        // Multiplies the movement by the change in speed
        newMovement = newMovement.normalized * deltaMove;

        // Adds to the current movement
        currentMovement += newMovement;

        // If the current movement is above 0, then the player has moved
        if (!playerMoved && currentMovement.magnitude > 0.0f)
            playerMoved = true;

        // Clamps the current movement
        if (currentMovement.magnitude >= currentSpeed)
            currentMovement = (currentMovement/currentMovement.magnitude) * currentSpeed;

        // Based on the rotation of the player, calculates a strafe
        if (!noclip) {
            Vector3 strafeMovement = CalculateStrafe(currentMovement);
            controllerBody.velocity = new Vector3(strafeMovement.x, controllerBody.velocity.y, strafeMovement.z);
        }
        else {
            Vector3 strafeMovement = CalculateNoclipStrafe(currentMovement);
            controllerBody.velocity = new Vector3(strafeMovement.x, strafeMovement.y, strafeMovement.z);
        }

        // multiplies the curr force by the damping
        if (!Keyboard.current[Key.W].IsPressed() &&
            !Keyboard.current[Key.A].IsPressed() &&
            !Keyboard.current[Key.S].IsPressed() &&
            !Keyboard.current[Key.D].IsPressed())
            currentMovement = currentMovement - (currentMovement * DAMPING * deltaMove);

        //--------------------------

        // If the player presses the backtick key, turn no-clip on/off
        if (Keyboard.current[Key.Backquote].IsPressed()) {
            if (backquoteReleased) {
                // Sets that the backquote's being pressed
                backquoteReleased = false;

                // Check + set noclipping if true/not true
                if (!noclip) {
                    noclip = true;
                    oxygenDepleting = false;

                    // Sets the gravity + collisions of the main rigidbody to false
                    controllerBody.useGravity = false;
                    controllerBody.detectCollisions = false;
                }
                else {
                    noclip = false;
                    oxygenDepleting = true;

                    // Sets the gravity + collisions of the main rigidbody to true
                    controllerBody.useGravity = true;
                    controllerBody.detectCollisions = true;

                    // Reset the camera's FoV
                    camera.fieldOfView = 60;
                }
            }
        } else {
            backquoteReleased = true;
        }

        // If the player is noclipping and scrolls the mouse, we can adjust the camera FoV
        if (noclip) {
            camera.fieldOfView += Mouse.current.scroll.ReadValue().y / 60.0f;
        }

        //--------------------------

        bool grounded = IsGrounded();

        // If the player is grounded, then 
        if (grounded && Keyboard.current[Key.Space].IsPressed()) {
            // Check the controllerbody's current velocity
            float upwardVelocity = controllerBody.velocity.y;

            // Calculate the jump height based on this upward velocity
            float jumpHeight = Mathf.Clamp(JUMP_HEIGHT - upwardVelocity, 0.0f, JUMP_HEIGHT);

            // Makes the player jump!
            controllerBody.AddForce(new Vector3(0.0f, jumpHeight, 0.0f), ForceMode.VelocityChange);
            
            if (!playerJumped)
                playerJumped = true;
        }

        //--------------------------

        // Sets a hard limit on the vertical velocity of the player to stop the ramping effect
        controllerBody.velocity = new Vector3(
            controllerBody.velocity.x,
            controllerBody.velocity.y,
            controllerBody.velocity.z
        );
        
        //--------------------------

        // Gets the mouse position
        Vector3 mousePosition = Mouse.current.position.ReadValue();
        mousePosition.z = camera.farClipPlane * .5f;

        // Calculates the world space position of the mouse
        Vector3 worldMousePosition = camera.ScreenToWorldPoint(mousePosition);
        
        // Draws a ray from the player to the mouse position
        Ray pickupRay = new Ray(camera.transform.position, worldMousePosition - camera.transform.position);
        RaycastHit hitInfo;

        // Set the selected object initially as null
        selectedObject = null;

        // Checks for hovering
        if (Physics.Raycast(pickupRay, out hitInfo, GRAB_DISTANCE)) {
            // Gets a pickup component inside the object
            ActionItem pickup = hitInfo.transform.GetComponent<ActionItem>();

            // If the pickup exists, then set the selected object
            if (pickup != null) {
                selectedObject = pickup;
            }
        }

        // Checks if the left mouse has been pressed
        if (Mouse.current.leftButton.IsPressed()) {
            // If the left mouse hasn't already been pressed, 
            if (!leftMousePressed) {
                // Sets the left mouse to being true
                leftMousePressed = true;

                // Checks if something's been hit by the pickupRay
                if (Physics.Raycast(pickupRay, out hitInfo, GRAB_DISTANCE)) {
                    // Gets a pickup component inside the object
                    ActionItem pickup = hitInfo.transform.GetComponent<ActionItem>();

                    // If the pickup exists, then pick it up.
                    if (pickup != null) {
                        lastInteraction = pickup.ActivateObject(this);
                    }
                }
            }
        } else {
            leftMousePressed = false;
        }
    }
    
    //--------------------------

    // Calculates the resultant movement of the player when accounting for the player's rotation
    Vector3 CalculateStrafe(Vector3 originalMovement) {
        return controllerBody.rotation * originalMovement;
    }

    // Calculates the resultant movement of the player when accounting for the player's rotation AND
    // the rotation of the camera (for no-clipping)
    Vector3 CalculateNoclipStrafe(Vector3 originalMovement) {
        return camera.transform.rotation * originalMovement;
    }

    // Checks if the player is grounded
    bool IsGrounded() {
        // Shoots a ray to the ground
        Ray groundRay = new Ray(mainCollider.position, -transform.up);
        RaycastHit hitInfo;

        // Fires a ray into the ground to check if the player is grounded
        if (Physics.Raycast(groundRay, out hitInfo, mainColliderCapsule.radius * 5.0f)) {
            // Gets the real hitdistance by taking away the radius of the collider capsule
            float hitDistance = hitInfo.distance - mainColliderCapsule.radius;

            // Gets the normal of the surface that's just been hit
            Vector3 surfaceNormal = hitInfo.normal;

            // Checks the angle between the surface normal and down vector
            float surfaceAngle = Vector3.Angle(transform.up, surfaceNormal);

            // If the angle of the surface is less than the maximum slope, continue.
            if (surfaceAngle < MAX_SLOPE) {
                // Gets the maximum distance from the ground the player can be
                float maxDistance = mainColliderCapsule.radius / Mathf.Cos(Mathf.Deg2Rad * surfaceAngle) + 0.02f;
                
                // If the hitDistance is less than the max distance, the player is grounded.
                if (hitDistance < maxDistance) {
                    return true;
                }
            }
        }

        return false;
    }
    
    //--------------------------

    public void AddToCredits(uint creditValue) {
        credits += creditValue;
    }

    public uint GetCredits() {
        return credits;
    }

    //--------------------------

    public void addToOxygen(float oxygenAdd) {
        oxygenLevel = Mathf.Clamp(oxygenLevel + oxygenAdd, 0.0f, 1.0f);
    }

    public float GetOxygen() {
        return oxygenLevel;
    }

    public void SetOxygenDepleting(bool on) {
        oxygenDepleting = on;
    }

    //--------------------------

    public bool GetPaused() {
        return paused;
    }

    public void SetPaused(bool on) {
        paused = on;
    }

    public bool GetDead() {
        return dead;
    }

    public bool GetVictory() {
        return victorious;
    }

    public void SetVictory(bool on) {
        victorious = on;
    }

    //--------------------------

    public bool HasLooked() {
        return playerLooked;
    }

    public bool HasMoved() {
        return playerMoved;
    }

    public bool HasJumped() {
        return playerJumped;
    }

    public bool HasSprinted() {
        return playerSprinted;
    }

    public string GetLastInteraction() {
        return lastInteraction;
    }

    public bool IsNoclipping() {
        return noclip;
    }
}
