using System.Collections;
using System.Collections.Generic;
using UnityEngine;
//[RequireComponent(typeof (AudioSource))]
public class FPSController : MonoBehaviour

{
    private bool isSprinting=> canSprint && Input.GetKey(sprintKey);
    private bool shouldJump=> Input.GetKeyDown(jumpKey) && playerControl.isGrounded;
    private bool shouldCrouch=> Input.GetKeyDown(crouchKey) && playerControl.isGrounded && !duringCrouchingAnimation;
    [Header("Functional Options")]
    [SerializeField] private bool canSprint=true;
    [SerializeField] private bool canJump=true;
    [SerializeField] private bool canCrouch=true;
    [SerializeField] private bool canUseHeadbob=true;
    [SerializeField] private bool canInteract=true;
    [SerializeField] private bool WillSlideOnSlopes=true;
    [SerializeField] private bool CanZoom=true;
    [SerializeField] private bool useFootsteps=true;
    [Header("Controls")]
    [SerializeField] private KeyCode sprintKey=KeyCode.LeftShift;
    [SerializeField] private KeyCode jumpKey=KeyCode.Space;
    [SerializeField] private KeyCode crouchKey=KeyCode.LeftControl;
    [SerializeField] private KeyCode interactionKey=KeyCode.E;
    [SerializeField] private KeyCode zoomKey=KeyCode.Mouse1;
    public bool canMove{get;private set;} = true;
    [Header("Movement Parameters")]
    [SerializeField] private float walkSpd=3.0f;
    [SerializeField] private float SprintSpd=6.0f;
    [SerializeField] private float CrouchSpd=1.5f;
    [SerializeField] private float slopeSpd=8f;
    [Header("Headbob Parametres")]
    [SerializeField] private float walkBobSpd=14f;
    [SerializeField] private float walkBobAmout=0.05f;
    [SerializeField] private float sprintBobSpd=18f;
    [SerializeField] private float sprintBobAmout=0.1f;
    [SerializeField] private float crouchBobSpd=8f;
    [SerializeField] private float crouchBobAmout=0.025f;
    //Sloping paremetres
    private Vector3 HitNormalPoint;
    private bool isSliding{
        get
            {
                if(playerControl.isGrounded&&Physics.Raycast(transform.position,Vector3.down,out RaycastHit slopeHit, 2f)){
                    HitNormalPoint = slopeHit.normal;
                    Debug.Log(Vector3.Angle(HitNormalPoint,Vector3.up)>playerControl.slopeLimit);
                    return Vector3.Angle(HitNormalPoint,Vector3.up)>playerControl.slopeLimit;
                }else{
                    return false;
                }
            }
        }
    [Header("Interaction Parametres")]
    [SerializeField] private GameObject crosshair;
    [SerializeField] private float interactionDistance=2f;
    [SerializeField] private LayerMask interactionLayer=default;
    private Vector3 interactionRayPoint = new Vector3(0.5f,0.5f,0);
    private Interactable currentInteractable;

