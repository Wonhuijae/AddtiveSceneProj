using UnityEngine;

public interface IMouseInteractable
{
    public void HoverEnter();
    public void HoverExit();
    public void ClickEnter();
    public void ClickExit();
}

public interface IDragInteractable
{
    public void DragStart();
    public void Dragging();
    public void DragEnd();
}