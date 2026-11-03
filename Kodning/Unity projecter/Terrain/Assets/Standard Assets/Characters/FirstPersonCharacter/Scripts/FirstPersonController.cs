using UnityEngine;
using UnityEngine.InputSystem;
using UnityStandardAssets.Utility;
using Random = UnityEngine.Random;

namespace UnityStandardAssets.Characters.FirstPerson
{
    [RequireComponent(typeof(CharacterController))]
    [RequireComponent(typeof(AudioSource))]
    [RequireComponent(typeof(PlayerInput))]
    public class FirstPersonController : MonoBehaviour
    {
        [SerializeField] private bool m_IsWalking;
        [SerializeField] private float m_WalkSpeed;
        [SerializeField] private float m_RunSpeed;
        [SerializeField][Range(0f, 1f)] private float m_RunstepLenghten;
        [SerializeField] private float m_JumpSpeed;
        [SerializeField] private float m_StickToGroundForce;
        [SerializeField] private float m_GravityMultiplier;
        [SerializeField] private bool m_UseFovKick;
        [SerializeField] private FOVKick m_FovKick = new FOVKick();
        [SerializeField] private bool m_UseHeadBob;
        [SerializeField] private CurveControlledBob m_HeadBob = new CurveControlledBob();
        [SerializeField] private LerpControlledBob m_JumpBob = new LerpControlledBob();
        [SerializeField] private float m_StepInterval;
        [SerializeField] private AudioClip[] m_FootstepSounds;    // an array of footstep sounds that will be randomly selected from.
        [SerializeField] private AudioClip m_JumpSound;           // the sound played when character leaves the ground.
        [SerializeField] private AudioClip m_LandSound;           // the sound played when character touches back on ground.

        [Header("Input Actions (names in the PlayerInput's action asset)")]
        [SerializeField] private string m_MoveActionName = "Move";
        [SerializeField] private string m_LookActionName = "Look";
        [SerializeField] private string m_JumpActionName = "Jump";
        [SerializeField] private string m_SprintActionName = "Sprint";

        [Header("Look (replaces MouseLook)")]
        [SerializeField] private float m_MouseSensitivity = 0.2f;   // degrees per pixel of mouse movement
        [SerializeField] private float m_StickSensitivity = 180f;   // degrees per second at full stick deflection
        [SerializeField] private bool m_ClampVerticalRotation = true;
        [SerializeField] private float m_MinimumX = -90f;
        [SerializeField] private float m_MaximumX = 90f;
        [SerializeField] private bool m_SmoothLook = false;
        [SerializeField] private float m_SmoothTime = 5f;
        [SerializeField] private bool m_LockCursor = true;

        private Camera m_Camera;
        private bool m_Jump;
        private Vector2 m_Input;
        private Vector3 m_MoveDir = Vector3.zero;
        private CharacterController m_CharacterController;
        private CollisionFlags m_CollisionFlags;
        private bool m_PreviouslyGrounded;
        private Vector3 m_OriginalCameraPosition;
        private float m_StepCycle;
        private float m_NextStep;
        private bool m_Jumping;
        private AudioSource m_AudioSource;

        private PlayerInput m_PlayerInput;
        private InputAction m_MoveAction;
        private InputAction m_LookAction;
        private InputAction m_JumpAction;
        private InputAction m_SprintAction;

        private Quaternion m_CharacterTargetRot;
        private Quaternion m_CameraTargetRot;
        private bool m_CursorIsLocked = true;


        private void Awake()
        {
            m_PlayerInput = GetComponent<PlayerInput>();
            m_MoveAction = FindAction(m_MoveActionName);
            m_LookAction = FindAction(m_LookActionName);
            m_JumpAction = FindAction(m_JumpActionName);
            m_SprintAction = FindAction(m_SprintActionName);
        }


        private InputAction FindAction(string actionName)
        {
            if (string.IsNullOrEmpty(actionName) || m_PlayerInput.actions == null)
            {
                return null;
            }
            InputAction action = m_PlayerInput.actions.FindAction(actionName);
            if (action == null)
            {
                Debug.LogWarning($"FirstPersonController: no input action named '{actionName}' found on PlayerInput.", this);
            }
            return action;
        }


