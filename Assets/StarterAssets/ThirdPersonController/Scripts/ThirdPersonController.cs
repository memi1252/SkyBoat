using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if ENABLE_INPUT_SYSTEM && STARTER_ASSETS_PACKAGES_CHECKED
using UnityEngine.InputSystem;
#endif

/* 참고: 애니메이션은 애니메이터 널(null) 체크를 통해 캐릭터와 캡슐 양쪽에서 컨트롤러를 통해 호출됩니다 */

namespace StarterAssets
{
    [RequireComponent(typeof(CharacterController))]
#if ENABLE_INPUT_SYSTEM && STARTER_ASSETS_PACKAGES_CHECKED
    [RequireComponent(typeof(PlayerInput))]
#endif
    public class ThirdPersonController : MonoBehaviour
    {
        [Header("Player")] [Tooltip("캐릭터의 이동 속도 (m/s)")]
        public float MoveSpeed = 2.0f;

        [Tooltip("캐릭터의 달리기 속도 (m/s)")] public float SprintSpeed = 5.335f;

        [Tooltip("캐릭터가 이동 방향을 바라보도록 회전하는 속도")] [Range(0.0f, 0.3f)]
        public float RotationSmoothTime = 0.12f;

        [Tooltip("가속 및 감속")] public float SpeedChangeRate = 10.0f;

        public AudioClip LandingAudioClip;
        public AudioClip[] FootstepAudioClips;
        [Range(0, 1)] public float FootstepAudioVolume = 0.5f;

        [Space(10)] [Tooltip("플레이어가 점프할 수 있는 높이")]
        public float JumpHeight = 1.2f;

        [Tooltip("캐릭터가 자체 중력값을 사용합니다. 엔진 기본값은 -9.81f입니다")]
        public float Gravity = -15.0f;

        [Space(10)] [Tooltip("다시 점프할 수 있기까지 걸리는 시간. 0f로 설정하면 즉시 재점프가 가능합니다")]
        public float JumpTimeout = 0.50f;

        [Tooltip("낙하 상태에 진입하기 전에 필요한 시간. 계단을 내려갈 때 유용합니다")]
        public float FallTimeout = 0.15f;

        [Header("Player Grounded")] [Tooltip("캐릭터가 지면에 닿아 있는지 여부. CharacterController의 내장 grounded 체크와는 별개입니다")]
        public bool Grounded = true;

        [Tooltip("울퉁불퉁한 지면에 유용한 오프셋")] public float GroundedOffset = -0.14f;

        [Tooltip("지면 체크 구의 반지름. CharacterController의 반지름과 일치해야 합니다")]
        public float GroundedRadius = 0.28f;

        [Tooltip("캐릭터가 지면으로 사용할 레이어들")] public LayerMask GroundLayers;

        [Header("Cinemachine")] [Tooltip("Cinemachine 가상 카메라에서 카메라가 따라갈 대상")]
        public GameObject CinemachineCameraTarget;

        [Tooltip("카메라를 위로 얼마나 움직일 수 있는지(도 단위)")]
        public float TopClamp = 70.0f;

        [Tooltip("카메라를 아래로 얼마나 움직일 수 있는지(도 단위)")]
        public float BottomClamp = -30.0f;

        [Tooltip("잠금 상태에서 카메라 위치를 미세조정할 때 사용할 추가 각도 오프셋")]
        public float CameraAngleOverride = 0.0f;

        [Tooltip("모든 축에서 카메라 위치를 고정할지 여부")] public bool LockCameraPosition = false;

        [Tooltip(("아이템 감지 거리"))] public float sensineRange = 3f;

        // iStep Starter Assets 캐릭터 컨트롤러 개선사항
        [Header("iStep Demo Extension")]
        [SerializeField, Tooltip("이 변수를 true로 설정하면 런타임에 비활성화되는 레이어 충돌 쌍을 확인하려면 이 스크립트의 Awake 함수를 확인하세요.")]
        protected bool m_applyCustomRuntimeGlobalPhysicsIgnoreCollisionSettings = true;

        [SerializeField, Tooltip("iStep의 알고리즘적 '지면 감지' 개선사항을 활성화/비활성화합니다")]
        protected bool m_useIStepGroundedImprovements = true;

        public bool useIstepGroundedImprovements
        {
            get { return m_useIStepGroundedImprovements; }
            set { m_useIStepGroundedImprovements = value; }
        }

