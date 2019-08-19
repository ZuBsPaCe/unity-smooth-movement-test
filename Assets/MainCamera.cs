using System;
using UnityEngine;
using zs;

public class MainCamera : MonoBehaviour
{
	#region Serializable Fields

    [SerializeField]
    private float _cameraSpeed = 5;

    [SerializeField]
    private int _border = 25;

    [SerializeField]
    private int _horizontalWorldSize = 38;

	#endregion Serializable Fields

	#region Private Vars

    private float _cameraVelocityY = 0;
    private bool _scrolled = false;

    private float _cameraMaxY = 10;

    private int _lastScreenWidth = 0;
    private int _lastScreenHeight = 0;

    private Player _followPlayer = null;

	#endregion Private Vars

	#region Public Vars
	#endregion Public Vars

	#region Public Methods

    public void Follow(Player player)
    {
        if (_followPlayer != null)
        {
            _followPlayer.SetHighlight(false);
        }

        _followPlayer = player;
        player.SetHighlight(true);
    }

    public void Unfollow()
    {
        if (_followPlayer != null)
        {
            _followPlayer.SetHighlight(false);

            _followPlayer = null;
            transform.position = new Vector3(0, transform.position.y, transform.position.z);
        }
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
        if (_lastScreenWidth != Screen.width ||
            _lastScreenHeight != Screen.height)
        {
            float unitsPerPixel = (float) _horizontalWorldSize / Screen.width;
            Camera.main.orthographicSize = Screen.height * unitsPerPixel * 0.5f;


            _lastScreenWidth = Screen.width;
            _lastScreenHeight = Screen.height;
        }
    }

    void LateUpdate()
    {
        if (_followPlayer == null)
        {
            float newCameraVelocityY = 0;

            if (Input.mouseScrollDelta.y > 0)
            {
                newCameraVelocityY = _cameraSpeed;
                _scrolled = true;
            }
            else if (Input.mouseScrollDelta.y < 0)
            {
                newCameraVelocityY = -_cameraSpeed;
                _scrolled = true;
            }
            else if (Input.GetKey(KeyCode.UpArrow) ||
                Input.GetKey(KeyCode.W))
            {
                newCameraVelocityY = _cameraSpeed;
                _scrolled = false;
            }
            else if (Input.GetKey(KeyCode.DownArrow) ||
                     Input.GetKey(KeyCode.S))
            {
                newCameraVelocityY = -_cameraSpeed;
                _scrolled = false;
            }
            else if (Input.mousePosition.y >= 0 &&
                     Input.mousePosition.y < Screen.height)
            {
                float distanceToUpperBorder = Screen.height - Input.mousePosition.y;
                float distanceToLowerBorder = Input.mousePosition.y;

                if (distanceToUpperBorder <= _border)
                {
                    newCameraVelocityY = (1f - distanceToUpperBorder / _border) * _cameraSpeed;
                    _scrolled = false;
                }
                else if (distanceToLowerBorder <= _border)
                {
                    newCameraVelocityY = (1f - distanceToLowerBorder / _border) * -_cameraSpeed;
                    _scrolled = false;
                }

                float sideFactor = Mathf.Max(0, 1f - Math.Abs(Input.mousePosition.x - Screen.width / 2f) / (Screen.width / 2f) * 1.4f);
                newCameraVelocityY *= sideFactor;
            }

            if (newCameraVelocityY == 0)
            {
                if (!_scrolled)
                {
                    _cameraVelocityY = 0;
                }
                else
                {
                    if (_cameraVelocityY > 0)
                    {
                        _cameraVelocityY -= Time.deltaTime * _cameraSpeed * 2;
                        if (_cameraVelocityY < 0)
                        {
                            _cameraVelocityY = 0;
                        }
                    }
                    else if (_cameraVelocityY < 0)
                    {
                        _cameraVelocityY += Time.deltaTime * _cameraSpeed * 2;
                        if (_cameraVelocityY > 0)
                        {
                            _cameraVelocityY = 0;
                        }
                    }
                }
            }
            else
            {
                _cameraVelocityY = newCameraVelocityY;
            }

            Vector3 cameraPos = transform.position;
            cameraPos += _cameraVelocityY * Time.deltaTime * Vector3.up;

            if (cameraPos.y < Simulation.Instance.CameraMinY)
            {
                cameraPos = new Vector3(cameraPos.x, Simulation.Instance.CameraMinY, cameraPos.z);
            }

            if (cameraPos.y > _cameraMaxY)
            {
                cameraPos = new Vector3(cameraPos.x, _cameraMaxY, cameraPos.z);
            }

            transform.position = cameraPos;
        }
        else
        {
            transform.position = new Vector3(_followPlayer.SpriteRenderer.transform.position.x, _followPlayer.SpriteRenderer.transform.position.y, transform.position.z);
        }
    }

	#endregion MonoBehaviour

	#region Private Methods
	#endregion Private Methods
}
