using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public class Raycaster : MonoBehaviour
{
    public float rayDistance = 5f;
    [SerializeField] private IMouseInteractable prevHover;
    [SerializeField] private IMouseInteractable prevClick;
    [SerializeField] private IDragInteractable dragTarget;
    [SerializeField] private LayerMask raycastMask;
    public bool isDragging = false;
    private Mouse mouse;

    private void OnEnable()
    {
        mouse = Mouse.current;
    }

    private void Update()
    {
        if (mouse == null) return;

        // UI 위에 마우스가 있는지 확인
        if (EventSystem.current != null &&
        EventSystem.current.IsPointerOverGameObject())
        {
            // UI 위에 마우스가 있을 때는 상호작용을 무시
            return;
        }

        Vector2 mousePos = mouse.position.ReadValue();
        bool leftButtonDown = mouse.leftButton.wasPressedThisFrame; // Input.GetMouseButtonDown(0)
        bool leftButtonUp = mouse.leftButton.wasReleasedThisFrame; // Input.GetMouseButtonUp(0)

        Ray ray = Camera.main.ScreenPointToRay(mousePos);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, rayDistance, raycastMask))
        {
            IMouseInteractable currentInteractable = hit.collider.GetComponent<IMouseInteractable>();
            IDragInteractable currentDraggable = hit.collider.GetComponent<IDragInteractable>();

            if (leftButtonDown)
            {
                if (currentInteractable != null)
                {
                    prevClick = currentInteractable;
                    currentInteractable.ClickEnter();
                }

                if (currentDraggable != null)
                {
                    dragTarget = currentDraggable;
                    dragTarget.DragStart();
                    isDragging = true;
                }
            }

            if (currentInteractable != prevHover)
            {
                prevHover?.HoverExit();
                currentInteractable?.HoverEnter();
                prevHover = currentInteractable;
            }
        }
        else
        {
            prevHover?.HoverExit();
            prevHover = null;
        }

        if (isDragging)
        {
            dragTarget?.Dragging();
        }

        if (leftButtonUp)
        {
            if (prevClick != null)
            {
                prevClick.ClickExit();
                prevClick = null;
            }

            if (dragTarget != null)
            {
                dragTarget?.DragEnd();
                dragTarget = null;
            }

            isDragging = false;
        }
    }
}
