using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using HoloToolkit.Unity;
using UnityEngine.UI;
using HoloToolkit.Unity.SpatialMapping;
using HoloToolkit.Unity.InputModule;

public class PopulateSpace : Singleton<PopulateSpace> {

    [Tooltip("3D object to be placed into the room (once room is scanned)")]
    public GameObject Obj;
    private GameObject currObj;

    [Tooltip("Create a canvas and Text object, link the text object here to see log messages")]
    public Text log;

	// Use this for initialization
	void Start () {
    }

    // Update is called once per frame
    void Update () {
	}

    // handler for placing all prefabs from the ObjectCollection
    public void InstallObjects() {

        GameObject.Find("SpatialMappingCtrlMenu").SetActive(false); // hide Space Scan Ctrl Menu
        SpatialMappingManager.Instance.DrawVisualMeshes = false; // hide Spatial Map Mesh

        currObj = Instantiate(Obj);
        currObj.transform.localScale *= 0.5f;
        currObj.GetComponent<Placeable>().OnSelect(); // activate placement mode

        Button b = GameObject.Find("ExploreBrainButton").GetComponent<Button>();
        b.onClick.AddListener(ExploreBrain); // register onClick event with the Button

        log.text += "Placing object " + Obj.name + "...\n";

    } // InstallObjects()

    public void ExploreBrain()
    {
        // remove the button
        GameObject.Find("ExploreBrainButton").SetActive(false);

        // remove drag & drop placeable script, as otherwise
        // it will interfere with the interaction with the brain segments
        Destroy(currObj.GetComponent<Placeable>());

        // remove Box collider, so that gaze cursor passes to child segments
        Destroy(currObj.GetComponent<BoxCollider>());

        // attach the interaction script to the brain prefab, so that segments can be clicked
        currObj.AddComponent<SegmentHighlights>();
        
    }

}