        // Use this for initialization
        private void Start()
        {
            m_CharacterController = GetComponent<CharacterController>();
            m_Camera = Camera.main;
            m_OriginalCameraPosition = m_Camera.transform.localPosition;
            m_FovKick.Setup(m_Camera);
            m_HeadBob.Setup(m_Camera, m_StepInterval);
            m_StepCycle = 0f;
            m_NextStep = m_StepCycle / 2f;
            m_Jumping = false;
            m_AudioSource = GetComponent<AudioSource>();

            m_CharacterTargetRot = transform.localRotation;
            m_CameraTargetRot = m_Camera.transform.localRotation;
        }


        // Update is called once per frame
        private void Update()
        {
            RotateView();
            UpdateCursorLock();

            // the jump state needs to read here to make sure it is not missed
            if (!m_Jump && m_JumpAction != null)
            {
                m_Jump = m_JumpAction.WasPressedThisFrame();
            }

            if (!m_PreviouslyGrounded && m_CharacterController.isGrounded)
            {
                StartCoroutine(m_JumpBob.DoBobCycle());
                PlayLandingSound();
                m_MoveDir.y = 0f;
                m_Jumping = false;
            }
            if (!m_CharacterController.isGrounded && !m_Jumping && m_PreviouslyGrounded)
            {
                m_MoveDir.y = 0f;
            }

            m_PreviouslyGrounded = m_CharacterController.isGrounded;
        }


        private void PlayLandingSound()
        {
            m_AudioSource.clip = m_LandSound;
            m_AudioSource.Play();
            m_NextStep = m_StepCycle + .5f;
        }


        private void FixedUpdate()
        {
            float speed;
            GetInput(out speed);
            // always move along the camera forward as it is the direction that it being aimed at
            Vector3 desiredMove = transform.forward * m_Input.y + transform.right * m_Input.x;

            // get a normal for the surface that is being touched to move along it
            RaycastHit hitInfo;
            Physics.SphereCast(transform.position, m_CharacterController.radius, Vector3.down, out hitInfo,
                               m_CharacterController.height / 2f, Physics.AllLayers, QueryTriggerInteraction.Ignore);
            desiredMove = Vector3.ProjectOnPlane(desiredMove, hitInfo.normal).normalized;

            m_MoveDir.x = desiredMove.x * speed;
            m_MoveDir.z = desiredMove.z * speed;


            if (m_CharacterController.isGrounded)
            {
                m_MoveDir.y = -m_StickToGroundForce;

                if (m_Jump)
                {
                    m_MoveDir.y = m_JumpSpeed;
                    PlayJumpSound();
                    m_Jump = false;
                    m_Jumping = true;
                }
            }
            else
            {
                m_MoveDir += Physics.gravity * m_GravityMultiplier * Time.fixedDeltaTime;
            }
            m_CollisionFlags = m_CharacterController.Move(m_MoveDir * Time.fixedDeltaTime);

            ProgressStepCycle(speed);
            UpdateCameraPosition(speed);
        }


        private void PlayJumpSound()
        {
            m_AudioSource.clip = m_JumpSound;
            m_AudioSource.Play();
        }


        private void ProgressStepCycle(float speed)
        {
            if (m_CharacterController.velocity.sqrMagnitude > 0 && (m_Input.x != 0 || m_Input.y != 0))
            {
                m_StepCycle += (m_CharacterController.velocity.magnitude + (speed * (m_IsWalking ? 1f : m_RunstepLenghten))) *
                             Time.fixedDeltaTime;
            }

            if (!(m_StepCycle > m_NextStep))
            {
                return;
            }

            m_NextStep = m_StepCycle + m_StepInterval;

            PlayFootStepAudio();
        }


        private void PlayFootStepAudio()
        {
            if (!m_CharacterController.isGrounded)
            {
                return;
            }
            // pick & play a random footstep sound from the array,
            // excluding sound at index 0
            int n = Random.Range(1, m_FootstepSounds.Length);
            m_AudioSource.clip = m_FootstepSounds[n];
            m_AudioSource.PlayOneShot(m_AudioSource.clip);
            // move picked sound to index 0 so it's not picked next time
            m_FootstepSounds[n] = m_FootstepSounds[0];
            m_FootstepSounds[0] = m_AudioSource.clip;
        }


        private void UpdateCameraPosition(float speed)
        {
            Vector3 newCameraPosition;
            if (!m_UseHeadBob)
            {
                return;
            }
            if (m_CharacterController.velocity.magnitude > 0 && m_CharacterController.isGrounded)
            {
                m_Camera.transform.localPosition =
                    m_HeadBob.DoHeadBob(m_CharacterController.velocity.magnitude +
                                      (speed * (m_IsWalking ? 1f : m_RunstepLenghten)));
                newCameraPosition = m_Camera.transform.localPosition;
                newCameraPosition.y = m_Camera.transform.localPosition.y - m_JumpBob.Offset();
            }
            else
            {
                newCameraPosition = m_Camera.transform.localPosition;
                newCameraPosition.y = m_OriginalCameraPosition.y - m_JumpBob.Offset();
            }
            m_Camera.transform.localPosition = newCameraPosition;
        }