        [SerializeField, Range(0.01f, 1), Tooltip("심연(abyss) 감지에 사용되는 spherecast 반지름은 이 값과 Grounded Radius의 곱입니다")]
        protected float m_checkAbyssColliderRadiusMultiplier = 0.05f;

        protected Vector3 m_slopeVelocity = Vector3.zero;
        protected Vector3 m_prevAppliedSlopeVelocity = Vector3.zero;
        protected Vector3 m_currVelDir;
        protected Vector3 m_groundedNormal;

        public Vector3 groundedNormal
        {
            get { return m_groundedNormal; }
        }

        // cinemachine
        private float _cinemachineTargetYaw;
        private float _cinemachineTargetPitch;

        // player
        private float _speed;
        private float _animationBlend;
        private float _targetRotation = 0.0f;
        private float _rotationVelocity;
        private float _verticalVelocity;
        private float _terminalVelocity = -53.0f;

        // timeout deltatime
        private float _jumpTimeoutDelta;
        private float _fallTimeoutDelta;

        // animation IDs
        private int _animIDSpeed;
        private int _animIDGrounded;
        private int _animIDJump;
        private int _animIDFreeFall;
        private int _animIDMotionSpeed;
        private int _animIDPickUp;
        private int _animIDAttackRight;
        private int _animIDAttackLeft;

        //무기 상태
        public bool none;
        public bool greatSword;



        private bool attackRight = true;

#if ENABLE_INPUT_SYSTEM && STARTER_ASSETS_PACKAGES_CHECKED
        private PlayerInput _playerInput;
#endif
        public Animator _animator;
        private CharacterController _controller;
        private StarterAssetsInputs _input;
        private PlayerQuickSlot _playerQuickSlot;
        private GameObject _mainCamera;

        private const float _threshold = 0.01f;

        private bool _hasAnimator;

        private bool IsCurrentDeviceMouse
        {
            get
            {
#if ENABLE_INPUT_SYSTEM && STARTER_ASSETS_PACKAGES_CHECKED
                return _playerInput.currentControlScheme == "KeyboardMouse";
#else
				return false;
#endif
            }
        }


        private void Awake()
        {
            // iStep Starter Assets 캐릭터 컨트롤러 개선사항
            if (m_applyCustomRuntimeGlobalPhysicsIgnoreCollisionSettings)
            {
                // UI 및 기타 레이어에 대한 레이어 충돌 무시 설정
                Physics.IgnoreLayerCollision(0, 5);
                Physics.IgnoreLayerCollision(1, 5);
                Physics.IgnoreLayerCollision(2, 5);
                Physics.IgnoreLayerCollision(4, 5);
                Physics.IgnoreLayerCollision(5, 5);
            }

            // 메인 카메라 참조 가져오기
            if (_mainCamera == null)
            {
                _mainCamera = GameObject.FindGameObjectWithTag("MainCamera");
            }

            Cursor.visible = true;
        }

        private void OnEnable()
        {
            _cinemachineTargetYaw = CinemachineCameraTarget.transform.rotation.eulerAngles.y;
            _cinemachineTargetPitch = CinemachineCameraTarget.transform.rotation.eulerAngles.x;

            // 시작 시 타임아웃 초기화
            _jumpTimeoutDelta = JumpTimeout;
            _fallTimeoutDelta = FallTimeout;
        }

        private void Start()
        {
            _cinemachineTargetYaw = CinemachineCameraTarget.transform.rotation.eulerAngles.y;
            _cinemachineTargetPitch = CinemachineCameraTarget.transform.rotation.eulerAngles.x;

            //_hasAnimator = TryGetComponent(out _animator);
            _animator = this.GetComponentInChildren<Animator>();
            _hasAnimator = _animator != null;
            _controller = GetComponent<CharacterController>();
            _input = GetComponent<StarterAssetsInputs>();
            _playerQuickSlot = GetComponent<PlayerQuickSlot>();
#if ENABLE_INPUT_SYSTEM && STARTER_ASSETS_PACKAGES_CHECKED
            _playerInput = GetComponent<PlayerInput>();
#else
			Debug.LogError( "Starter Assets package is missing dependencies. Please use Tools/Starter Assets/Reinstall Dependencies to fix it");
#endif

            AssignAnimationIDs();

            // 시작 시 타임아웃 초기화
            _jumpTimeoutDelta = JumpTimeout;
            _fallTimeoutDelta = FallTimeout;
        }

        private void Update()
        {
            //_hasAnimator = TryGetComponent(out _animator);

            JumpAndGravity();
            if (m_useIStepGroundedImprovements) improvedGroundedCheckByiStep(); // iStep의 개선된 grounded 체크
            else GroundedCheck(); // 기본 grounded 체크

            Move();

            QuickSlot();

            Attack();

            Interaction();
        }

