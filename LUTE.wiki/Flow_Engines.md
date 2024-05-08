The heart of LUTE revolves around the Flow Engine and the handy window. Unity scenes can have multiple engines and these will all work simultaneously.

# Define: Flow Engine
The Flow Engine contains `Nodes` which execute all of your experience features and mechanics (from simple dialogue to intricate mini-games). Typically, we find that one Engine in a single scene is enough; if you have a rather large game you could split this into multiple engines or scenes (and the Nodes will allow for scene management).

A typical Engine contains Nodes and Groups which are linked either directly (i.e., by calling one another) or by 'unlocking' other Nodes (see [Unlocking Nodes](Unlocking_Nodes) for more details).

# The Flow Engine Window
You will need to have the Flow Engine window open to work with LUTE. First of all, open this up and get it docked:

1. Open the window by selecting the item from the top context drop-down menus: `LoGa>Show Engine Menu`

![showWindowContexts](https://github.com/IoIoToTM/LoGaCulture-Authorship-Tool/assets/9216959/e6966465-e31a-4664-a7c4-c42a93b972c8)

2. Drag-and drop the window to the location you wish to dock it:

![windowDocked](https://github.com/IoIoToTM/LoGaCulture-Authorship-Tool/assets/9216959/ddadfc09-9ffa-44d2-9548-afb551ef436b)

# Create a new Flow Engine
If you have not already done so then please create a new Flow Engine for the Window to use:

1. Select `Loga>Create>Engine`
2. A new 'BasicFlowEngine' Game Object should appear in the hierarchy window.
3. If the Flow Engine window was not open or created, this will be shown.
4. If the new Flow Engine Game Object is not selected then please select this and you will see the Flow Engine properties in the Inspector Window and the properties on the Flow Engine window:

![emptyEngine](https://github.com/IoIoToTM/LoGaCulture-Authorship-Tool/assets/9216959/89eff296-fb9c-40b9-acb2-6ab86e0873fb)

5. As visible, when a new Engine is created a single command Node is created named 'Node' which has the `Game Started` Event Handler attached (meaning it will start when the game enters **Play Mode**).

# Navigating the Flow Engine Window
You can navigate around the Flow Engine window by **panning **and **zooming**.

Panning means moving the contents of the window as if they were on a piece of paper (much like you drag a map using Google or Apple or Bing Maps). You hold right or middle mouse button down to move around and let go when at the desired location.

![panningWindow](https://github.com/IoIoToTM/LoGaCulture-Authorship-Tool/assets/9216959/8b09b531-471c-49cc-9ef8-e3d790effa86)

Zooming allows you to view your window and nodes from a greater distance or see them closer for more detail. You can eitehr zoom the window using the slider in the top left (shown below) or using the mouse scroll wheel/trackpad. 

![zoomWindow](https://github.com/IoIoToTM/LoGaCulture-Authorship-Tool/assets/9216959/6a48a0cb-48d6-4a2e-a0a5-4f6d29d36812)

# Toolbar Buttons
Next to the zoom slider there are some other buttons which include annotations and map features (see [Annotations](Annotations) and [Map and Location](Maps_Locations) for more details). The other button is `Centre Window` which will centre the window around all the nodes on the window (this is useful when wanting to get an overview of your project of if your ever get lost!)

This concludes the guide for using the Flow Engine and window. We suggest playing around with the examples and the above guide to get comfortable with navigating and using the Flow Engine! Good luck and have fun!