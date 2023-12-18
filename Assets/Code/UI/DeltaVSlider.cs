using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class DeltaVSlider : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    public Vector3 unitVector;
    private ManeuverNode markerBehaviour;
    private Slider slider;
    public StateTracker isDragging = new StateTracker();

    private void Start()
    {
        markerBehaviour = GetComponentInParent<ManeuverNode>();
        slider = GetComponent<Slider>();
    }

    private void Update()
    {
        if (isDragging.IsOn())
        {
            markerBehaviour.SetDeltaV(unitVector, slider.value);
        }
    }

    void IPointerDownHandler.OnPointerDown(PointerEventData eventData)
    {
        isDragging.SetState(true);
    }

    void IPointerUpHandler.OnPointerUp(PointerEventData eventData)
    {
        markerBehaviour.ClearSliders();
        isDragging.SetState(false);
    }
}