        private void LateUpdate()
        {
            CameraRotation();
        }

        private void AssignAnimationIDs()
        {
            _animIDSpeed = Animator.StringToHash("Speed");
            _animIDGrounded = Animator.StringToHash("Grounded");
            _animIDJump = Animator.StringToHash("Jump");
            _animIDFreeFall = Animator.StringToHash("FreeFall");
            _animIDMotionSpeed = Animator.StringToHash("MotionSpeed");
            _animIDPickUp = Animator.StringToHash("PickUp");
            _animIDAttackRight = Animator.StringToHash("AttackRight");
            _animIDAttackLeft = Animator.StringToHash("AttackLeft");
        }

        private void improvedGroundedCheckByiStep()
        {
            // Function Copyright © Kreshnik Halili

            Vector3 charVelDir = _controller.velocity;
            charVelDir.y = 0;
            if (charVelDir.magnitude > 0.01f) m_currVelDir = charVelDir.normalized;

            m_groundedNormal = Vector3.up;

            if (_verticalVelocity > 0)
            {
                Grounded = false;

                m_slopeVelocity = Vector3.Lerp(m_slopeVelocity, Vector3.zero, Time.deltaTime * 20.0f);

                // 애니메이터가 있는 경우 애니메이터에 상태 업데이트
                if (_hasAnimator)
                {
                    _animator.SetBool(_animIDGrounded, Grounded);
                }

                return;
            }

            GroundedRadius = _controller.radius * Mathf.Max(transform.localScale.x, transform.localScale.z);

            Vector3 targetSlopeVelocity = Vector3.zero;

            RaycastHit hit;
            float positiveGroundedOffset = Mathf.Abs(GroundedOffset);
            Vector3 origin = transform.position + transform.up * (GroundedRadius + positiveGroundedOffset);
            float twoTimesPositiveGroundedOffset = positiveGroundedOffset * 2;
            if (Physics.SphereCast(origin, GroundedRadius, -transform.up, out hit, twoTimesPositiveGroundedOffset,
                    GroundLayers, QueryTriggerInteraction.Ignore))
            {
                // 이 히트의 반대편에서 다른 오브젝트와 충돌하는지 확인
                Vector3 scaledCenterOffsetVec = _controller.center;
                scaledCenterOffsetVec.x *= transform.localScale.x;
                scaledCenterOffsetVec.y *= transform.localScale.y;
                scaledCenterOffsetVec.z *= transform.localScale.z;
                float scaledHeight = _controller.height * transform.localScale.y;
                Vector3 p1 = transform.position + scaledCenterOffsetVec +
                             transform.up * (scaledHeight * 0.5f - GroundedRadius);
                Vector3 p2 = transform.position + scaledCenterOffsetVec -
                             transform.up * (scaledHeight * 0.5f - GroundedRadius);

                float penetrationCastRadius = GroundedRadius * 0.99f;
                float additionalRayDist = GroundedRadius - penetrationCastRadius;

                Vector3 rayDirDistVec = transform.position - hit.point;
                Vector3 rayDirVec = rayDirDistVec.normalized;
                Vector3 projRayDistVec = Vector3.ProjectOnPlane(rayDirDistVec, transform.up);
                float lengthX = Mathf.Max(projRayDistVec.magnitude, 0.01f);
                float lengthX2 = GroundedRadius - lengthX;
                Vector3 hypothenuse2 = (lengthX2 / lengthX) * rayDirDistVec +
                                       rayDirVec * (additionalRayDist + twoTimesPositiveGroundedOffset);

                if (Physics.CapsuleCast(p1, p2, penetrationCastRadius, rayDirVec, hypothenuse2.magnitude, GroundLayers,
                        QueryTriggerInteraction.Ignore))
                {
                    Grounded = true;
                }
                else
                {
                    // 일반 동작을 계속 수행
                    m_groundedNormal = hit.normal;
                    float angle = Vector3.Angle(hit.normal, transform.up);
                    RaycastHit hit2;

                    if (angle > _controller.slopeLimit)
                    {
                        Vector3 raycastOrigin = transform.position +
                                                transform.up * (positiveGroundedOffset + _controller.stepOffset) +
                                                m_currVelDir * GroundedRadius;
                        if (Physics.Raycast(raycastOrigin, -transform.up, out hit2,
                                twoTimesPositiveGroundedOffset + _controller.stepOffset * 2.0f, GroundLayers,
                                QueryTriggerInteraction.Ignore))
                        {
                            if (Vector3.Angle(hit2.normal, transform.up) > _controller.slopeLimit)
                            {
                                Grounded = false;
                                Vector3 rightVec = Vector3.Cross(hit.normal, transform.up).normalized;
                                targetSlopeVelocity = Vector3.Cross(hit.normal, rightVec).normalized;
                            }
                            else
                            {
                                if (m_checkAbyssColliderRadiusMultiplier > 0.999f)
                                {
                                    Grounded = true;
                                }
                                else
                                {
                                    float abyssRadius = m_checkAbyssColliderRadiusMultiplier * GroundedRadius;
                                    raycastOrigin = transform.position +
                                                    transform.up * (positiveGroundedOffset + abyssRadius);
                                    if (Physics.SphereCast(raycastOrigin, abyssRadius, -transform.up, out hit2,
                                            twoTimesPositiveGroundedOffset + _controller.stepOffset, GroundLayers,
                                            QueryTriggerInteraction.Ignore))
                                    {
                                        Grounded = true;
                                    }
                                    else
                                    {
                                        Grounded = false;
                                        Vector3 rightVec = Vector3.Cross(hit.normal, transform.up).normalized;
                                        targetSlopeVelocity = Vector3.Cross(hit.normal, rightVec).normalized;
                                    }
                                }
                            }
                        }
                        else
                        {
                            Grounded = false;
                            Vector3 rightVec = Vector3.Cross(hit.normal, transform.up).normalized;
                            targetSlopeVelocity = Vector3.Cross(hit.normal, rightVec).normalized;
                        }
                    }
                    else
                    {
                        if (m_checkAbyssColliderRadiusMultiplier > 0.999f)
                        {
                            Grounded = true;
                        }
                        else
                        {
                            float abyssRadius = m_checkAbyssColliderRadiusMultiplier * GroundedRadius;
                            Vector3 raycastOrigin = transform.position +
                                                    transform.up * (positiveGroundedOffset + abyssRadius);
                            if (Physics.SphereCast(raycastOrigin, abyssRadius, -transform.up, out hit2,
                                    twoTimesPositiveGroundedOffset + _controller.stepOffset, GroundLayers,
                                    QueryTriggerInteraction.Ignore))
                            {
                                Grounded = true;
                            }
                            else
                            {
                                Grounded = false;
                                Vector3 rightVec = Vector3.Cross(hit.normal, transform.up).normalized;
                                targetSlopeVelocity = Vector3.Cross(hit.normal, rightVec).normalized;
                            }
                        }
                    }
                }
            }
            else
            {
                Grounded = false;
            }

            if (targetSlopeVelocity.magnitude > 0.001f)
            {
                m_slopeVelocity += targetSlopeVelocity * Time.deltaTime * Mathf.Abs(Gravity);
            }
            else
            {
                m_slopeVelocity = Vector3.Lerp(m_slopeVelocity, Vector3.zero, Time.deltaTime * 20.0f);
            }

            // 애니메이터가 있는 경우 애니메이터에 상태 업데이트
            if (_hasAnimator)
            {
                _animator.SetBool(_animIDGrounded, Grounded);
            }
        }

