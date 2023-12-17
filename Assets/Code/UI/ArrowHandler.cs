using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ArrowHandler : MonoBehaviour
{
    public Vector3 unitVector;
    public float deltaV;
    public StateTracker isDragging = new StateTracker();

    private MarkerBehaviour marker;
    private Vector3 startPosition;
    private Vector3 startPointHit;

    private void Start()
    {
        marker = GetComponentInParent<MarkerBehaviour>();
        startPosition = transform.localPosition;
    }

    private void Update()
    {
        transform.localScale = Mathf.Lerp(marker.minArrowSize, marker.maxArrowSize, deltaV / marker.maxDeltaV) * Vector3.one;

        if (Input.GetMouseButtonDown(0) && !GameManager.Instance.isRotatingCamera)
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit, Mathf.Infinity, marker.layerMask) && hit.collider.gameObject == gameObject)
            {
                isDragging.SetState(true);
                GameManager.Instance.isInteractingUI = true;
                startPointHit = hit.point;
            }
        }

        if (isDragging.IsOn())
        {
            Vector2 mousePosition = Input.mousePosition;
            Vector2 startPoint = Camera.main.WorldToScreenPoint(startPointHit);
            Vector2 endPoint = Camera.main.WorldToScreenPoint(startPointHit + transform.forward * marker.maxDragCourse);
            Vector2 lineVector = endPoint - startPoint;
            Vector2 mouseVector = endPoint - mousePosition;
            float lerpFactor = 1 - Mathf.Clamp(Vector2.Dot(mouseVector, lineVector) / Vector2.Dot(lineVector, lineVector), 0f, 1f);
            deltaV = marker.GetScaledDeltaV(lerpFactor);
            lerpFactor = Mathf.Pow(lerpFactor, marker.springiness);
            transform.localPosition = startPosition + unitVector * lerpFactor * marker.maxDragCourse * marker.visualOffsetFactor;
        }

        if (Input.GetMouseButtonUp(0))
        {
            isDragging.SetState(false);
            GameManager.Instance.isInteractingUI = false;
            transform.localPosition = startPosition;
            deltaV = 0f;
        }
        
    }
}
