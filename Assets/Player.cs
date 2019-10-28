using UnityEngine;

namespace zs
{
    public class Player : MonoBehaviour
    {
        #region Serializable Fields

        [SerializeField]
        private SpriteRenderer _spriteRenderer = null;

        [SerializeField]
        private PidController _addForcePidController = null;

        #endregion Serializable Fields

        #region Private Vars

        private int _playerIndex;

        private MethodType _methodType;
        private BodyType _bodyType;
        private MovementType _movementType;
        private InterpolationType _interpolationType;

        private float _speed = 5f;

        private Transform _spriteTransform;
        private Rigidbody2D _rigidbody;
        private HingeJoint2D _hingeJoint;
        private CharacterController _characterController;
        private float _velocity;

        private float _minX;
        private float _maxX;

        private Vector2 _startInterpolationPos = Vector2.zero;
        private Vector2 _endInterpolationPos = Vector2.zero;

        #endregion Private Vars

        #region Public Vars

        public SpriteRenderer SpriteRenderer
        {
            get { return _spriteRenderer; }
        }

        public Rigidbody2D Rigidbody
        {
            get { return _rigidbody; }
        }

        public HingeJoint2D HingeJoint
        {
            get { return _hingeJoint; }
        }

        #endregion Public Vars

        #region Public Methods

        public void Restart(
            int playerIndex,
            Vector3 position,
            float minX,
            float maxX,
            MethodType methodType,
            BodyType bodyType,
            MovementType movementType,
            InterpolationType interpolationType,
            float speed,
            DisplayStyle displayStyle)
        {
            _playerIndex = playerIndex;
            gameObject.name = "Player " + (_playerIndex + 1);
            _spriteRenderer.gameObject.name = "Player Sprite " + (_playerIndex + 1);

            _methodType = methodType;
            _bodyType = bodyType;
            _movementType = movementType;

            _speed = speed;
            _velocity = speed;

            transform.position = position;
            transform.rotation = Quaternion.identity;

            _minX = minX;
            _maxX = maxX;

            _hingeJoint.enabled = false;


            _spriteTransform.parent = transform;
            _spriteTransform.transform.localPosition = Vector3.zero;

            _startInterpolationPos = position;
            _endInterpolationPos = position;


            if (_movementType == MovementType.CharacterController_Move)
            {
                // Those components are not compatible with the CharacterController component.
                DestroyImmediate(GetComponent<HingeJoint2D>());
                DestroyImmediate(GetComponent<Rigidbody2D>());
                DestroyImmediate(GetComponent<BoxCollider2D>());

                _rigidbody = null;
                _hingeJoint = null;
                _characterController = gameObject.AddComponent<CharacterController>();
                _characterController.height = 1;

                displayStyle = DisplayStyle.Sprite;

                if (_methodType == MethodType.FixedUpdate &&
                    (interpolationType == InterpolationType.Custom_Bad || interpolationType == InterpolationType.Custom_Good))
                {
                    _interpolationType = interpolationType;
                }
                else
                {
                    _interpolationType = InterpolationType.None;
                }
            }
            else
            {
                _rigidbody.velocity = Vector2.zero;
                _rigidbody.angularVelocity = 0;

                if (_bodyType == BodyType.Dynamic)
                {
                    _rigidbody.bodyType = RigidbodyType2D.Dynamic;
                }
                else
                {
                    _rigidbody.bodyType = RigidbodyType2D.Kinematic;
                }

                if (_methodType == MethodType.FixedUpdate)
                {
                    _interpolationType = interpolationType;
                }
                else
                {
                    _interpolationType = InterpolationType.None;
                }
            }
            
            switch (_interpolationType)
            {
                case InterpolationType.Rigidbody_Interpolate:
                    if (_rigidbody != null)
                    {
                        _rigidbody.interpolation = RigidbodyInterpolation2D.Interpolate;
                    }
                    break;

                case InterpolationType.Rigidbody_Extrapolate:
                    if (_rigidbody != null)
                    {
                        _rigidbody.interpolation = RigidbodyInterpolation2D.Extrapolate;
                    }
                    break;

                case InterpolationType.Custom_Bad:
                    _spriteTransform.parent = null;
                    break;

                case InterpolationType.Custom_Good:
                    _spriteTransform.parent = null;
                    StartCoroutine(AfterFixedUpdate());
                    break;

                default:
                    if (_rigidbody != null)
                    {
                        _rigidbody.interpolation = RigidbodyInterpolation2D.None;
                    }
                    break;
            }

            switch (displayStyle)
            {
                case DisplayStyle.Sprite:
                case DisplayStyle.RigidbodyAndSprite:
                    _spriteRenderer.enabled = true;
                    _spriteRenderer.enabled = true;
                    break;

                case DisplayStyle.Rigidbody:
                    _spriteRenderer.enabled = false;
                    break;
            }

            StopAllCoroutines();
        }

        public void SetHighlight(bool highlightEnabled)
        {
            if (highlightEnabled)
            {
                _spriteRenderer.color = Color.yellow;
            }
            else
            {
                _spriteRenderer.color = Color.white;
            }
        }

        #endregion Public Methods

        #region MonoBehaviour
	
        void Awake()
        {
            _spriteTransform = _spriteRenderer.transform;
            _rigidbody = GetComponent<Rigidbody2D>();
            _hingeJoint = GetComponent<HingeJoint2D>();
            _velocity = _speed;
        }

