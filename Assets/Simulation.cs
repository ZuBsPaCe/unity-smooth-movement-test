using System;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Tilemaps;
using UnityEngine.UI;
using zs;

public class Simulation : MonoBehaviour
{
	#region Serializable Fields

    [SerializeField]
    private MainCamera _mainCamera = null;

    [SerializeField]
    private Transform _ballPrefab = null;

    [SerializeField]
    private Player _playerPrefab = null;

    [SerializeField]
    private LaneCanvas _laneCanvasPrefab = null;

    [SerializeField]
    private Rigidbody2D _attachmentPrefab = null;

    [SerializeField]
    private Text _speedText = null;

    [SerializeField]
    private Slider _speedSlider = null;

    [SerializeField]
    private Text _vsyncText = null;

    [SerializeField]
    private Slider _vsyncSlider = null;

    [SerializeField]
    private RectTransform _frameRatePanel = null;

    [SerializeField]
    private Text _frameRateText = null;

    [SerializeField]
    private Slider _frameRateSlider = null;

    [SerializeField]
    private Text _physicsRateText = null;

    [SerializeField]
    private Slider _physicsRateSlider = null;

    [SerializeField]
    private Text _ballText = null;

    [SerializeField]
    private Slider _ballSlider = null;

    [SerializeField]
    private Text _hingeText = null;

    [SerializeField]
    private Slider _hingeSlider = null;

    [SerializeField]
    private Text _fpsText = null;

    [SerializeField]
    private Text _varianceText = null;

    [SerializeField]
    private Slider _laneSlider = null;

    [SerializeField]
    private Text _laneText = null;

    [SerializeField]
    private Slider _displaySlider = null;

    [SerializeField]
    private Text _displayText = null;

    [SerializeField]
    private Text _settingsText = null;

    [SerializeField]
    private Text _followText = null;

    [SerializeField]
    private List<LaneCanvas> _laneCanvases = new List<LaneCanvas>();

    [SerializeField]
    private List<Player> _players = new List<Player>();

    [SerializeField]
    private Tilemap _colliderTilemap = null;

    [SerializeField]
    private Tilemap _backgroundTilemap = null;

    [SerializeField]
    private Tile _wallTile = null;

    [SerializeField]
    private Tile _backgroundTile = null;

    [SerializeField]
    private Text _logButtonText = null;

    #endregion Serializable Fields

    #region Private Vars

    private bool _sharedSettings = false;
    private int _followLane = -1;
    private int _lastFollowLane = 0;

    private bool _restart = true;
    private float _restartTime = 0;

    private Transform _ballParent = null;
    private List<Transform> _balls = new List<Transform>();

    private Transform _attachmentParent = null;
    private List<Rigidbody2D> _attachments = new List<Rigidbody2D>();


    private Stopwatch _fpsRefreshStopwatch = new Stopwatch();
    private int _frameCount = 0;
    private long _minFrameDelta;
    private long _maxFrameDelta;

    private Log _log = null;
    private Stopwatch _updateStopwatch = new Stopwatch();
    private Stopwatch _fixedUpdateStopwatch = new Stopwatch();

    private Stopwatch _pauseStopwatch = null;

    #endregion Private Vars

	#region Public Vars

    public static Simulation Instance { get; private set; }

    public float CameraMinY { get; private set; }

    public Rigidbody2D AttachmentPrefab
    {
        get { return _attachmentPrefab; }
    }

    #endregion Public Vars

	#region Public Methods

    public void Restart()
    {
        _restart = true;
        _restartTime = Time.unscaledTime;

        UpdateTextFields();
    }

