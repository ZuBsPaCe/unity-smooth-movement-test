using UnityEngine;

namespace zs
{
    [System.Serializable]
    public class PidController
    {
        #region Serializable Fields

        /// <summary>
        /// Linear correction
        /// </summary>
        [SerializeField]
        private float _kp = 0.2f;
	
        /// <summary>
        /// If error increases, increases correction.
        /// If error decreases, reduces correction.
        /// -> Smoothing
        /// </summary>
        [SerializeField]
        private float _ki = 0.05f;
	
        /// <summary>
        /// If 
        /// </summary>
        [SerializeField]
        private float _kd = 1f;

        #endregion Serializable Fields

        #region Private Vars

        private float _lastError;
        private float _integral;

        #endregion Private Vars

        #region Public Vars

        #endregion Public Vars

        #region Public Methods

        public void Reset()
        {
            _lastError = 0;
            _integral = 0;
        }

        public float Update(float error, float deltaTime)
        {
            float derivative = (error - _lastError) / deltaTime;
            _integral += error * deltaTime;
            _lastError = error;
		
            return _kp * error + _ki * _integral + _kd * derivative;
        }

        #endregion Public Methods

        #region Private Methods
        #endregion Private Methods
    }
}
