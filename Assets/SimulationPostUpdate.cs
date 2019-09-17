using System;
using System.Collections.Generic;
using UnityEngine;

namespace zs
{
    public class SimulationPostUpdate : MonoBehaviour
    {
        #region Serializable Fields

        [SerializeField]
        private Transform _rigidbodySpritePrefab = null;

        #endregion Serializable Fields

        #region Private Vars

        private Transform _rigidbodySpriteParent = null;

        private PhysicsSyncType _syncType;
        private DisplayStyle _displayStyle;

        private readonly List<Tuple<Player, Transform>> _playerToRigidbodySprite = new List<Tuple<Player, Transform>>(); 

        #endregion Private Vars

        #region Public Vars

        #endregion Public Vars

        #region Public Methods

        public void PerformRestart(
            PhysicsSyncType syncType,
            DisplayStyle displayStyle)
        {
            _syncType = syncType;
            _displayStyle = displayStyle;

            foreach (var item in _playerToRigidbodySprite)
            {
                Destroy(item.Item2.gameObject);
            }

            _playerToRigidbodySprite.Clear();
        }

        public void RegisterPlayer(Player player)
        {
            if (player.Rigidbody != null &&
                (_displayStyle == DisplayStyle.RigidbodyAndSprite || _displayStyle == DisplayStyle.Rigidbody))
            {
                _playerToRigidbodySprite.Add(Tuple.Create(player, Instantiate(_rigidbodySpritePrefab, player.Rigidbody.position, Quaternion.identity, _rigidbodySpriteParent)));
            }
        }

        #endregion Public Methods

        #region MonoBehaviour

        void Awake()
        {
            _rigidbodySpriteParent = new GameObject().transform;
            _rigidbodySpriteParent.gameObject.name = "Rigidbody Sprite Container";
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

            foreach (var item in _playerToRigidbodySprite)
            {
                item.Item2.transform.position = item.Item1.Rigidbody.position;
            }
        }
        
        #endregion MonoBehaviour

        #region Private Methods

        #endregion Private Methods
    }
}
