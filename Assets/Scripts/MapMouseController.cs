using UnityEngine;
using UnityEngine.InputSystem;

public class MapMouseController : MonoBehaviour
{
    public Transform mapRoot;

    public InputActionReference zoomAction;     // Mouse Scroll
    public InputActionReference panAction;      // Mouse Delta
    public InputActionReference panButtonAction;// Right Click

    public float zoomSpeed = 0.1f;
    public float minZoom = 0.5f;
    public float maxZoom = 3f;
    public float panSpeed = 0.01f;
    Vector3 baseScale;
    float baseY;

    float targetZoom = 1f;

    private void Awake()
    {
        baseScale = mapRoot.localScale;
        baseY = mapRoot.position.y;
    }

    void OnEnable()
    {
        zoomAction.action.Enable();
        panAction.action.Enable();
        panButtonAction.action.Enable();
    }

    void OnDisable()
    {
        zoomAction.action.Disable();
        panAction.action.Disable();
        panButtonAction.action.Disable();
    }

    void Update()
    {
        HandleZoom();
        HandlePan();
    }

    void HandleZoom()
    {
        float scroll = zoomAction.action.ReadValue<Vector2>().y;
        if (Mathf.Abs(scroll) > 0.01f)
        {
            targetZoom += scroll * zoomSpeed;
            targetZoom = Mathf.Clamp(targetZoom, minZoom, maxZoom);

            mapRoot.localScale = baseScale * targetZoom;
        }
    }

    void HandlePan()
    {
        if (!panButtonAction.action.IsPressed())
            return;

        Vector2 delta = panAction.action.ReadValue<Vector2>();

        // 스크린 입력 → 로컬 평면 이동
        Vector3 localMove = new Vector3(
            -delta.x,
            0f,
            -delta.y
        );

        // 확대 시 이동감 보정
        float zoomFactor = 1f / targetZoom;

        mapRoot.position += localMove * panSpeed * zoomFactor;
    }

}
