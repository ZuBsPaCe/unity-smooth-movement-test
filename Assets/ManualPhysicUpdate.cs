using System;
using System.Collections.Generic;
using UnityEngine;

namespace zs
{
    public class ManualPhysicUpdate : MonoBehaviour
    {
        #region Serializable Fields

        #endregion Serializable Fields

        #region Private Vars

        private PhysicsSyncType _syncType;

        #endregion Private Vars

        #region Public Vars

        #endregion Public Vars

        #region Public Methods

        public void PerformRestart(
            PhysicsSyncType syncType)
        {
            _syncType = syncType;

            if (_syncType == PhysicsSyncType.Post_Update)
            {
                Physics2D.autoSimulation = false;
            }
            else
            {
                Physics2D.autoSimulation = true;
            }
        }

        #endregion Public Methods

        #region MonoBehaviour

        void Awake()
        {
        }

        /// <summary>
        /// Get's called at the end of the Update cycle.
        /// Defined in project settings under "Script Execution Order".
        /// </summary>
        void Update()
        {
            if (_syncType == PhysicsSyncType.Post_Update)
            {
                Physics2D.Simulate(Time.deltaTime);
            }
        }
        
        #endregion MonoBehaviour

        #region Private Methods

        #endregion Private Methods
    }
}
