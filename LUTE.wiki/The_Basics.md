# Launch Your LUTE Experience
Follow these quick steps to get started with LUTE. Once you're up and running, delve deeper into LUTE's capabilities and explore the documentation available.

# The Engine Window
All of the creation takes place within the Loga Flow Engine window and you can see your game executing through each component inside this window. We suggest opening the window and keeping it docked initially:

1. Open the window by selecting the item from the top context drop-down menus: `LoGa>Show Engine Menu`

![showWindowContexts](https://github.com/IoIoToTM/LoGaCulture-Authorship-Tool/assets/9216959/e6966465-e31a-4664-a7c4-c42a93b972c8)

2. Drag-and drop the window to the location you wish to dock it:

![windowDocked](https://github.com/IoIoToTM/LoGaCulture-Authorship-Tool/assets/9216959/ddadfc09-9ffa-44d2-9548-afb551ef436b)

# Examples
When importing the LUTE plugin, a folder containing multiple examples is also included. Such examples showcase some of the core features of the system and include _Ghost Hunt_, _A Simple Walk_ and _XR Showcase_.

When loading examples, one should consider having the Flow Engine window open to immediately show how each example scene is constructed in the Flow Engine. Users can then enter play mode and watch each node and orde being executed as the game is played through.

# Playmode Tips
It is important to note that, as with all Unity projects, you can make changes whilst the scene is running (i.e., whilst in play mode) but these changes are _ephemeral_. That is, they only last when the scene is running and will revert back to their original setting when leaving play mode.

You can always save these settings by copying properties or saving them as objects while you play through the game but always remember to save your scene when in edit mode **frequently!**

Working in Unity Play Mode can lead to editing objects which are then forgotten when outside of play mode. To prevent this confusion, set a clear visual tint when in Play Mode. To do this:

1. Go to `Edit > Preferences (Windows) or Unity > Preferences (Mac).`
2. Select Colors.

![playModeTint](https://github.com/IoIoToTM/LoGaCulture-Authorship-Tool/assets/9216959/ee470477-e369-4a51-ac4a-8284fe89f21e)

3. Choose a light-colored tint (e.g., light green).
4. Close the window (changes are saved automatically).

Now, your editor will be tinted whenever you enter Play Mode, providing a clear visual cue.

# Creating your first scene

Creating a new scene in Unity for your first game is rather simple.

1. Choose ```File | New Scene```
2. Note: if you have any unsaved changes for the current scene you need to either save or abandon them before a new scene can be created.

3. You should now have a new scene, with a Hierarchy containing just one gameObject, a Main Camera. The new scene will have been give the default name "Untitled", which you can see in the title of the Application window.

4. Good practice is to save your scene (in the right place, with the right name), before creating your work in the scene. You should save most of your scenes in the root of your project "Assets" folder. To save your scene first choose menu: ```File | Save Scene As...```

5. Choose the location and name (we'll choose folders "Assets" and scene name "demo1"):

6. Once you have successfully saved the scene you should now see the new scene file "demo1" in your Assets folder in the Project window, and you should also see in the Application window title that you are currently editing the scene named "demo1".

# Menu: Loga > Create
The LoGa>Create submenu offers the following items:

![createEngineMenu](https://github.com/IoIoToTM/LoGaCulture-Authorship-Tool/assets/9216959/8789d296-78e8-40a9-ae6a-a0f2cf677c9a)

1. Create an empty engine
2. Create an engine with blueprint or add a blueprint to an exisiting engine

# Creating your first Flow Engine
To create a LUTE Flow Engine: 

1. Select `Loga>Create>Engine`
2. A new 'BasicFlowEngine' Game Object should appear in the hierarchy window.
3. If the Flow Engine window was not open or created, this will be shown.
4. If the new Flow Engine Game Object is not selected then please select this and you will see the Flow Engine properties in the Inspector Window and the properties on the Flow Engine window:

![emptyEngine](https://github.com/IoIoToTM/LoGaCulture-Authorship-Tool/assets/9216959/89eff296-fb9c-40b9-acb2-6ab86e0873fb)

5. As visible, when a new Engine is created a single command Node is created named 'Node' which has the `Game Started` Event Handler attached (meaning it will start when the game enters **Play Mode**).

The engine has a few properties you can adjust in the inspector. These are all related to the visuals of the engine and window besides from the `Demo Map Mode` boolean check. This switches between using a dummy location on the map or the real device location (useful for testing location experiences in the editor).

# Node Properties
Clicking on a a node in the engine window will show the inspector for it. Here, we can change some basic values of the node and begin to add `orders` to begin building out your experience. To begin, change the node's name, description and tint. You should see these update in the window as well. A node's description is visible when hovering over it (and this behaviour can be toggled on/off in the engine).

1. Create a new LoGa Engine and then click the basic starting node
2. When shown the inspector for the node, update its name, description and colour (tint)

![starterNode](https://github.com/IoIoToTM/LoGaCulture-Authorship-Tool/assets/9216959/07f99996-022f-454c-88d0-d5cf7feeec90)

# Adding Order
Adding some basic mechanics to your first game is straightforward. In essence, `nodes `drive the experiences and `orders `are the individual mechanics and components that a node will iterate through when called. The basic structure is: _Engine>Node>Orders_.

A common trope you will find in narrative games is dialogue and dialogue boxes. Let's try adding this to our experience now:

1. Using your new Flow Engine window, select the starter node.
2. Click on the 'Plus' icon to show the order menu. This menu contains all possible orders that you can add to nodes (developers can add more orders with ease - see [page on custom orders]. You can search this menu for easier navigation.
3. Choose `Narrative>Dialogue`:

![dialogueBox](https://github.com/IoIoToTM/LoGaCulture-Authorship-Tool/assets/9216959/cb300822-48d5-4993-ab12-41cd2a872300)

4. In the inspector window you should see more details to add information about your dialogue order:

![dialogueEditor](https://github.com/IoIoToTM/LoGaCulture-Authorship-Tool/assets/9216959/1c8700d7-1bfe-476b-8f7b-bc77ff02892c)

5. You can add text, change characters and more but for now we will add a simple piece of text.
6. Run the scene and LUTE will create a dialogue window and write the text that was inputted.

![dialogueExample](https://github.com/IoIoToTM/LoGaCulture-Authorship-Tool/assets/9216959/81c6fd09-05da-4167-b6f3-821647e062cf)

You have just created your first experience using the LUTE authoring tool! Checkout further tutorials for more detailed stuff!
