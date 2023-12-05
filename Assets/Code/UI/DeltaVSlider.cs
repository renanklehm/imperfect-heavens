using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class DeltaVSlider : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    public Vector3 unitVector;
    private MarkerBehaviour markerBehaviour;
    private Slider slider;
    private bool isDragging;

    private void Start()
    {
        markerBehaviour = GetComponentInParent<MarkerBehaviour>();
        slider = GetComponent<Slider>();
    }

    private void Update()
    {
        if (isDragging)
        {
            markerBehaviour.SetDeltaV(unitVector, slider.value);
        }
    }

    void IPointerDownHandler.OnPointerDown(PointerEventData eventData)
    {
        isDragging = true;
    }

    void IPointerUpHandler.OnPointerUp(PointerEventData eventData)
    {
        markerBehaviour.ClearSliders();
        isDragging = false;
    }
}