        private void GroundedCheck()
        {
            // 오프셋을 적용한 구 위치 설정
            Vector3 spherePosition = new Vector3(transform.position.x, transform.position.y - GroundedOffset,
                transform.position.z);
            Grounded = Physics.CheckSphere(spherePosition, GroundedRadius, GroundLayers,
                QueryTriggerInteraction.Ignore);

            // 애니메이터가 있는 경우 애니메이터에 상태 업데이트
            if (_hasAnimator)
            {
                _animator.SetBool(_animIDGrounded, Grounded);
            }
        }

        private void CameraRotation()
        {
            // 입력이 있고 카메라 위치가 고정되어 있지 않은 경우
            if (_input.look.sqrMagnitude >= _threshold && !LockCameraPosition)
            {
                // 마우스 입력은 Time.deltaTime을 곱하지 않습니다.
                float deltaTimeMultiplier = IsCurrentDeviceMouse ? 1.0f : Time.deltaTime;

                _cinemachineTargetYaw += _input.look.x * deltaTimeMultiplier;
                _cinemachineTargetPitch += _input.look.y * deltaTimeMultiplier;
            }

            // 회전을 클램프하여 값이 제한되도록 함 (360도를 넘지 않음)
            _cinemachineTargetYaw = ClampAngle(_cinemachineTargetYaw, float.MinValue, float.MaxValue);
            _cinemachineTargetPitch = ClampAngle(_cinemachineTargetPitch, BottomClamp, TopClamp);

            // Cinemachine이 이 대상을 따라감
            CinemachineCameraTarget.transform.rotation = Quaternion.Euler(_cinemachineTargetPitch + CameraAngleOverride,
                _cinemachineTargetYaw, 0.0f);
        }