        private void GetInput(out float speed)
        {
            // Read input
            Vector2 move = m_MoveAction != null ? m_MoveAction.ReadValue<Vector2>() : Vector2.zero;

            bool waswalking = m_IsWalking;

            // walk/run is controlled by the Sprint action (works for keyboard, gamepad, touch, etc.)
            m_IsWalking = m_SprintAction == null || !m_SprintAction.IsPressed();

            // set the desired speed to be walking or running
            speed = m_IsWalking ? m_WalkSpeed : m_RunSpeed;
            m_Input = move;

            // normalize input if it exceeds 1 in combined length:
            if (m_Input.sqrMagnitude > 1)
            {
                m_Input.Normalize();
            }

            // handle speed change to give an fov kick
            // only if the player is going to a run, is running and the fovkick is to be used
            if (m_IsWalking != waswalking && m_UseFovKick && m_CharacterController.velocity.sqrMagnitude > 0)
            {
                StopAllCoroutines();
                StartCoroutine(!m_IsWalking ? m_FovKick.FOVKickUp() : m_FovKick.FOVKickDown());
            }
        }


        private void RotateView()
        {
            if (m_LookAction == null)
            {
                return;
            }

            Vector2 look = m_LookAction.ReadValue<Vector2>();

            // Mouse/pen/touch deltas are already per-frame amounts; stick values are rates and need deltaTime.
            bool fromPointer = m_LookAction.activeControl != null && m_LookAction.activeControl.device is Pointer;
            look *= fromPointer ? m_MouseSensitivity : m_StickSensitivity * Time.deltaTime;

            m_CharacterTargetRot *= Quaternion.Euler(0f, look.x, 0f);
            m_CameraTargetRot *= Quaternion.Euler(-look.y, 0f, 0f);

            if (m_ClampVerticalRotation)
            {
                m_CameraTargetRot = ClampRotationAroundXAxis(m_CameraTargetRot);
            }

            if (m_SmoothLook)
            {
                transform.localRotation = Quaternion.Slerp(transform.localRotation, m_CharacterTargetRot,
                    m_SmoothTime * Time.deltaTime);
                m_Camera.transform.localRotation = Quaternion.Slerp(m_Camera.transform.localRotation, m_CameraTargetRot,
                    m_SmoothTime * Time.deltaTime);
            }
            else
            {
                transform.localRotation = m_CharacterTargetRot;
                m_Camera.transform.localRotation = m_CameraTargetRot;
            }
        }


        private Quaternion ClampRotationAroundXAxis(Quaternion q)
        {
            q.x /= q.w;
            q.y /= q.w;
            q.z /= q.w;
            q.w = 1.0f;

            float angleX = 2.0f * Mathf.Rad2Deg * Mathf.Atan(q.x);
            angleX = Mathf.Clamp(angleX, m_MinimumX, m_MaximumX);
            q.x = Mathf.Tan(0.5f * Mathf.Deg2Rad * angleX);

            return q;
        }


        private void UpdateCursorLock()
        {
            if (!m_LockCursor)
            {
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
                return;
            }

            Keyboard keyboard = Keyboard.current;
            Mouse mouse = Mouse.current;

            if (keyboard != null && keyboard.escapeKey.wasReleasedThisFrame)
            {
                m_CursorIsLocked = false;
            }
            else if (mouse != null && mouse.leftButton.wasReleasedThisFrame)
            {
                m_CursorIsLocked = true;
            }

            Cursor.lockState = m_CursorIsLocked ? CursorLockMode.Locked : CursorLockMode.None;
            Cursor.visible = !m_CursorIsLocked;
        }


        private void OnControllerColliderHit(ControllerColliderHit hit)
        {
            Rigidbody body = hit.collider.attachedRigidbody;
            //dont move the rigidbody if the character is on top of it
            if (m_CollisionFlags == CollisionFlags.Below)
            {
                return;
            }

            if (body == null || body.isKinematic)
            {
                return;
            }
            body.AddForceAtPosition(m_CharacterController.velocity * 0.1f, hit.point, ForceMode.Impulse);
        }
    }
}