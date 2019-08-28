using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Windows.Speech;
using HoloToolkit.Unity;
using HoloToolkit.Unity.SpatialMapping;
using UnityEngine.SceneManagement;

/// <summary>
/// The SpaceScannerManager class helps scan the room and then clean the scanned mesh.
/// </summary>
public class SpaceScannerManager : MonoBehaviour {

    public Material defaultMapMaterial; // while scanning
    public Material secondaryMaterial; // after scanning
    public uint minFloors = 1; // minimum number of floor planes needed
    public uint minWalls = 1; // minimum number of walls needed
    List<GameObject> horizontal = new List<GameObject>(); // Collection of floor and table planes
    List<GameObject> vertical = new List<GameObject>(); // Collection of wall planes 

    private bool scanning = false;
    private bool enoughSpace = false;

    // UI elements of the menu
    public Text log;

    // initialization
    void Start () {

    }

    // Update is called once per frame
    void Update () {
        if (scanning)
        {
            CreatePlanes();
        } 
    }

    // MeshToPlanes
    private void MeshToPlanes(object source, System.EventArgs args)
    {

        // get floor/table planes
        horizontal = SurfaceMeshesToPlanes.Instance.GetActivePlanes(PlaneTypes.Table | PlaneTypes.Floor);

        // get wall planes
        vertical = SurfaceMeshesToPlanes.Instance.GetActivePlanes(PlaneTypes.Wall);

        // Check to see if we have enoughSpace horizontal planes (minimumFloors)
        // and vertical planes (minimumWalls), to set holograms on in the world.
        if (horizontal.Count >= minFloors && vertical.Count >= minWalls)
        {
            // We have enoughSpace floors and walls to place our holograms on...
            enoughSpace = true;
        }
        else
        {
            // We do not have enoughSpace floors/walls to place our holograms on...
            enoughSpace = false;
        }
    } // mesh2planes

    /// <summary>
    /// Convert spatial map mesh to planes
    /// </summary>
    private void CreatePlanes()
    {
        SurfaceMeshesToPlanes surfaceToPlanes = SurfaceMeshesToPlanes.Instance;
        if (surfaceToPlanes != null && surfaceToPlanes.enabled)
        {
            surfaceToPlanes.MakePlanes();
        }
    }

    /// <summary>
    /// Thin out surface mesh by removing triangles
    /// </summary>
    /// <param name="boundingObjects"></param>
    private void RemoveTriangles(IEnumerable<GameObject> boundingObjects)
    {
        RemoveSurfaceVertices removeVerts = RemoveSurfaceVertices.Instance;
        if (removeVerts != null && removeVerts.enabled)
        {
            removeVerts.RemoveSurfaceVerticesWithinBounds(boundingObjects);
        }
    } // RemoveTriangles()

    private void OnDestroy()
    {
        if (SurfaceMeshesToPlanes.Instance != null)
        {
            SurfaceMeshesToPlanes.Instance.MakePlanesComplete -= MeshToPlanes;
        }
    } // OnDestroy()

    public void StartScanning()
    {
        enoughSpace = false;
        scanning = true;

        // Update surfaceObserver and storedMeshes to use the same material during scanning.
        SpatialMappingManager.Instance.SetSurfaceMaterial(defaultMapMaterial);

        // Register for the MakePlanesComplete event.
        SurfaceMeshesToPlanes.Instance.MakePlanesComplete += MeshToPlanes;

        if (!SpatialMappingManager.Instance.IsObserverRunning()) SpatialMappingManager.Instance.StartObserver();
        //GameObject.Find("SpatialUnderstanding").GetComponent<SpatialUnderstanding>().AutoBeginScanning = true;

        log.text += "\n";
        log.text += "Starting the Scan\n";
    }

    public void StopScanning()
    {

        if (!enoughSpace)
        {
            log.text += "Not yet enough space scanned, try more wall or floor planes:\n" + horizontal.Count + " horizontal planes\n and " + vertical.Count + " vertical planes.";
        }
        else
        {
            scanning = false;
            log.text += "Stopped scanning - enough space:\n" + horizontal.Count + " horizontal planes\n and " + vertical.Count + " vertical planes.";

            if (SpatialMappingManager.Instance.IsObserverRunning())
            {
                SpatialMappingManager.Instance.StopObserver();
                //GameObject.Find("SpatialUnderstanding").GetComponent<SpatialUnderstanding>().AutoBeginScanning = false;
                log.text += "Stopped SpatialMapping Observer\n";
            } else
            {
                log.text += "Warning: tried to stop observer, but no SpatialMapping Observer running\n";
            }

            RemoveTriangles(SurfaceMeshesToPlanes.Instance.ActivePlanes); // clean up the mesh
            SpatialMappingManager.Instance.SetSurfaceMaterial(secondaryMaterial); // change mesh visualisation to different material

        }
    }

    public void StartApp()
    {
        if (enoughSpace && !scanning)
        {
            log.text += "Starting app.\n";
            PopulateSpace.Instance.InstallObjects();
        } else if (scanning && !enoughSpace)
        {
            log.text += "ERROR: Scanning not complete: please continue to move and gaze around till the map is reasonably large...\n";
        }
        else
        {
            log.text += "ERROR: no spatial map: please move and gaze around to scan the room...\n";
        }
    }

}