        private void Move()
        {
            // 이동 속도, 달리기 속도 및 달리기 입력에 따라 목표 속도 설정
            float targetSpeed = _input.sprint ? SprintSpeed : MoveSpeed;

            // 간단한 가속 및 감속 (나중에 쉽게 교체 가능)

            // 참고: Vector2의 == 연산자는 근사값 비교를 사용하므로 부동소수점 오차에 민감하지 않으며 magnitude보다 저렴합니다
            // 입력이 없으면 목표 속도를 0으로 설정
            if (_input.move == Vector2.zero) targetSpeed = 0.0f;

            // 플레이어의 현재 수평 속도 참조
            // + iStep 요구사항
            float currentHorizontalSpeed = new Vector3(_controller.velocity.x - m_prevAppliedSlopeVelocity.x, 0.0f,
                _controller.velocity.z - m_prevAppliedSlopeVelocity.z).magnitude;

            float speedOffset = 0.1f;
            float inputMagnitude = _input.analogMovement ? _input.move.magnitude : 1f;

            // 목표 속도로 가속 또는 감속
            if (currentHorizontalSpeed < targetSpeed - speedOffset ||
                currentHorizontalSpeed > targetSpeed + speedOffset)
            {
                // 선형이 아닌 곡선형 결과를 만들어 더 자연스러운 속도 변화 제공
                // Lerp의 t는 클램프되므로 속도를 별도로 클램프할 필요 없음
                _speed = Mathf.Lerp(currentHorizontalSpeed, targetSpeed * inputMagnitude,
                    Time.deltaTime * SpeedChangeRate);

                // 속도를 소수점 3자리로 반올림
                _speed = Mathf.Round(_speed * 1000f) / 1000f;
            }
            else
            {
                _speed = targetSpeed;
            }

            _animationBlend = Mathf.Lerp(_animationBlend, targetSpeed, Time.deltaTime * SpeedChangeRate);
            if (_animationBlend < 0.01f) _animationBlend = 0f;

            // 입력 방향 정규화
            Vector3 inputDirection = new Vector3(_input.move.x, 0.0f, _input.move.y).normalized;

            // 참고: Vector2의 != 연산자도 근사값 비교를 사용함
            // 이동 입력이 있으면 플레이어를 회전시킴
            if (_input.move != Vector2.zero)
            {
                _targetRotation = Mathf.Atan2(inputDirection.x, inputDirection.z) * Mathf.Rad2Deg +
                                  _mainCamera.transform.eulerAngles.y;
                float rotation = Mathf.SmoothDampAngle(transform.eulerAngles.y, _targetRotation, ref _rotationVelocity,
                    RotationSmoothTime);

                // 카메라 위치를 기준으로 입력 방향을 바라보도록 회전
                transform.rotation = Quaternion.Euler(0.0f, rotation, 0.0f);
            }


            Vector3 targetDirection = Quaternion.Euler(0.0f, _targetRotation, 0.0f) * Vector3.forward;

            if (m_useIStepGroundedImprovements)
            {
                Vector3 slopeVelocityToUse = calculateSlopeVelocityToUseByiStep(targetDirection, _speed); // iStep 개선사항

                // 플레이어 이동
                _controller.Move(targetDirection.normalized * _speed * Time.deltaTime +
                                 new Vector3(0.0f, _verticalVelocity, 0.0f) * Time.deltaTime +
                                 slopeVelocityToUse * Time.deltaTime /* iStep 개선사항 */);

                m_prevAppliedSlopeVelocity = slopeVelocityToUse;
            }
            else
            {
                // 플레이어 이동
                _controller.Move(targetDirection.normalized * (_speed * Time.deltaTime) +
                                 new Vector3(0.0f, _verticalVelocity, 0.0f) * Time.deltaTime);
            }

            // 애니메이터가 있는 경우 애니메이터에 매개변수 업데이트
            if (_hasAnimator)
            {
                _animator.SetFloat(_animIDSpeed, _animationBlend);
                _animator.SetFloat(_animIDMotionSpeed, inputMagnitude);
            }
        }

