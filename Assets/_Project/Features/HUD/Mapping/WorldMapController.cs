using UnityEngine;
using UnityEngine.EventSystems;

namespace Wordania.HUD.Mapping
{
    public class WorldMapController : MapRenderer, IDragHandler, IScrollHandler
    {
        private Rect _currentViewRect = new(0, 0, 1, 1);

        public void OnDrag(PointerEventData eventData)
        {
            float deltaX = eventData.delta.x / Screen.width * _currentViewRect.width;
            float deltaY = eventData.delta.y / Screen.height * _currentViewRect.height;

            _currentViewRect.x -= deltaX;
            _currentViewRect.y -= deltaY;
            
            ApplyRect();
        }

        public void OnScroll(PointerEventData eventData)
    {
        RectTransform rectTransform = _displayImage.rectTransform;

        if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(
            rectTransform, 
            eventData.position, 
            eventData.pressEventCamera, 
            out Vector2 localPointerPosition))
        {
            return;
        }

        Vector2 normalizedPointerPos = new(
            (localPointerPosition.x + rectTransform.rect.width * rectTransform.pivot.x) / rectTransform.rect.width,
            (localPointerPosition.y + rectTransform.rect.height * rectTransform.pivot.y) / rectTransform.rect.height
        );

        float scrollAmount = eventData.scrollDelta.y * _config.WorldMapZoomSensitivity;
        float oldWidth = _currentViewRect.width;
        float oldHeight = _currentViewRect.height;

        float newWidth = Mathf.Clamp(oldWidth - (oldWidth * scrollAmount), _config.WorldMapMinZoom, _config.WorldMapMaxZoom);
        float newHeight = Mathf.Clamp(oldHeight - (oldHeight * scrollAmount), _config.WorldMapMinZoom, _config.WorldMapMaxZoom);

        float deltaWidth = newWidth - oldWidth;
        float deltaHeight = newHeight - oldHeight;

        _currentViewRect.x -= deltaWidth * normalizedPointerPos.x;
        _currentViewRect.y -= deltaHeight * normalizedPointerPos.y;

        _currentViewRect.width = newWidth;
        _currentViewRect.height = newHeight;

        ApplyRect();
    }

    private void ApplyRect()
    {
        _currentViewRect.x = Mathf.Clamp(_currentViewRect.x, 0f, 1f - _currentViewRect.width);
        _currentViewRect.y = Mathf.Clamp(_currentViewRect.y, 0f, 1f - _currentViewRect.height);
        
        _displayImage.uvRect = _currentViewRect;
    }
    }
}