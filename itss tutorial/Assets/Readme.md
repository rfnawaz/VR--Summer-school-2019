# Tutorial 1: Spatial Mapping, Gaze, and Gestures

### by Dr Fridolin Wild, Performance Augmentation Lab, Oxford Brookes University

#### for Dr Mattia Montanari and the Immersive Technologies Summer School, Department of Engineering Science, Oxford University



The capability to map and understand the spatiality of the physical environment surrounding us of the new Smart Augmented Reality Glasses like the Microsoft Hololens or the Magic Leap is a game changer. For the first time, this enables pervasive experiences with an optical quality that causes our perception to sometimes struggle to distinguish what is real and what not.

At the same time, this has triggered a paradigm change in Human Computer Interaction, with gaze pointers and gesture control replacing swipes & taps (and mouse pointing and clicking).

In this tutorial, we will be looking into how to set up these three functionalities: spatial mapping (aka 'room scanning'), gaze navigation, and 'click' activation using air-tapping gestures.

The tutorial closes with configuration information on how to set up Unity3D, Visual Studio, and all the needed packages, modules, and SDKs. In theory, this tutorial in itself should enable you to get started from scratch and take you far enough to build your first 'hello world' app - with the world being your own personal reality you created :).



### Spatial Mapping - Prefabs and Configuration

Clean up the game object Hierarchy and delete the camera and the directional light. We will replace these with the correct ones.

For this, import the HoloToolkit 2017.4.3.0 (see configuration section at the end of this tutorial): in the menu, call Assets > Import Package > Custom Package. Once all files are imported, you should see a new folder "HoloToolkit" in your Project file browser.

Search in the Project explorer now for "HoloLensCamera" and drag and drop this prefab into the game object Hierarchy.

Now create an empty game object (Create > Create empty) in the game object Hierarchy and rename it to HoloToolkit. This is to make sure we keep things nice and tidy and have no difficulty finding our game objects and scripts later. 

Now search in the Project explorer for Spatial Mapping, then InputManager, also InteractiveMeshCursor, and SpatialUnderstanding. Add all four to the project Hierarchy. Your Hierarchy looks now like the one depicted below:

### ![1 - htk import and hierarchy setup](.\_screenshots\1 - htk import and hierarchy setup.png)

Let's go through them. We have replaced the standard camera with the HololensCamera - as every Unity project uses a camera to define how to render the scene.

The other prefabs we added support us in scanning the room (and - potentially - cleaning up the meshes scanned with the Spatial Understanding). They help with the gaze and gesture interaction (InputManager). And a cursor was selected to visualise where (and what) the gaze cursor (kind of '3D mouse pointer') rests upon in the environment.

We select the SpatialMapping game object and look at the Inspector to set the number of Triangles Per Cubic meter to 100 only (this will give us less precise meshes, but they will be much faster). Make sure that the Physics Layer is 31, and that the Surface Material is set to Wireframe (there are cool shaders available to pimp this visually). Deactivate the "Auto Start Observer" - we will build our own user interface (UI) for starting and stopping the scan. Do the same with the SpatialUnderstanding: deactivate the option "Auto Begin Scanning".

Short cut: If you just want the SpatialMapping to anchor your game objects, you can leave the "Auto Start Observer" and "Auto Begin Scanning" on and ignore the next section on how to build a UI for scanning rooms. 

![2 - spatialmapping](.\_screenshots\2 - spatialmapping.png)

![4 - spatialunderstanding](.\_screenshots\4 - spatialunderstanding.png)

For the InputManager, there is not much we need to configure. Just make sure to drag and drop the InteractiveMeshCursor from the Hierarchy onto the slot "Cursor".

![3 - inputmanager](.\_screenshots\3 - inputmanager.png)

### Spatial Mapping - UI for room scans

Next we start building up our own user interface. Unity uses a Canvas game object to hold the elements of the user interface together. So the first thing we do is to create game object UI > Canvas and rename it to "SpatialMappingCtrlMenu". 

