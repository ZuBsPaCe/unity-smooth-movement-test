using System;
using UnityEngine;
using UnityEngine.UI;
using zs.Assets;

namespace zs
{
    public class LaneCanvas : MonoBehaviour
    {
        #region Serializable Fields

        [SerializeField]
        private Text _laneLabel = null;

        [SerializeField]
        private Text _methodText = null;

        [SerializeField]
        private Button _methodButton = null;

        [SerializeField]
        private Text _bodyText = null;

        [SerializeField]
        private Text _movementText = null;

        [SerializeField]
        private Text _interpolationText = null;

        [SerializeField]
        private Text _velocityLabel = null;

        [SerializeField]
        private Text _velocityText = null;

        [SerializeField]
        private RectTransform _bodyPanel = null;

        [SerializeField]
        private RectTransform _interpolationPanel = null;

        #endregion Serializable Fields

        #region Private Vars

        private int _laneIndex = 0;
        private Player _player = null;

        private PhysicsSyncType _physicsSyncType;

        private MethodType _methodType = MethodType.Update;
        private BodyType _bodyType = BodyType.Kinematic;
        private MovementType _movementType = MovementType.Transform_Position;
        private InterpolationType _interpolationType = InterpolationType.None;

        private float _lastVelocityUpdate = 0;

        #endregion Private Vars


        public MethodType MethodType
        {
            get { return _methodType; }
            set
            {
                _methodType = value;
                UpdateTexts();
            }
        }

        public BodyType BodyType
        {
            get { return _bodyType; }
            set
            {
                _bodyType = value; 
                UpdateTexts();
            }
        }

        public MovementType MovementType
        {
            get { return _movementType; }
            set
            {
                _movementType = value;
                UpdateTexts();
            }
        }

        public InterpolationType InterpolationType
        {
            get { return _interpolationType; }
            set
            {
                _interpolationType = value;
                UpdateTexts();
            }
        }

        #region Public Vars
        #endregion Public Vars

        #region Public Methods

        public void Init(
            int laneIndex,
            Player player)
        {
            _laneIndex = laneIndex;
            _player = player;
        }

        public void Restart(
            PhysicsSyncType physicsSyncType)
        {
            _physicsSyncType = physicsSyncType;

            if (physicsSyncType == PhysicsSyncType.Default)
            {
                _methodText.text = _methodType.ToString();
                _methodButton.interactable = true;
            }
            else
            {
                _methodText.text = MethodType.Update.ToString();
                _methodButton.interactable = false;
            }
        }

        public void OnMethodButton_Clicked()
        {
            SetNextEnumValue(ref _methodType);
            UpdateTexts();

            Simulation.Instance.Restart();
        }

        public void OnBodyTypeButton_Clicked()
        {
            SetNextEnumValue(ref _bodyType);
            UpdateTexts();

            Simulation.Instance.Restart();
        }

        public void OnMovementButton_Clicked()
        {
            SetNextEnumValue(ref _movementType);
            UpdateTexts();

            Simulation.Instance.Restart();
        }

        public void OnInterpolationButton_Clicked()
        {
            SetNextEnumValue(ref _interpolationType);
            UpdateTexts();

            Simulation.Instance.Restart();
        }

        #endregion Public Methods

        #region MonoBehaviour
	
        void Start()
        {
            UpdateTexts();
        }

        void Update()
        {
            if (Time.time - _lastVelocityUpdate > 0.2)
            {
                _lastVelocityUpdate = Time.time;

                float velocity = _player.Rigidbody?.velocity.x ?? 0;

                if (Mathf.Abs(velocity) < 0.01f)
                {
                    if (_velocityText.enabled)
                    {
                        _velocityText.enabled = false;
                        _velocityLabel.enabled = false;

                        _velocityText.text = string.Empty;
                    }
                }
                else
                {
                    if (!_velocityText.enabled)
                    {
                        _velocityText.enabled = true;
                        _velocityLabel.enabled = true;
                    }

                    _velocityText.text = velocity.ToString("0.0");
                }
            }
        }

        #endregion MonoBehaviour

        #region Private Methods

        private static void SetNextEnumValue<T>(ref T currentValue)
        {
            int newIndex = Convert.ToInt32(currentValue) + 1;
            Array values = Enum.GetValues(typeof(T));
            if (newIndex >= values.Length)
            {
                newIndex = 0;
            }

            currentValue = (T) values.GetValue(newIndex);
        }

        private void UpdateTexts()
        {
            _laneLabel.text = "Lane " + (_laneIndex + 1);

            _interpolationPanel.gameObject.SetActive(_methodType == MethodType.FixedUpdate && _physicsSyncType == PhysicsSyncType.Default);
            _bodyPanel.gameObject.SetActive(_movementType != MovementType.CharacterController_Move);

            _methodText.text = _methodType.ToString();
            _bodyText.text = _bodyType.ToString();
            _movementText.text = _movementType.ToString().Replace("_", ".");
            _interpolationText.text = _interpolationType.ToString().Replace("_", " ");
        }

        #endregion Private Methods
    }
}