    private float defaultYpos=0;
    private float timer;
    [Header("Jump Parametres")]
    [SerializeField] private float jumpForce=8.0f;
    [SerializeField] private float gravity = 30f;
    [Header("Crouch Parametres")]
    [SerializeField] private float crouchHeight=0.5f;
    [SerializeField] private float standingHeight=2.0f;
    [SerializeField] private float crouchTime=0.25f;
    [SerializeField] private Vector3 crouchCenter=new Vector3(0,0.5f,0);
    [SerializeField] private Vector3 standingCenter=new Vector3(0,0f,0);
    private bool isCrouching;
    private bool duringCrouchingAnimation;
    [Header("Look Parameters")]
    [SerializeField, Range(1,10)] private float mouseSpdX=2.0f;
    [SerializeField, Range(1,10)] private float mouseSpdY=2.0f;
    [SerializeField, Range(1,180)] private float UpperLookMax=80.0f;
    [SerializeField, Range(1,180)] private float LowerLookMax=80.0f;
    [SerializeField] private bool invertYaxis=false; 
    [Header("Zoom Parametres")]
    [SerializeField] private float timeToZoom=0.3f;
    [SerializeField] private float zoomFOV=30f;
    [Header("Animation Paremetres")]
    [SerializeField] private Animator animator;
    [Header("Audio Parametres")]
    [SerializeField,Range(0.5f,5f)] private float baseStepSpeed=0.5f;
    [SerializeField,Range(0.5f,5f)] private float crouchStepMultipler=1.5f;
    [SerializeField,Range(0.5f,5f)] private float sprintStepMultipler=0.6f;
    [SerializeField] private AudioSource audioSource=default;
    [SerializeField] private AudioClip[] defaultFootstepSounds=default;// an array of footstep sounds that will be randomly selected from.
    [SerializeField] private AudioClip[] woodFootstepSounds=default;
    [SerializeField] private AudioClip[] grassFootstepSounds=default;
    [SerializeField] private AudioClip[] metalFootstepSounds=default;
    [SerializeField] private AudioClip jumpSound;// the sound played when character leaves the ground.
    [SerializeField] private AudioClip landSound;// the sound played when character touches back on ground.
    private float footstepTimer=0;
    private float GetCurrentOffset=> isCrouching? baseStepSpeed*crouchStepMultipler:isSprinting? baseStepSpeed*sprintStepMultipler: baseStepSpeed;
    private float defaultFOV;
    private Coroutine zoomRotine;
    private int dirV;
    private int dirH;
    private Camera playerCamera;
    public Camera FPCamera;
    private CharacterController playerControl;
    private Vector3 direction;
    private Vector2 currentInput; 
    private bool isFalling;
    private float yaw=0.0f;
    // Start is called before the first frame update
     void Awake() {
        playerCamera = GetComponentInChildren<Camera>();
        playerControl = GetComponent<CharacterController>();
            audioSource = GetComponent<AudioSource>();
        defaultFOV = playerCamera.fieldOfView;
        defaultYpos = playerCamera.transform.localPosition.y;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }
    // Update is called once per frame
    void Update()
    {
        if(canMove){
            HandleMovimentInput();
            HandleMovimentLook();
            if(canJump)
            HandleJump();
            if(canCrouch)
            HandleCrouch();
            if(canUseHeadbob)
            HandleHeadbob();
            if(canInteract){
            HandleInteractionCheck();
            HandleInteractionInput();
            }
            if(CanZoom)
            HandleZoom();
            ApplyFinalMovemenst();
            if(useFootsteps)
            footStpesHandler();
        }
        if(!playerControl.isGrounded){
            isFalling=true;
        }
        if(isFalling&&playerControl.isGrounded){
            isFalling=false;
            LandSound();
        }
        // Debug.Log(playerControl.velocity);
    }
    private void HandleMovimentInput(){    
        currentInput = new Vector2(( isCrouching ? CrouchSpd : isSprinting ? SprintSpd : walkSpd)*Input.GetAxis("Vertical"),
        ( isCrouching ? CrouchSpd : isSprinting ? SprintSpd : walkSpd)*Input.GetAxis("Horizontal"));
        float moveDirY = direction.y;
        direction = (transform.TransformDirection(Vector3.forward)*currentInput.x)+(transform.TransformDirection(Vector3.right)*currentInput.y);
        direction.y = moveDirY;
    }
    private void HandleMovimentLook(){
        yaw += Input.GetAxis("Mouse Y")*mouseSpdX*(invertYaxis? 1 : -1);
        yaw = Mathf.Clamp(yaw,-UpperLookMax,LowerLookMax);
        playerCamera.transform.localRotation = Quaternion.Euler(yaw,0,0);
        if(FPCamera!=null)
        FPCamera.transform.localRotation = Quaternion.Euler(yaw,0,0);
        transform.rotation *= Quaternion.Euler(0,Input.GetAxis("Mouse X")*mouseSpdY,0);
    }
    private void HandleJump(){
        if(shouldJump)
        direction.y = jumpForce;
    }
    private void HandleCrouch(){
        if(shouldCrouch){
            StartCoroutine(CrouchStand());
        }
    }
    private void HandleHeadbob(){
        if(!playerControl.isGrounded)
            return;
        if(Mathf.Abs(direction.x)>0.1f||Mathf.Abs(direction.z)>0.1f){
            timer += Time.deltaTime*(isCrouching ? crouchBobSpd : isSprinting ? sprintBobSpd : walkBobSpd);
            playerCamera.transform.localPosition = new Vector3(playerCamera.transform.localPosition.x,
            defaultYpos+Mathf.Sin(timer)*(isCrouching ? crouchBobAmout : isSprinting ? sprintBobAmout : walkBobAmout),
            playerCamera.transform.localPosition.z);
        }
    }
    private void HandleInteractionCheck(){
        if(Physics.Raycast(playerCamera.ViewportPointToRay(interactionRayPoint), out RaycastHit hit,interactionDistance)){
            if(hit.collider.gameObject.layer == 11&&(currentInteractable==null||hit.collider.gameObject.GetInstanceID()!=currentInteractable.GetInstanceID()&&(currentInteractable==null))){
                hit.collider.TryGetComponent(out currentInteractable);
                if(currentInteractable){
                    if(crosshair!=null)
                    crosshair.SetActive(true);
                    currentInteractable.OnFocus();
                    }
                 }
        }else if(currentInteractable){
                    if(crosshair!=null)
                        crosshair.SetActive(false);
                    currentInteractable.OnLoseFocus();
                    currentInteractable=null;
                }
            
        
    }
    private void HandleInteractionInput(){
        if(Input.GetKey(interactionKey)&&currentInteractable!=null&&Physics.Raycast(playerCamera.ViewportPointToRay(interactionRayPoint),out RaycastHit hit,interactionDistance,interactionLayer)){
            currentInteractable.OnInteract();
        }else if(currentInteractable!=null)
             currentInteractable.OffInteract();
    }
    private void HandleZoom(){
        if(Input.GetKeyDown(zoomKey)){
            if(zoomRotine!=null){
                StopCoroutine(zoomRotine);
                zoomRotine=null;
            }
            zoomRotine=StartCoroutine(toggleZoom(true));
        }
        if(Input.GetKeyUp(zoomKey)){
            if(zoomRotine!=null){
                StopCoroutine(zoomRotine);
                zoomRotine=null;
            }
            zoomRotine=StartCoroutine(toggleZoom(false));
        }
    }
    private void ApplyFinalMovemenst(){
        if(!playerControl.isGrounded){
            direction.y -= gravity*Time.deltaTime;
           
            }
        if(WillSlideOnSlopes&&isSliding){
            direction = new Vector3(HitNormalPoint.x,-HitNormalPoint.y,HitNormalPoint.z)*slopeSpd;
        }
            playerControl.Move(direction*Time.deltaTime);
        
    }
    private IEnumerator CrouchStand(){
        if(isCrouching && Physics.Raycast(playerCamera.transform.position,Vector3.up,1f))
        yield break;

        duringCrouchingAnimation=true;
        float timeElapsed=0;
        float targetHeight = isCrouching ? standingHeight : crouchHeight;
        float currentHeight = playerControl.height;
        Vector3 targetCenter = isCrouching ? standingCenter : crouchCenter;
        Vector3 currentCenter = playerControl.center;
        while(timeElapsed<crouchTime){
            playerControl.height = Mathf.Lerp(currentHeight,targetHeight,timeElapsed/crouchTime);
            playerControl.center = Vector3.Lerp(currentCenter,targetCenter,timeElapsed/crouchTime);
            timeElapsed+=Time.deltaTime;
            yield return null;
        }
            playerControl.height = targetHeight;
            playerControl.center = targetCenter;

            isCrouching = !isCrouching;
        duringCrouchingAnimation=false;
    }
    private IEnumerator toggleZoom(bool isEnter){
        float targetFOV = isEnter ? zoomFOV:defaultFOV;
        float startingFOV = playerCamera.fieldOfView;
        float timeElapsed=0f;
        while(timeElapsed<timeToZoom){
            playerCamera.fieldOfView = Mathf.Lerp(startingFOV,targetFOV,timeElapsed/timeToZoom);
            timeElapsed+=Time.deltaTime;
            yield return null;
        }
        playerCamera.fieldOfView=targetFOV;
        zoomRotine=null;
    }
     private void footStpesHandler(){
        if(!playerControl.isGrounded) return;
        if(currentInput==Vector2.zero) return;
        if(isSliding) return;
        footstepTimer-=Time.deltaTime;
        if(footstepTimer<=0){
            if(Physics.Raycast(playerCamera.transform.position,Vector3.down,out RaycastHit hit,3f)){
                    switch(hit.collider.tag){
                        case "Grass":
                            audioSource.PlayOneShot(grassFootstepSounds[Random.Range(0,grassFootstepSounds.Length-1)]);
                            break;
                        case "Metal":
                            audioSource.PlayOneShot(metalFootstepSounds[Random.Range(0,metalFootstepSounds.Length-1)]);
                            break;
                        case "Wood":
                            audioSource.PlayOneShot(woodFootstepSounds[Random.Range(0,woodFootstepSounds.Length-1)]);
                            break;
                        default: 
                            audioSource.PlayOneShot(defaultFootstepSounds[Random.Range(0,defaultFootstepSounds.Length-1)]);
                            break;
                    }
            }
            footstepTimer=GetCurrentOffset;
        }
     }
     private void LandSound(){
        if(!isCrouching&&!isSliding)
        audioSource.PlayOneShot(landSound);
     }

}