        private Vector3 calculateSlopeVelocityToUseByiStep(Vector3 targetDirection, float movingVel)
        {
            // Function Copyright © Kreshnik Halili

            if (m_slopeVelocity.magnitude > 0.001f && movingVel > 0.001f)
            {
                Vector3 verticalVel = Vector3.Project(m_slopeVelocity, transform.up);
                Vector3 slopeVelPlanar = m_slopeVelocity - verticalVel;

                Vector3 subtract = Vector3.Project(targetDirection.normalized * movingVel, slopeVelPlanar.normalized);
                Vector3 nextSlopeVelPlanar = slopeVelPlanar - subtract;

                float dot = Vector3.Dot(nextSlopeVelPlanar.normalized, slopeVelPlanar.normalized);

                if (dot > 0)
                    return nextSlopeVelPlanar + verticalVel; // 플레이어가 slopeVelocity와 반대 방향으로 달리거나 같은 방향이지만 평면 성분보다 느릴 때
                else return Vector3.zero; //verticalVel; // 이 경우 플레이어가 slopeVelocity와 같은 방향으로 더 빠르게 달리므로 평면 성분을 추가하지 않음
            }
            //else if (Grounded) return Vector3.zero; // 이 경우 충돌이 더 나빠지므로 사용하지 않음

            return m_slopeVelocity;
        }

        private void JumpAndGravity()
        {
            if (Grounded)
            {
                // 낙하 타임아웃 타이머 리셋
                _fallTimeoutDelta = FallTimeout;

                // 애니메이터가 있는 경우 애니메이터에 상태 업데이트
                if (_hasAnimator)
                {
                    _animator.SetBool(_animIDJump, false);
                    _animator.SetBool(_animIDFreeFall, false);
                }

                // 지면에 있을 때 속도가 무한히 감소하지 않도록 함
                if (_verticalVelocity < 0.0f)
                {
                    if (m_useIStepGroundedImprovements)
                    {
                        _verticalVelocity = Mathf.Lerp(Gravity, -2.0f, Vector3.Dot(Vector3.up, m_groundedNormal));
                    }
                    else
                    {
                        _verticalVelocity = -2f;
                    }
                }

                // 점프
                if (_input.jump && _jumpTimeoutDelta <= 0.0f)
                {
                    // 점프 타임아웃 타이머 리셋
                    _jumpTimeoutDelta = JumpTimeout;

                    // 원하는 높이에 도달하기 위한 초기 수직 속도 계산
                    _verticalVelocity = Mathf.Sqrt(JumpHeight * -2f * Gravity);

                    // 애니메이터가 있는 경우 애니메이터에 상태 업데이트
                    if (_hasAnimator)
                    {
                        _animator.SetBool(_animIDJump, true);
                    }
                }

                // 점프 타임아웃 처리
                if (_jumpTimeoutDelta >= 0.0f)
                {
                    _jumpTimeoutDelta -= Time.deltaTime;
                }
            }
            else
            {
                // 낙하 타임아웃 처리
                if (_fallTimeoutDelta >= 0.0f)
                {
                    _fallTimeoutDelta -= Time.deltaTime;
                }
                else
                {
                    // 애니메이터가 있는 경우 자유 낙하 상태로 전환
                    if (_hasAnimator)
                    {
                        _animator.SetBool(_animIDFreeFall, true);
                    }
                }

                // 지면에 없으면 점프 입력을 무시
                _input.jump = false;
            }

            // 터미널 속도 아래라면 중력 적용 (deltaTime을 두 번 곱해 시간에 따라 선형적으로 가속)
            if (_verticalVelocity > _terminalVelocity)
            {
                _verticalVelocity += Gravity * Time.deltaTime;
            }

            if (_verticalVelocity > 0)
            {
                // 천장과 충돌하는지 검사
                Vector3 scaledCenterOffsetVec = _controller.center;
                scaledCenterOffsetVec.x *= transform.localScale.x;
                scaledCenterOffsetVec.y *= transform.localScale.y;
                scaledCenterOffsetVec.z *= transform.localScale.z;
                float scaledHeight = _controller.height * transform.localScale.y;
                float scaledRadius = _controller.radius * Mathf.Max(transform.localScale.x, transform.localScale.z);
                Vector3 origin = transform.position + scaledCenterOffsetVec +
                                 transform.up * (scaledHeight * 0.5f - scaledRadius);

                float positiveGroundedOffset = Mathf.Abs(GroundedOffset);
                float penetrationCastRadius = scaledRadius * 0.99f;
                float rayDist = scaledRadius - penetrationCastRadius + positiveGroundedOffset;

                RaycastHit hit;
                if (Physics.SphereCast(origin, penetrationCastRadius, transform.up, out hit, rayDist, GroundLayers,
                        QueryTriggerInteraction.Ignore))
                {
                    _jumpTimeoutDelta = 0.0f;
                    _verticalVelocity = Mathf.Lerp(_verticalVelocity, 0.0f, Time.deltaTime * 10.0f);
                }
            }
        }