![4 - ctrl menu - create-canvas](.\_screenshots\4 - ctrl menu - create-canvas.png)

The scale of 1 is rather big for the small display of the Hololens, so we set x/y/z scale to 0.3. 

There are different ways of rendering the UI, but for here, we need to set the Canvas to render to World Space (this will also trigger automatically to pick a different Camera in the sub settings for rendering it).

![4 - ctrl menu - 1 - canvas worldspace](.\_screenshots\4 - ctrl menu - 1 - canvas worldspace.png)

We now create three UI > Text game objects and three UI > Button objects. We rename the text objects to menuTitle, Instructions, and log. The buttons are renamed to StartScanningButton, StopScanningButton, StartAppButton. Your hierarchy now looks like this:

 ![4 - ctrl menu - hierarchy](.\_screenshots\4 - ctrl menu - hierarchy.png)

We rearrange the elements in the scene preview by switching to 2D in the Scene and double clicking the SpatialMappingCtrlMenu canvas in the Hierarchy. Arrange them to something like this:

![4 - ctrl menu - preview](.\_screenshots\4 - ctrl menu - preview.png)

The menu canvas needs to face always to the user ('billboard') and follow the user around ('tag-along', so we search for the HoloToolkit Tagalong and Billboard scripts and attach them to the canvas.

![4 - ctrl menu - canvas - tagalong billboard](.\_screenshots\4 - ctrl menu - canvas - tagalong billboard.png)

We now import the tutorial unity package (itss tutorial 1.unitypackage). Once imported, we add the SpaceScannerManager script (folder "_scripts") and attach it to the menu canvas. 

![4 - ctrl menu - canvas - spacescanner](.\_screenshots\4 - ctrl menu - canvas - spacescanner.png)

The SpaceScannerManager has several configuration settings. We add two mesh map materials (search for WireframeBlue and Occlusion and add them accordingly). Moreover, we drag and drop the Text game object log onto the Log slot. This allows the SpaceScannerManager script to output log messages onto the area we have foreseen for debug output on the menu canvas. 

To fully function, we have to add two more HoloToolkit scripts, SurfaceMeshesToPlanes and RemoveSurfaceVertices. We attach them to the menu canvas as well.

![4 - ctrl menu - canvas - spatial scripts](.\_screenshots\4 - ctrl menu - canvas - spatial scripts.png)

We select for Draw Planes and Destroy Planes the option 'Nothing' each. The SurfaceMeshesToPlanes script is resonsible for identifying plane surfaces from the raw mesh, detecting floor, walls, tables, and the like. It is possible to replace the raw mesh with these identified surfaces, but we don't want that, as it makes the preview flicker during the scanning. The RemoveSurfaceVertices is resonsible for thinning out the mesh, where possible. 

The SpaceScannerManager script takes care of all this. It communicates with the SpatialMapping to start and stop the observer responsible for scanning the room mesh, cleaning the mesh once scanned, and checking whether enough surfaces have been scanned (satisfying the min floors and min walls requirements set). Moreover, it provides three methods for the buttons to control: StartScanning(), StopScanning(), and StartApp().

```c#
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
            CreatePlanes(); // convert raw mesh to planes
        } // if scanning

    } // Update()

    // MeshToPlanes
    private void MeshToPlanes(object source, System.EventArgs args)
    {

        // get floor/table planes
        horizontal = SurfaceMeshesToPlanes.Instance.GetActivePlanes(PlaneTypes.Table | PlaneTypes.Floor);

        // get wall planes
        vertical = SurfaceMeshesToPlanes.Instance.GetActivePlanes(PlaneTypes.Wall);

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
        SpatialMappingManager.Instance.SetSurfaceMaterial(defaultMapMaterial);
        SurfaceMeshesToPlanes.Instance.MakePlanesComplete += MeshToPlanes;
        SpatialMappingManager.Instance.StartObserver();
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

```

The final act for getting the menu to run is now to register the three control methods StartScanning, StopScanning, and StartApp with the according buttons.

![4 - ctrl menu - 9 button 1 click](.\_screenshots\4 - ctrl menu - 9 button 1 click.png)

For this, we add (+) an On Click() event to the button and drag and drop the menu canvas game object onto the game object slot. Then we can select SpaceScannerManager > StartScanning as the method to call when the button is clicked.

We apply the according method calls for StopScanning and StartApp analogously to the other two buttons.

### Placement with the gaze pointer and air tap

The error message we see in the console is telling us that the call to the singleton PopulateSpace is not (yet) working. No surprise, we haven't added it yet.

We first create a new empty game object and rename it to ObjectCollection. Moreover, we add as a child another canvas, into which we place another Text game object. The canvas gets renamed to "UICanvas" and the Text to "log". For the canvas, again, we set the Render Mode to "World Space", and we add a Billboard and Tagalong script.

We attach two scripts PopulateSpace and GazeGestureManager (from "_scripts") to the empty game object ObjectCollection. For PopulateSpace we drag and drop the log Text game object from the UIcanvas onto the Log slot in the Inspector. 

We have prepared a 3D model prefab, "brain", which has the Placeable script already attached. The Placeable script contains the code needed for conveniently displaying through the use of coloured bounding boxes and shadows, where the user can actually place an object. Once a suitable location is found, the user simply airtaps to drop the object.

![6 - brain](.\_screenshots\6 - brain.png)

To connect the brain model prefab with the ObjectCollection, the other configuration slot, "Obj", gets filled with the brain prefab (folder "_prefabs"). This will, once the scanning finished and StartApp() was triggered, execute the InstallObjects() method in the PopulateSpace singleton - there then instantiating a copy of the brain prefab into the scene and triggering its OnSelect method for convenient placement in the environment (on horizontal surfaces only!).

![5 - objectcollection](.\_screenshots\5 - objectcollection.png)

```c#
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

    [Tooltip("Link a Text UI object here to display log messages")]
    public Text log;

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    public void InstallObjects() {

        GameObject.Find("SpatialMappingCtrlMenu").SetActive(false); // hide menu
        SpatialMappingManager.Instance.DrawVisualMeshes = false; // hide map mesh

        GameObject currObj = Instantiate(Obj);
        currObj.transform.localScale *= 0.5f;
        currObj.GetComponent<Placeable>().OnSelect(); // activate placement mode
        log.text += "Placing object " + Obj.name + "...\n";

    } // InstallObjects()

}
```

### Testing

The simplest way to test the app is to use holographic remoting. For this, the Holographic Remoting Player has to be installed on the Hololens from the Windows Store. Once started, the app will display the IP address of the Hololens. If your development machine is on the same network (and the network is reasonably open - like your home network), then Unity can directly connect to this IP address.

![holographc-remoting](.\_screenshots\holographc-remoting.jpg)

![7 - testing holographic remoting](.\_screenshots\7 - testing holographic remoting.png)

Pressing the big play button, will now launch the app directly on the Hololens, also providing a preview in the Game panel.

![10 - demo - scanning](.\_screenshots\10 - demo - scanning.jpg)

NOTE: Holographic remoting does no longer work when using the Vuforia package. This is to do with the camera not being released from processing (probably for performance reasons).

### Building

Since a few versions back, the HoloToolkit also provides a convenient Build Window, which automates some of the work for you. By hand, you'd have to Build the app, then open this exported ('build') solution with Visual Studio, then build the executable and install it on the glasses. The Build Window is still bit instable and not always every step works - on this machine, for example, installing from there onto the device does not work and I have to use the web device portal for that. Nonetheless, building is a lot more convenient from here, see this:

![8 - build window](.\_screenshots\8 - build window.png)

Once the Unity Project is built, the appx bundle can be built, which then can be installed via the device portal. (Don't forget to add the Dependencies\x86\ files the first time around!).

![11 - device-portal-install](.\_screenshots\11 - device-portal-install.png)

![12 - app](.\_screenshots\12 - app.jpg)

![12 - app - scanning](.\_screenshots\12 - app - scanning.jpg)

![12 - app - brains](.\_screenshots\12 - app - brains.jpg)

### Configuration

This tutorial was written for Unity3D **2017.4.31f1** (15 Aug, 2019). Via the UnityHub the modules for Windows Store .NET Scripting Backend and Windows Store IL2CPP Scripting Backend have been added. It uses Visual Studio **2017** (15.9.15) and the Windows 10 SKD **10.0.17134**, see also release notes at https://github.com/microsoft/MixedRealityToolkit-Unity/releases. 

Beware: When installing through the UnityHub, the version 2017.4.31f1 is not necessarily offered, but has to be selected from the Unity3D website (click on Download Unity in the dark footer of the homepage, then click on Older Versions in the dark footer, then select 2017 to click on the UnityHub Download for the latest version 2017.4.31f1).

![unity3dwebsite2017431f1](.\_screenshots\unity3dwebsite2017431f1.png)

When you create your new project, select as target platform 2017.4.31f1. Once Unity is up and running, delete the camera and lighting from the game object Hierarchy (right click > delete). Then navigate in the menu to File > Build Settings. Select "Universal Windows".

![Buildsettings](.\_screenshots\Buildsettings.png)

Navigate to Edit > Project Settings > Quality. Make sure that for the column with the Windows icon, the default quality settings is set to "very low".

![qualitysettings](.\_screenshots\qualitysettings.png) in 

Then navigate to menu Edit > Project Settings > Player. There are several important settings that need to be done here. First and foremost, under XR settings, VR support has to be ticked and "Windows Mixed Reality" must be added to the list (best clean out the rest).

![Annotation 2019-08-25 100627 - playersettings - xr](.\_screenshots\Playersettings - xr.png)

Furthermore, select the group "Publishing Settings" and make sure it has a unique package name. Don't forget to add a meaningful product name at the very top. Depending on what functionality you will be using, several "Capabilities" have to be added here. For our tutorial, we will be using "Internet Client", "Microphone", "Objects3D", and "Spatial Perception".

![Publishingsettings](.\_screenshots\Publishingsettings.png)

Most importantly, the "Other" settings must set the Configuration setting for Scripting Backend to "IL2CPP" (as .NET is being phased out).

 ![Playersettings - other](.\_screenshots\Playersettings - other.png)

Sometimes, when compiling not via the Unity Mixed Reality Toolkit build window, but directly with Visual Studio 2017, the solution forgets which one is the main one (right click on the non-editor/player solution and set as StartUp Project). Moreover, sometimes the Capabilities are forgotten, in which case you have to find the Package.appxmanifest, click on Capabilities, and make sure the right ones are ticked. Similarly, remnants of packages no longer used sometimes linger around in the configuration files, causing havoc. Vuforia is such an example (and [here](https://forum.unity.com/threads/remove-vuforia-from-project.535923/) is a solution for that).

There are many other pitfalls, as software is still experimental. Cryptic error messages in Visual Studio, however, most often indicate version conflicts between not the right version of Unity, missing Unity modules, not the right version of the Windows 10 SDK, not the right version of Visual Studio, missing automatic updates of nuGet packages, missing Unity support for Visual Studio.

Download the 2017.4.3.0 Refresh Holotoolkit unity package from [here](https://github.com/microsoft/MixedRealityToolkit-Unity/releases/tag/2017.4.3.0-Refresh) (no need for the examples and preview or sources) - we well come back to this in the first step - Spatial Mapping - of the tutorial.

Set up your Unity panels so that they look like this: Scene / Game top left, console bottom left. Hierarchy above Project in the middle column, and Inspector on the right. You can also open Window > Holographic Emulation and drag it to rest with Scene and Game. And you can open the Mixed Reality Toolkit > Build Window.

![panel-layout](.\_screenshots\panel-layout.png)

Good luck.

*NOTE: The 3D brain model is from https://free3d.com/3d-model/brain-18357.html and free for personal use. The Placeable and GazeGestureManager behaviours are from the Microsoft Holoacademy Spatial Mapping tutorial.*