        System.Collections.IEnumerator AfterFixedUpdate()
        {
            while (true)
            {
                yield return new WaitForFixedUpdate();
                _endInterpolationPos = _rigidbody.position;
            }
        }

        void OnDestroy()
        {
            Destroy(_spriteTransform.gameObject);

        }

        void Start()
        {
            _addForcePidController.Reset();
        }
	
        void Update()
        {
            if (_methodType == MethodType.Update)
            {
                PerformUpdate();
            }
            else
            {
                if (_interpolationType == InterpolationType.Custom_Bad)
                {
                    var factor = (Time.time - Time.fixedTime) / Time.fixedDeltaTime;
                    _spriteTransform.position  = Vector2.Lerp(_startInterpolationPos, _endInterpolationPos, factor);
                }
                else if (_interpolationType == InterpolationType.Custom_Good)
                {
                    float factor = (Time.time - Time.fixedTime) / Time.fixedDeltaTime;
                    _spriteTransform.position  = Vector2.Lerp(_startInterpolationPos, _endInterpolationPos, factor);
                }
            }
        }

        void FixedUpdate()
        {
            if (_methodType == MethodType.FixedUpdate)
            {
                PerformUpdate();
            }
        }

        #endregion MonoBehaviour

        #region Private Methods

        private void PerformUpdate()
        {
            switch (_movementType)
            {
                case MovementType.Transform_Position:
                {
                    _startInterpolationPos = transform.position;

                    Vector3 position = transform.position;
                    position.x += Time.deltaTime * _velocity;

                    HandleReversal(ref position);

                    transform.position = position;

                    _endInterpolationPos = transform.position;
                    break;
                }

                case MovementType.Rigidbody_Position:
                {
                    _startInterpolationPos = _rigidbody.position;

                    Vector3 position = _rigidbody.position;
                    position.x += Time.deltaTime * _velocity;

                    HandleReversal(ref position);

                    _rigidbody.position = position;

                    _endInterpolationPos = _rigidbody.position;
                    break;
                }

                case MovementType.Rigidbody_MovePosition:
                {
                    _startInterpolationPos = _rigidbody.position;

                    Vector3 position = _rigidbody.position;
                    position.x += Time.deltaTime * _velocity;

                    HandleReversal(ref position);

                    _rigidbody.MovePosition(position);

                    _endInterpolationPos = _rigidbody.position;
                    break;
                }

                case MovementType.Rigidbody_CustomMove:
                {
                    _startInterpolationPos = _rigidbody.position;

                    Vector3 position = _rigidbody.position;
                    position.x += Time.deltaTime * _velocity;

                    HandleReversal(ref position);

                    _rigidbody.CustomMove(position);

                    _endInterpolationPos = _rigidbody.position;
                    break;
                }

                case MovementType.Rigidbody_SetVelocity:
                {
                    _startInterpolationPos = _rigidbody.position;

                    Vector3 position = _rigidbody.position;
                    if (HandleReversal(ref position))
                    {
                        _rigidbody.position = position;
                    }

                    _rigidbody.velocity = new Vector2(_velocity, 0);

                    _endInterpolationPos = _startInterpolationPos + _rigidbody.velocity * Time.deltaTime;
                    break;
                }

                case MovementType.Rigidbody_AddVelocity:
                case MovementType.Rigidbody_AddForce:
                case MovementType.Rigidbody_AddImpulse:
                {
                    _startInterpolationPos = _rigidbody.position;

                    float change = _addForcePidController.Update(_velocity - _rigidbody.velocity.x, Time.deltaTime);

                    if (_movementType == MovementType.Rigidbody_AddVelocity)
                    {
                        _rigidbody.velocity += new Vector2(change * Time.deltaTime, 0);
                    }
                    else if (_movementType == MovementType.Rigidbody_AddForce)
                    {
                        _rigidbody.AddForce(new Vector2(change, 0), ForceMode2D.Force);
                    }
                    else if (_movementType == MovementType.Rigidbody_AddImpulse)
                    {
                        _rigidbody.AddForce(new Vector2(change * Time.deltaTime, 0), ForceMode2D.Impulse);
                    }

                    Vector3 position = _rigidbody.position;
                    if (HandleReversal(ref position))
                    {
                        _rigidbody.position = position;
                        _rigidbody.velocity = -_rigidbody.velocity;
                    }

                    _endInterpolationPos = _startInterpolationPos + _rigidbody.velocity * Time.deltaTime;
                    break;
                }

                case MovementType.CharacterController_Move:
                {
                    _startInterpolationPos = transform.position;

                    Vector3 position = transform.position;
                    position.x += Time.deltaTime * _velocity;

                    HandleReversal(ref position);

                    _characterController.Move(new Vector3(Time.deltaTime * _velocity, 0, 0));

                    _endInterpolationPos = transform.position;

                    break;
                }
            }
        }
    
        private bool HandleReversal(ref Vector3 position)
        {
            if (position.x > _maxX)
            {
                _velocity *= -1;
                position.x -= position.x - _maxX;

                return true;
            }
            
            if (position.x < _minX)
            {
                _velocity *= -1;
                position.x += -position.x + _minX;

                return true;
            }

            return false;
        }

        #endregion Private Methods
    }
}