        private void Attack()
        {
            if (_input.attack)
            {
                if (greatSword)
                {
                    if (attackRight)
                    {
                        _animator.SetTrigger(_animIDAttackRight);
                        StartCoroutine(AnimatorBoolFalse(_animIDAttackRight));
                        attackRight = false;
                    }
                    else
                    {
                        _animator.SetTrigger(_animIDAttackLeft);
                        StartCoroutine(AnimatorBoolFalse(_animIDAttackLeft));
                        attackRight = true;
                    }
                    Collider[] nearMonster = Physics.OverlapSphere(transform.position, 5, LayerMask.GetMask("Monsters"));
                    foreach (var monster in nearMonster)
                    {
                        float dis = Vector3.Distance(transform.position, monster.transform.position);
                        if (dis <= 3f)
                        {
                            monster.GetComponent<monster>().TakeDamage(40);
                        }
                    }
                }
                _input.attack = false;
            }

        }

        IEnumerator AnimatorBoolFalse(int id)
        {
            yield return new WaitForSeconds(0.2f);
            _animator.SetBool(id, false);
        }

        private void QuickSlotOff()
        {
            if (UIManager.Instance.quickSlotUI.quickSlot1.isSet)
            {
                UIManager.Instance.quickSlotUI.quickSlot1.ItemSet();
            }else if (UIManager.Instance.quickSlotUI.quickSlot2.isSet)
            {
                UIManager.Instance.quickSlotUI.quickSlot2.ItemSet();
            }else if (UIManager.Instance.quickSlotUI.quickSlot3.isSet)
            {
                UIManager.Instance.quickSlotUI.quickSlot3.ItemSet();
            }else if (UIManager.Instance.quickSlotUI.quickSlot4.isSet)
            {
                UIManager.Instance.quickSlotUI.quickSlot4.ItemSet();
            }
        }

        private void QuickSlot()
        {
            if(_playerQuickSlot.equiping) return;
            // 큇슬롯에 있는 아이템을 점보를 가져와서 할 예정
            if (_input.quickSlot1)
            {
                if (_playerQuickSlot.currentSlot != 1)
                {
                    QuickSlotOff();
                    _playerQuickSlot.SelectSlot(1);
                    UIManager.Instance.quickSlotUI.SetQuickSlotUI(1);
                }
                else
                {
                    _playerQuickSlot.SelectSlot(-1);
                    UIManager.Instance.quickSlotUI.SetQuickSlotUI(1);
                }
                _input.quickSlot1 = false;
            }
            else if (_input.quickSlot2)
            {
                if (_playerQuickSlot.currentSlot != 2)
                {
                    QuickSlotOff();
                    _playerQuickSlot.SelectSlot(2);
                    UIManager.Instance.quickSlotUI.SetQuickSlotUI(2);
                }
                else
                {
                    _playerQuickSlot.SelectSlot(-1);
                    UIManager.Instance.quickSlotUI.SetQuickSlotUI(2);
                }
                _input.quickSlot2 = false;
            }
            else if (_input.quickSlot3)
            {
                if (_playerQuickSlot.currentSlot != 3)
                {
                    QuickSlotOff();
                    _playerQuickSlot.SelectSlot(3);
                    UIManager.Instance.quickSlotUI.SetQuickSlotUI(3);
                }
                else
                {
                    _playerQuickSlot.SelectSlot(-1);
                    UIManager.Instance.quickSlotUI.SetQuickSlotUI(3);
                }
                _input.quickSlot3 = false;
            }
            else if (_input.quickSlot4)
            {
                if (_playerQuickSlot.currentSlot != 4)
                {
                    QuickSlotOff();
                    _playerQuickSlot.SelectSlot(4);
                    UIManager.Instance.quickSlotUI.SetQuickSlotUI(4);
                }
                else
                {
                    _playerQuickSlot.SelectSlot(-1);
                    UIManager.Instance.quickSlotUI.SetQuickSlotUI(4);
                }
                _input.quickSlot4 = false;
            }
            _animator.SetBool("None", none);
            _animator.SetBool("GreatSword", greatSword);
        }