    public void Reset()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void ToggleLog()
    {
        if (_log == null)
        {
#if UNITY_STANDALONE && !UNITY_EDITOR
            _log = new FileLog();
#else
            _log = new DebugLog();
#endif
            _log.Write($"Started! {DateTime.Now:yyyy-MM-dd HH:mm:ss}");

            _logButtonText.text = "Stop!";
        }
        else
        {
            _log.Write($"Stopped! {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
            _log.Dispose();
            _log = null;

            _logButtonText.text = "Start!";
        }
    }

    public void ToggleSharedSettings()
    {
        _sharedSettings = !_sharedSettings;
        Restart();
    }

    public void ToggleFollow()
    {
        if (_followLane < 0)
        {
            _followLane = _lastFollowLane;
            if (_followLane > _players.Count - 1)
            {
                _followLane = _players.Count - 1;
            }
            _mainCamera.Follow(_players[_followLane]);
            _followText.text = "Unfollow!";
        }
        else
        {
            _lastFollowLane = _followLane;
            _followLane = -1;
            _mainCamera.Unfollow();
            _followText.text = "Follow!";
        }
    }

    public void LanePlusClicked()
    {
        if (_laneSlider.value < _laneSlider.maxValue)
        {
            _laneSlider.value = Mathf.Min(_laneSlider.value + 1, _laneSlider.maxValue);
            Restart();
        }
    }

    public void LaneMinusClicked()
    {
        if (_laneSlider.value > _laneSlider.minValue)
        {
            _laneSlider.value = Mathf.Max(_laneSlider.value - 1, _laneSlider.minValue);
            Restart();
        }
    }

	#endregion Public Methods

	#region MonoBehaviour
	
	void Awake()
    {
        Instance = this;

        _ballParent = new GameObject().transform;
        _ballParent.gameObject.name = "Ball Container";

        _attachmentParent = new GameObject().transform;
        _attachmentParent.gameObject.name = "Attachment Container";
    }


    void OnDestroy()
    {
        _log?.Dispose();
        _log = null;
    }

	void Start()
	{
        UpdateTextFields();

        _fpsText.text = string.Empty;
        _fpsRefreshStopwatch.Start();

        _updateStopwatch.Start();
        _fixedUpdateStopwatch.Start();
    }
	
	void Update()
    {
        _frameCount += 1;

        long realDeltaMsecs = _updateStopwatch.ElapsedMilliseconds;
        _updateStopwatch.Restart();

        _log?.Write($"     Update Cycle  #  Real Delta {realDeltaMsecs.ToString("0").PadLeft(4)} ms  #  Unity Delta {(Time.deltaTime * 1000).ToString("0").PadLeft(4)} ms  #  Unity Time {Time.time.ToString("0.000").PadLeft(8)} s");

        _minFrameDelta = realDeltaMsecs < _minFrameDelta ? realDeltaMsecs : _minFrameDelta;
        _maxFrameDelta = realDeltaMsecs > _maxFrameDelta ? realDeltaMsecs : _maxFrameDelta;

        if (_fpsRefreshStopwatch.ElapsedMilliseconds >= 500)
        {
            long elapsedMsecs = _fpsRefreshStopwatch.ElapsedMilliseconds;
            _fpsRefreshStopwatch.Restart();

            _fpsText.text = (1000f / (elapsedMsecs / (float) _frameCount)).ToString("0");
            _varianceText.text = "±" + (_maxFrameDelta - _minFrameDelta) / 2 + "ms";

            _frameCount = 0;
            _minFrameDelta = realDeltaMsecs;
            _maxFrameDelta = realDeltaMsecs;
        }

        if (_followLane >= 0)
        {
            if (Input.GetKeyDown(KeyCode.UpArrow) ||
                Input.GetKeyDown(KeyCode.W) ||
                Input.mouseScrollDelta.y > 0)
            {
                if (_followLane - 1 >= 0)
                {
                    _followLane -= 1;
                    _mainCamera.Follow(_players[_followLane]);
                }
            }
            else if (Input.GetKeyDown(KeyCode.DownArrow) ||
                     Input.GetKeyDown(KeyCode.S) ||
                    Input.mouseScrollDelta.y < 0)
            {
                if (_followLane + 1 < _players.Count)
                {
                    _followLane += 1;
                    _mainCamera.Follow(_players[_followLane]);
                }
            }
            else if (Input.GetKeyDown(KeyCode.F))
            {
                ToggleFollow();
            }
        }
        else if (Input.GetKeyDown(KeyCode.F))
        {
            ToggleFollow();
        }

        PerformPauseIfRequested(); 
	}

    void FixedUpdate()
    {
        long realDeltaMsecs = _fixedUpdateStopwatch.ElapsedMilliseconds;
        _fixedUpdateStopwatch.Restart();

        _log?.Write($"FixedUpdate Cycle  #  Real Delta {realDeltaMsecs.ToString("0").PadLeft(4)} ms  #  Unity Delta {(Time.deltaTime * 1000).ToString("0").PadLeft(4)} ms  #  Unity Time {Time.time.ToString("0.000").PadLeft(8)} s");

        if (_restart &&
            Time.unscaledTime - _restartTime > 0)
        {
            _restart = false;

            PerformRestart();

            _log?.RestartStopwatch();
            _log?.Write("Restart");
        }

        PerformPauseIfRequested(); 
    }

	#endregion MonoBehaviour

	#region Private Methods
    
    private void PerformRestart()
    {
        for (int i = 0; i < _balls.Count; i++)
        {
            Destroy(_balls[i].gameObject);
        }

        _balls.Clear();

        for (int i = 0; i < _attachments.Count; i++)
        {
            Destroy(_attachments[i].gameObject);
        }

        _attachments.Clear();

        if (_vsyncSlider.value != 0)
        {
            QualitySettings.vSyncCount = (int) _vsyncSlider.value;
            Application.targetFrameRate = 0;
        }
        else
        {
            QualitySettings.vSyncCount = 0;

            if (_frameRateSlider.value <= 300)
            {
                Application.targetFrameRate = (int) _frameRateSlider.value;
            }
            else
            {
                Application.targetFrameRate = int.MaxValue;
            }
        }

        Time.fixedDeltaTime = 1f / (int) _physicsRateSlider.value;

        _frameRatePanel.gameObject.SetActive(_vsyncSlider.value == 0);

        _colliderTilemap.ClearAllTiles();
        _backgroundTilemap.ClearAllTiles();

        int laneCount = (int) _laneSlider.value;
        const int laneMinX = -18;
        const int laneMaxX = 9;
        int laneY = 6;

        for (int i = 0; i < laneCount; ++i)
        {
            for (int yOffset = i == 0 ? 0 : 1; yOffset < 4; ++yOffset)
            {
                for (int x = laneMinX; x <= laneMaxX; ++x)
                {
                    if (yOffset == 0 || yOffset == 3 || x == laneMinX || x == laneMaxX)
                    {
                        _colliderTilemap.SetTile(new Vector3Int(x, laneY, 0), _wallTile);
                    }
                    else
                    {
                        _backgroundTilemap.SetTile(new Vector3Int(x, laneY, 0), _backgroundTile);
                    }
                }

                laneY -= 1;
            }
        }

        CameraMinY = laneY;

        for (int i = 0; i < _players.Count; ++i)
        {
            Destroy(_players[i].gameObject);
        }
        _players.Clear();

        while (_players.Count < laneCount)
        {
            Player player = Instantiate(_playerPrefab);
            _players.Add(player);
        }

        if (_followLane >= 0)
        {
            if (_followLane > _players.Count - 1)
            {
                _followLane = _players.Count - 1;
            }

            _mainCamera.Follow(_players[_followLane]);
        }

        while (!_sharedSettings && _laneCanvases.Count < laneCount ||
               _sharedSettings && _laneCanvases.Count < 1)
        {
            LaneCanvas newLaneCanvas = Instantiate(_laneCanvasPrefab);

            if (_laneCanvases.Count > 0)
            {
                LaneCanvas lastLaneCanvas = _laneCanvases[_laneCanvases.Count - 1];
                newLaneCanvas.MethodType = lastLaneCanvas.MethodType;
                newLaneCanvas.BodyType = lastLaneCanvas.BodyType;
                newLaneCanvas.MovementType = lastLaneCanvas.MovementType;
                newLaneCanvas.InterpolationType = lastLaneCanvas.InterpolationType;
            }

            _laneCanvases.Add(newLaneCanvas);
        }

        for (int i = 0; i < _laneCanvases.Count; ++i)
        {
            if (!_sharedSettings && i < laneCount ||
                _sharedSettings && i == 0)
            {
                _laneCanvases[i].gameObject.SetActive(true);
            }
            else
            {
                _laneCanvases[i].gameObject.SetActive(false);
            }

            if (i < laneCount)
            {
                _laneCanvases[i].Init(i, _players[i]);
            }
        }

        DisplayStyle displayStyle = (DisplayStyle) (int) _displaySlider.value;

        for (int i = 0; i < _players.Count; ++i)
        {
            Player player = _players[i];

            LaneCanvas laneCanvas;

            if (!_sharedSettings)
            {
                laneCanvas = _laneCanvases[i];
                laneCanvas.transform.position = new Vector3(14.75f, 5f - i * 3, 0);
            }
            else
            {
                laneCanvas = _laneCanvases[0];
                laneCanvas.transform.position = new Vector3(14.75f, 5f, 0);
            }

            player.Restart(
                i,
                new Vector3(-16, 5 - i * 3, 0),
                laneCanvas.MethodType,
                laneCanvas.BodyType,
                laneCanvas.MovementType,
                laneCanvas.InterpolationType,
                _speedSlider.value,
                displayStyle);


            if (_hingeSlider.value > 0 && player.HingeJoint != null)
            {
                Vector2 offset = new Vector2(0.0f, 0.7f);
                Vector3 connectedPos = player.transform.position;

                connectedPos.x += offset.x;
                connectedPos.y += offset.y;

                HingeJoint2D playerHingeJoint = player.HingeJoint;

                Rigidbody2D attachment = GameObject.Instantiate(Simulation.Instance.AttachmentPrefab, connectedPos, Quaternion.identity, _attachmentParent);

                playerHingeJoint.connectedBody = attachment;
                playerHingeJoint.connectedAnchor = offset;
                playerHingeJoint.enabled = true;

                _attachments.Add(attachment);
            }


            float x = -13;

            float y = _players[i].transform.position.y;
            float yOffset = 0;

            int ballCount = (int)_ballSlider.value;

            for (int j = 0; j < ballCount; ++j)
            {
                Transform transform = Instantiate(_ballPrefab, _ballParent);
                transform.position = new Vector3(x, y + (yOffset % 1f) - 0.5f);
                _balls.Add(transform);

                x += 1.2f;
                if (x > 7)
                {
                    x = -13;
                }

                yOffset += Mathf.PI;
            }
        }
    }
  
    private void UpdateTextFields()
    {
        _speedText.text = _speedSlider.value.ToString("0.0");

        if (_vsyncSlider.value == 0)
        {
            _vsyncText.text = "Off";
        }
        else
        {
            _vsyncText.text = "On";
        }

        if (_frameRateSlider.value <= 300)
        {
            _frameRateText.text = _frameRateSlider.value.ToString();
        }
        else
        {
            _frameRateText.text = "Unlimited";
        }

        _physicsRateText.text = _physicsRateSlider.value.ToString();
        _ballText.text = _ballSlider.value.ToString();
        _laneText.text = _laneSlider.value.ToString();

        if (_hingeSlider.value == 0)
        {
            _hingeText.text = "Off";
        }
        else
        {
            _hingeText.text = "On";
        }

        if (_displaySlider.value == 0)
        {
            _displayText.text = "Sprite";
        }
        else if (_displaySlider.value == 1)
        {
            _displayText.text = "Both";
        }
        else if (_displaySlider.value == 2)
        {
            _displayText.text = "Rigidbody";
        }
        else
        {
            _displayText.text = "???";
        }

        _settingsText.text = _sharedSettings ? "Shared" : "Per Lane";
    }

    private void PerformPauseIfRequested()
    {
        if (_laneCanvases.Count < 1)
        {
            return;
        }

        if (Time.inFixedTimeStep != (_laneCanvases[0].MethodType == MethodType.FixedUpdate))
        {
            return;
        }

        if (Input.GetKeyDown(KeyCode.P))
        {
            _pauseStopwatch = Stopwatch.StartNew();
        }

        if (Input.GetKeyUp(KeyCode.P) && _pauseStopwatch != null)
        {
            _pauseStopwatch.Stop();
            _log?.Write($"PAUSE in {(Time.inFixedTimeStep ? "FixedUpdate" : "Update")} for {_pauseStopwatch.ElapsedMilliseconds} ms");
            System.Threading.Thread.Sleep((int) _pauseStopwatch.ElapsedMilliseconds);
            _pauseStopwatch = null;
        }
    }

	#endregion Private Methods
}
