using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class NestedScrollRect : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    [SerializeField] private ScrollRect parentScrollRect;
    [SerializeField] private ScrollRect currentScrollRect;
    [SerializeField] private bool horizontal = true;

    private bool routeToParent = false;

    public void Setup(ScrollRect parent, ScrollRect current, bool isHorizontal)
    {
        parentScrollRect = parent;
        currentScrollRect = current;
        horizontal = isHorizontal;
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (parentScrollRect == null || currentScrollRect == null)
            return;

        routeToParent = horizontal
            ? Mathf.Abs(eventData.delta.y) > Mathf.Abs(eventData.delta.x)
            : Mathf.Abs(eventData.delta.x) > Mathf.Abs(eventData.delta.y);

        if (routeToParent)
            parentScrollRect.OnBeginDrag(eventData);
        else
            currentScrollRect.OnBeginDrag(eventData);
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (parentScrollRect == null || currentScrollRect == null)
            return;

        if (routeToParent)
            parentScrollRect.OnDrag(eventData);
        else
            currentScrollRect.OnDrag(eventData);
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (parentScrollRect == null || currentScrollRect == null)
            return;

        if (routeToParent)
            parentScrollRect.OnEndDrag(eventData);
        else
            currentScrollRect.OnEndDrag(eventData);

        routeToParent = false;
    }
}