        List<Transform> nearItem = new List<Transform>();
        float nearItemDis = Mathf.Infinity;
        GameObject nearItemObj;

        bool isItem = false;
        
        public void Interaction()
        {
            if (_input.drop)
            {
                _input.drop = false;
                if (_playerQuickSlot.currentSlot != -1)
                {
                    Debug.Log("dd");
                    _playerQuickSlot.DropItem(_playerQuickSlot.currentSlot);
                }
            }
            isItem = false;
            if (nearItem.Count > 0)
            {
                nearItem.Clear();
            }

            if (UIManager.Instance.LookInteractObject)
            {
                return;
            }
            nearItemDis = Mathf.Infinity;
            foreach (var col in Physics.OverlapSphere(transform.position, sensineRange))
            {
                if (col.CompareTag("DropItem"))
                {
                    isItem = true;
                    nearItem.Add(col.transform);
                }
            }

            if (nearItem.Count > 0)
            {
                foreach (var col in nearItem)
                {
                    float dis = Vector3.Distance(transform.position, col.transform.position);
                    if (dis < nearItemDis)
                    {
                        nearItemDis = dis;
                        nearItemObj = col.gameObject;
                    }
                }
                ItemInfo itemInfo = nearItemObj.GetComponent<DropItem>().itemInfo;
                UIManager.Instance.interactionUI.Set(itemInfo.itemName + " 획득 (F)");
            }

            if (isItem)
            {
                if (_input.interaction)
                {   
                    _input.interaction = false;
                    if(_playerQuickSlot.currentSlot != -1) return;
                    if (_playerQuickSlot.slot1 == null)
                    {
                        _playerQuickSlot.SetQuickSlot(nearItemObj.GetComponent<DropItem>().itemInfo);
                    }else if (_playerQuickSlot.slot2 == null)
                    {
                        _playerQuickSlot.SetQuickSlot(nearItemObj.GetComponent<DropItem>().itemInfo);
                    }else if (_playerQuickSlot.slot3 == null)
                    {
                        _playerQuickSlot.SetQuickSlot(nearItemObj.GetComponent<DropItem>().itemInfo);
                    }else if (_playerQuickSlot.slot4 == null)
                    {
                        _playerQuickSlot.SetQuickSlot(nearItemObj.GetComponent<DropItem>().itemInfo);
                    }
                    else
                    {
                        UIManager.Instance.errorLogUI.CreateErrorLog("퀵슬롯이 모두 가득 찼습니다.");
                        return;
                    }
                    if (!none) return;
                    _animator.SetTrigger(_animIDPickUp);
                    Destroy(nearItemObj);
                    
                }
            }

            if (!isItem)
            {
                UIManager.Instance.interactionUI.Hide();
            }
        }

        private static float ClampAngle(float lfAngle, float lfMin, float lfMax)
        {
            if (lfAngle < -360f) lfAngle += 360f;
            if (lfAngle > 360f) lfAngle -= 360f;
            return Mathf.Clamp(lfAngle, lfMin, lfMax);
        }

        private void OnDrawGizmosSelected()
        {
            if (m_useIStepGroundedImprovements) return;

            Color transparentGreen = new Color(0.0f, 1.0f, 0.0f, 0.35f);
            Color transparentRed = new Color(1.0f, 0.0f, 0.0f, 0.35f);

            if (Grounded) Gizmos.color = transparentGreen;
            else Gizmos.color = transparentRed;

            // 선택 시 지면 충돌체 위치와 반지름에 맞춰 gizmo를 그림
            Gizmos.DrawSphere(
                new Vector3(transform.position.x, transform.position.y - GroundedOffset, transform.position.z),
                GroundedRadius);
        }

        private void OnFootstep(AnimationEvent animationEvent)
        {
            if (animationEvent.animatorClipInfo.weight > 0.5f)
            {
                if (FootstepAudioClips.Length > 0)
                {
                    var index = Random.Range(0, FootstepAudioClips.Length);
                    AudioSource.PlayClipAtPoint(FootstepAudioClips[index], transform.TransformPoint(_controller.center), FootstepAudioVolume);
                }
            }
        }

        private void OnLand(AnimationEvent animationEvent)
        {
            if (animationEvent.animatorClipInfo.weight > 0.5f)
            {
                if (LandingAudioClip != null) // iStep 수정
                {
                    AudioSource.PlayClipAtPoint(LandingAudioClip, transform.TransformPoint(_controller.center), FootstepAudioVolume);
                }
            }
        }
    }
}