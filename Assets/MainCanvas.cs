using UnityEngine;
using zs;

public class MainCanvas : MonoBehaviour
{
    #region Serializable Fields

    #endregion Serializable Fields

    #region Private Vars

    #endregion Private Vars

    #region Public Vars

    #endregion Public Vars

    #region Public Methods

    public void Restart()
    {
        Simulation.Instance.Restart();
    }

    public void Reset()
    {
        Simulation.Instance.Reset();
    }

    public void ToggleLog()
    {
        Simulation.Instance.ToggleLog();
    }

    public void ToggleSharedSettings()
    {
        Simulation.Instance.ToggleSharedSettings();
    }

    public void ToggleFollow()
    {
        Simulation.Instance.ToggleFollow();
    }

    #endregion Public Methods

	#region MonoBehaviour
	
	void Awake()
	{
	}

	void Start()
	{
	}
	
	void Update()
	{
	}

	#endregion MonoBehaviour

	#region Private Methods
	#endregion Private Methods
}
