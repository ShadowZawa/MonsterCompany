

using UnityEngine;
using UnityEngine.EventSystems;

public class CameraManager : MonoBehaviour
{
    [Header("Camera Bounds")]
    public float minXPos = -10f;
    public float maxXPos = 50f;
    public float minYPos = -15f;
    public float maxYPos = 10f;

    [Header("Touch Settings")]
    public float dragSpeed = 0.5f;
    public float minMoveDist = 10f; // 最小移動距離，避免誤觸

    private bool isDragging = false;
    private Camera mainCamera;
    private Vector3 lastValidPosition;

    void Start()
    {
        mainCamera = Camera.main;
        lastValidPosition = transform.position;
    }

    void Update()
    {
        if (!TowerBuilder.IsBuilding)
        {
            HandleSingleTouchInput();
        }
        else
        {
            HandleDoubleTouchInput();
        }
    }

    void HandleSingleTouchInput()
    {
        if (Input.touchCount != 1) 
        {
            isDragging = false;
            return;
        }

        Touch touch = Input.GetTouch(0);

        switch (touch.phase)
        {
            case TouchPhase.Began:
                isDragging = true;
                break;

            case TouchPhase.Moved:
                if (!isDragging) return;

                // 計算在世界空間中的移動距離
                float orthoSize = mainCamera.orthographicSize;
                float screenHeight = Screen.height;
                float worldToScreenRatio = orthoSize * 2f / screenHeight;

                Vector2 delta = touch.deltaPosition;
                Vector3 moveDirection = new Vector3(
                    -delta.x * worldToScreenRatio,
                    -delta.y * worldToScreenRatio,
                    0
                ) * dragSpeed;

                MoveCamera(moveDirection);
                break;

            case TouchPhase.Ended:
            case TouchPhase.Canceled:
                isDragging = false;
                break;
        }
    }

    void HandleDoubleTouchInput()
    {
        if (Input.touchCount != 2)
        {
            isDragging = false;
            return;
        }

        Touch touch1 = Input.GetTouch(0);
        Touch touch2 = Input.GetTouch(1);

        if (touch1.phase == TouchPhase.Began || touch2.phase == TouchPhase.Began)
        {
            isDragging = true;
        }
        else if (isDragging && (touch1.phase == TouchPhase.Moved || touch2.phase == TouchPhase.Moved))
        {
            // 使用兩指中心點的移動來控制相機
            Vector2 prevCenterPoint = (touch1.position - touch1.deltaPosition + touch2.position - touch2.deltaPosition) * 0.5f;
            Vector2 currentCenterPoint = (touch1.position + touch2.position) * 0.5f;
            Vector2 deltaPosition = currentCenterPoint - prevCenterPoint;

            float worldToScreenRatio = mainCamera.orthographicSize * 2f / Screen.height;
            Vector3 moveDirection = new Vector3(
                -deltaPosition.x * worldToScreenRatio,
                -deltaPosition.y * worldToScreenRatio,
                0
            ) * dragSpeed;

            MoveCamera(moveDirection);
        }
        
        if (touch1.phase == TouchPhase.Ended || touch2.phase == TouchPhase.Ended)
        {
            isDragging = false;
        }
    }

    void MoveCamera(Vector3 moveDirection)
    {
        Vector3 newPosition = transform.position + moveDirection;
        
        // 限制x, y範圍
        newPosition.x = Mathf.Clamp(newPosition.x, minXPos, maxXPos);
        newPosition.y = Mathf.Clamp(newPosition.y, minYPos, maxYPos);
        newPosition.z = transform.position.z; // 保持z軸不變

        if (IsPositionValid(newPosition))
        {
            transform.position = newPosition;
            lastValidPosition = newPosition;
        }
        else
        {
            transform.position = lastValidPosition;
        }
    }

    bool IsPositionValid(Vector3 position)
    {
        return position.x >= minXPos && position.x <= maxXPos &&
               position.y >= minYPos && position.y <= maxYPos;
    }

#if UNITY_EDITOR
    void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Vector3 center = new Vector3((maxXPos + minXPos) * 0.5f, (maxYPos + minYPos) * 0.5f, 0);
        Vector3 size = new Vector3(maxXPos - minXPos, maxYPos - minYPos, 0);
        Gizmos.DrawWireCube(center, size);
    }
#endif
}