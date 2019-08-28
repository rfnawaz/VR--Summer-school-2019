using System.Collections.Generic;
using UnityEngine;
using HoloToolkit.Unity;
using HoloToolkit.Unity.SpatialMapping;
using HoloToolkit.Unity.InputModule;
using UnityEngine.XR.WSA.Input;
using HoloToolkit;
using System.Linq;

public class SegmentHighlights : MonoBehaviour {

    private GestureRecognizer recogniser;
    private GameObject FocusedObject;
    private Color OldColour;

    // Use this for initialization
    void Start() {

        recogniser = new GestureRecognizer();
        recogniser.SetRecognizableGestures(GestureSettings.Tap);
        recogniser.Tapped += onTapped;
        recogniser.StartCapturingGestures();

    }

    // Update is called once per frame
    void Update() {
        HoverHandler();
    }

    void HoverHandler ()
    {

        GameObject oldFocusObject = FocusedObject;

        var headPosition = Camera.main.transform.position;
        var gazeDirection = Camera.main.transform.forward;
        RaycastHit hitInfo;
        if (Physics.Raycast(headPosition, gazeDirection, out hitInfo))
        {
            // If the raycast hit a hologram, use that as the focused object.
            FocusedObject = hitInfo.collider.gameObject;
        }
        else
        {
            // If the raycast did not hit a hologram, clear the focused object.
            FocusedObject = null;
        }

        // If the focused object changed this frame, add/remove highlight colour
        if (FocusedObject != oldFocusObject)
        {
            string[] segs = { "Brain_Part_02", "Brain_Part_04", "Brain_Part_05", "Brain_Part_06" };
            if (oldFocusObject != null && segs.Any(oldFocusObject.name.Equals))
            {
                RemoveHighlightFromSegment(oldFocusObject);
            }
            if (FocusedObject != null && segs.Any(FocusedObject.name.Equals))
            {
                HighlightSegment(FocusedObject);
            }
        }
    }

    public void HighlightSegment(GameObject Segment)
    {
        OldColour = Segment.GetComponent<Renderer>().material.color;
        Segment.GetComponent<Renderer>().material.color = Color.yellow;
    }

    public void RemoveHighlightFromSegment(GameObject Segment)
    {
        Segment.GetComponent<Renderer>().material.color = OldColour;
    }

    public void onTapped(TappedEventArgs args)
    {

        var headPosition = args.headPose.position;
        var gazeDirection = args.headPose.forward;

        RaycastHit hitInfo;
        if (Physics.Raycast(headPosition, gazeDirection, out hitInfo, 10.0f, Physics.DefaultRaycastLayers)) 
        {
            // If the raycast hit a hologram, use that as the focused object.
            FocusedObject = hitInfo.collider.gameObject;

            string[] segs = { "Brain_Part_02", "Brain_Part_04", "Brain_Part_05", "Brain_Part_06" };
            if (segs.Any(FocusedObject.name.Equals))
            {
                if (FocusedObject.name == "Brain_Part_04")
                {
                    FocusedObject.transform.Translate(Vector3.left * 0.1f, Space.World);
                } else if (FocusedObject.name == "Brain_Part_06")
                {
                    FocusedObject.transform.Translate(Vector3.right * 0.1f, Space.World);
                } else if (FocusedObject.name == "Brain_Part_02")
                {
                    FocusedObject.transform.Translate(Vector3.up * 0.5f, Space.World);
                } else if (FocusedObject.name == "Brain_Part_05")
                {
                    FocusedObject.transform.Translate(Vector3.up * 0.2f, Space.World);
                }
            }

        }
        else
        {
            // If the raycast did not hit a hologram, clear the focused object.
            FocusedObject = null;
        }

    }
}
