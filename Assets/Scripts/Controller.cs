using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class Controller : MonoBehaviour
{
    public static Controller Instance { get; protected set; }
    public Camera MainCamera;
    public Transform CameraPosition;
    public AudioSource footSource;
    public AudioClip[] clips;

    [Header("Control Settings")]
    public float MouseSensitivity = 100.0f;
    public float PlayerSpeed = 5.0f;
    public float RunningSpeed = 7.0f;
    public float JumpSpeed = 5.0f;
    public float mass = 70f;
    public float slopeSpeed = 8f;

    float m_VerticalSpeed = 0.0f;
    bool m_IsPaused = false;
    public bool m_Dead = false;
    
    float m_VerticalAngle, m_HorizontalAngle;
    public float Speed { get; private set; } = 0.0f;

    public bool LockControl { get; set; }
    private bool LockCamera;

    public bool Grounded => m_Grounded;
    int step;

    CharacterController m_CharacterController;

    bool m_Grounded;
    bool m_CanGetUp;
    bool m_Crounching;
    bool m_UsingVehicle = false;

    float m_GroundedTimer;
    float m_SpeedAtJump = 0.0f;
    float oldHeight;

    [Header("Head Bobbing Settings")]
    public float walkingBobbingSpeed = 14f;
    public float bobbingAmount = 0.05f;

    float defaultPosY = 0;
    float timer = 0;

    //Sliding Parameters
    private Vector3 hitPointNormal;

    private bool IsSliding
    {
        get
        {
            Debug.DrawRay(transform.position, Vector3.down*1.5f,Color.red);
            if (m_CharacterController.isGrounded && Physics.Raycast(transform.position, Vector3.down, out RaycastHit slopeHit, 1.5f))
            {
                hitPointNormal = slopeHit.normal;
                return Vector3.Angle(hitPointNormal, Vector3.up) > m_CharacterController.slopeLimit;
            }
            else
            {
                return false;
            }
        }
    }

    void Awake()
    {
        Instance = this;
    }
    
    void Start()
    {     
        m_IsPaused = false;
        m_Grounded = true;
        m_CanGetUp = true;


        MainCamera.transform.SetParent(CameraPosition, false);
        MainCamera.transform.localPosition = Vector3.zero;
        MainCamera.transform.localRotation = Quaternion.identity;
        m_CharacterController = GetComponent<CharacterController>();
        oldHeight = m_CharacterController.height;

        m_VerticalAngle = 0.0f;
        m_HorizontalAngle = transform.localEulerAngles.y;

        defaultPosY = MainCamera.transform.localPosition.y;

        DisplayCursor(false);
    }

    bool running;
    Vector3 move;
    bool loosedGrounding;
    void Update()
    {
        bool wasGrounded = m_Grounded;
        loosedGrounding = false;

        if (!m_CharacterController.isGrounded)
        {
            if (m_Grounded)
            {
                m_GroundedTimer += Time.deltaTime;
                if (m_GroundedTimer >= 0.5f)
                {
                    loosedGrounding = true;
                    m_Grounded = false;
                }
            }
        }
        else
        {
            m_GroundedTimer = 0.0f;
            m_Grounded = true;
        }

        // Lock Control if paused
        if (m_IsPaused) LockControl = true;
        else LockControl = false;

        Speed = 0;
        move = Vector3.zero;
        if (!LockControl)
        {
            if (!m_UsingVehicle && m_CharacterController.enabled) UpdateMove();
            if (!LockCamera)
            {
                UpdateCamera();
                if (!m_UsingVehicle) BobHead();
            }
        }
        else IdleCamera();
        if(m_CharacterController.enabled) Falldown();
    }
    private void UpdateMove()
    {
        // Jump (we do it first as 
        if (m_Grounded && Input.GetButtonDown("Jump"))
        {
            footSource.PlayOneShot(clips[3], 0.5f);
            m_VerticalSpeed = JumpSpeed;
            m_Grounded = false;
            loosedGrounding = true;
        }

        running = Input.GetButton("Run");
        if (m_Crounching) running = false;
        float actualSpeed = running ? RunningSpeed : PlayerSpeed;

        // Crouch movement
        float height = m_CharacterController.height;
        float crouchedHeight = oldHeight / 2;
        if (Input.GetButton("Crouch"))
        {
            height = Mathf.Lerp(height, crouchedHeight, .5f);
            m_CharacterController.height = height;
            m_Crounching = true;
        }
        else if (m_CanGetUp)
        {
            height = Mathf.Lerp(height, oldHeight, .5f);
            m_CharacterController.height = height;
            m_Crounching = false;
        }

        if (Input.GetButton("Fire2"))
        {
            MainCamera.fieldOfView = Mathf.Lerp(MainCamera.fieldOfView, 20, .5f);
        }
        else
        {
            MainCamera.fieldOfView = Mathf.Lerp(MainCamera.fieldOfView, 60, .5f);
        }

        if (loosedGrounding)
        {
            m_SpeedAtJump = actualSpeed;
        }

        // Move around with WASD
        move = new Vector3(Input.GetAxis("Horizontal"), 0, Input.GetAxisRaw("Vertical"));
        if (move.sqrMagnitude > 1.0f)
            move.Normalize();

        float usedSpeed = m_Grounded ? actualSpeed : m_SpeedAtJump;

        move = move * usedSpeed * Time.deltaTime;

        move = transform.TransformDirection(move);

        m_CharacterController.Move(move);
    }
    private void UpdateCamera()
    {
        // Turn player
        float turnPlayer = Input.GetAxis("Mouse X") * MouseSensitivity;
        m_HorizontalAngle = m_HorizontalAngle + turnPlayer;

        if (m_HorizontalAngle > 360) m_HorizontalAngle -= 360.0f;
        if (m_HorizontalAngle < 0) m_HorizontalAngle += 360.0f;

        Vector3 currentAngles = transform.localEulerAngles;
        currentAngles.y = m_HorizontalAngle;
        transform.localEulerAngles = currentAngles;

        // Camera look up/down
        var turnCam = -Input.GetAxis("Mouse Y");
        turnCam = turnCam * MouseSensitivity;
        m_VerticalAngle = Mathf.Clamp(turnCam + m_VerticalAngle, -89.0f, 89.0f);
        currentAngles = CameraPosition.transform.localEulerAngles;
        currentAngles.x = m_VerticalAngle;
        CameraPosition.transform.localEulerAngles = currentAngles;

        Speed = move.magnitude / (PlayerSpeed * Time.deltaTime);
    }
    public void RecoilCamera()
    {
        m_HorizontalAngle += Random.Range(-1f, 1f);
        m_VerticalAngle += Random.Range(-3f, -1f);
    }
    private void BobHead()
    {

        float actualBobSpeed;
        if (running) actualBobSpeed = walkingBobbingSpeed * (RunningSpeed / PlayerSpeed);
        else actualBobSpeed = walkingBobbingSpeed;

        //Bob Head
        if ((Mathf.Abs(Input.GetAxis("Horizontal")) > 0.1f || Mathf.Abs(Input.GetAxis("Vertical")) > 0.1f) && m_Grounded && !m_Crounching)
        {
            if (!footSource.isPlaying && MainCamera.transform.localPosition.y < -0.05f)
            {
                step++;
                if (step == 2) step = 0;
                footSource.PlayOneShot(clips[step], m_CharacterController.velocity.magnitude / 5f);
            }

            //Player is moving
            timer += Time.deltaTime * actualBobSpeed;
            MainCamera.transform.localPosition = new Vector3(MainCamera.transform.localPosition.x, defaultPosY + Mathf.Sin(timer) * bobbingAmount, MainCamera.transform.localPosition.z);
        }
        else
        {
            IdleCamera();
        }
    }
    private void IdleCamera()
    {
        //Idle
        timer = 0;
        MainCamera.transform.localPosition = new Vector3(MainCamera.transform.localPosition.x, Mathf.Lerp(MainCamera.transform.localPosition.y, defaultPosY, Time.deltaTime * walkingBobbingSpeed), MainCamera.transform.localPosition.z);
    }
    private void Falldown()
    {
        // Fall down / gravity
        m_VerticalSpeed = m_VerticalSpeed - 10.0f * Time.deltaTime;
        if (m_VerticalSpeed < -10.0f)
            m_VerticalSpeed = -10.0f; // max fall speed
        var verticalMove = new Vector3(0, m_VerticalSpeed * Time.deltaTime, 0);
        var flag = m_CharacterController.Move(verticalMove);
        if ((flag & CollisionFlags.Below) != 0)
            m_VerticalSpeed = 0;
    }

    public void SpawnPosition(Vector3 pos)
    {
        m_CharacterController = GetComponent<CharacterController>();
        m_CharacterController.enabled = false;
        transform.position = pos;
        m_CharacterController.enabled = true;
    }
    public IEnumerator LerpPosition(Transform target, float time)
    {
        m_CharacterController = GetComponent<CharacterController>();
        m_CharacterController.enabled = false;
        for (int i = 0; i < time; i++)
        {
            transform.position = Vector3.Lerp(transform.position, target.position, 0.3f);
            transform.rotation = Quaternion.Lerp(transform.rotation, target.rotation, 0.3f);
            yield return new WaitForSeconds(0.01f);
        }
        transform.position = target.position;
        transform.rotation = target.rotation;
        m_CharacterController.enabled = true;
    }
    public IEnumerator Backstab(Transform target)
    {
        LockCamera = true;
        Transform tarPar = target.parent.transform;
        m_CharacterController = GetComponent<CharacterController>();
        m_CharacterController.enabled = false;
        for (int i = 0; i < 96; i++)
        {
            tarPar.rotation = new Quaternion(tarPar.rotation.x, tarPar.rotation.y, 0, tarPar.rotation.w);
            transform.position = Vector3.Lerp(transform.position, target.position, 0.3f);
            transform.rotation = Quaternion.Lerp(transform.rotation, tarPar.rotation, 0.3f);
            MainCamera.transform.rotation = Quaternion.Lerp(MainCamera.transform.rotation, target.rotation, 0.3f);
            yield return new WaitForSeconds(0.01f);
        }
        transform.position = target.position;
        transform.rotation = tarPar.rotation;
        MainCamera.transform.rotation = target.rotation;
        LockCamera = false;
        m_CharacterController.enabled = true;
    }
    public IEnumerator LerpPosition(Transform target, Transform parent, bool inVehicle)
    {
        if(inVehicle) m_UsingVehicle = inVehicle;
        m_CharacterController = GetComponent<CharacterController>();
        m_CharacterController.enabled = false;
        for (int i = 0; i < 4; i++)
        {
            transform.position = Vector3.Lerp(transform.position, target.position, 0.3f);
            transform.rotation = Quaternion.Lerp(transform.rotation, target.rotation, 0.3f);
            yield return new WaitForSeconds(0.01f);
        }
        transform.position = target.position;
        transform.rotation = target.rotation;
        transform.SetParent(parent);
        m_CharacterController.enabled = !inVehicle;
        m_UsingVehicle = inVehicle;
    }


    bool canLandImpact = true;
    private void OnControllerColliderHit(ControllerColliderHit hit)
    {
        if(hit.gameObject.tag == "Land" && !m_Grounded && canLandImpact)
        {
            float volume = m_CharacterController.velocity.y * 0.075f;
            if (m_CharacterController.velocity.y  == 0) volume = 0.2f;
            footSource.PlayOneShot(clips[2], volume);
            StartCoroutine(CheapLandSolution());
        }
    }
    IEnumerator CheapLandSolution()
    {
        canLandImpact = false;
        yield return new WaitForSeconds(0.3f);
        canLandImpact = true;
    }

    public void DisplayCursor(bool display)
    {
        m_IsPaused = display;
        Cursor.lockState = display ? CursorLockMode.None : CursorLockMode.Locked;
        Cursor.visible = display;
    }
}
