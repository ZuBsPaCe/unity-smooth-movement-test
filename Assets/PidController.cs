using UnityEngine;

namespace zs
{
    /// <summary>
    /// PID stands for proportional, integral, and derivative.
    /// </summary>
    [System.Serializable]
    public class PidController
    {
        #region Serializable Fields

        /// <summary>
        /// Linear correction. Multiplied with the error. Basic response.
        ///
        /// If Velocity is too low, speeds up.
        /// If Velocity is too high, slows down.
        /// </summary>
        [SerializeField]
        private float _kp = 0.2f;
	
        /// <summary>
        /// Integral correction. Multiplied with the integral/sum of the error.
        ///
        /// Takes into account the error at previous times.
        /// Helps with responses time. If velocity is too low and increases
        /// too slow, the integral correction will increase the velocity faster.
        /// </summary>
        [SerializeField]
        private float _ki = 0.05f;
	
        /// <summary>
        /// Derivative correction. Multiplied with the derivative of the error.
        ///
        /// Determines how fast the error is changing. Prevents overshoot.
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
