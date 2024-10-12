AR Tetris 3D with Unity
==============================
Blog post: [https://matthew-bird.com/blogs/Tetris-3D.html](https://matthew-bird.com/blogs/Tetris-3D.html)

### Note
Most AR features are based off of the AR Foundation Samples github repository. Anything new is usually in a folder called Original Content. 

## Premise
My plan for this project was to create Tetris 3D with Unity using the AR Foundation Samples github repository. 

## Controls
Soft fall: hold down top half of screen
Hard fall: swipe down starting at top half of screen

Toggle translation/rotation: tap bottom half of screen
Translation/rotation: drag finger in bottom half of screen

## Mechanics
### Soft Bodies
The soft bodies were created using the [Ideefixze's Softbodies repository's riggedbody feature](https://github.com/Ideefixze/Softbodies/tree/master/Softbodies/Assets/Riggedbody) as well as an armature made in blender. Essentially, the script works by creating rigidbodies at each joint of the armature and attaching springs between them to create a jelly effect, and it seemed to work quite well for the tetris pieces. 

### Rotation
There were two main parts to this problem. First, the script determines which axis to rotate by finding all points on each of the three axes that have a slope/gradient matching our first touch movement. To accomplish this, the derivative is taken of all three axes to create a function for x and y with respect to dy and dx.([desmos](https://www.desmos.com/calculator/oclrhpxkwi)) After this, the script takes the closest point to determine our selected axis. 

After this is done, the script has to continuously update the projection of our finger's position on screen to the axis, and then use this to determine our desired change in rotation. ([desmos](https://www.desmos.com/calculator/jhaxiuzhz4))

### Falling
To accomplish falling physics, the script moves the tetris piece down at a set amount of units every time step until it collides with something. To avoid intersection, the script does a raycast to determine how far it should move if it were to intersect after the movement. After the block collides with a surface or another block, the script enables gravity for the object and the soft body physics for the object, as well as imparting an initial velocity equal to the static fall speed of the object. 

### Hard fall
The hard fall mechanic is similar to falling, raycasting downwards until it collides with something, and then moving the block down accordingly. 

### Overlaying cameras
To avoid issues with the riggedbody script that occurred when scaling objects down to small sizes, the game renders the tetris field and blocks with a separate camera, and changes the position of the camera and orientation of the play field in accordance to the relative position of the phone to the position of the play field in real life. 

### Layer clearing
Due to the fact that the game is non-grid based and in 3 dimensions, layer clearing is a bit different from tetris. To clear layers, since each layer is a 2x10x1 rectangle, the script checks for collisions at each of the 20 points on an invisible grid per layer, and if they all are colliding with blocks, the script clears the layer. To make the process more forgiving, when a point fails the collision test, the script also checks at a -0.5 and +0.5 offset, and if both are colliding, the script still clears the line. To break up the pieces, the script creates cubes at each of the four positions of each shape and removes any that intersect with the layer. 

## Gameplay
The gameplay is pretty good, and the layer clearing doesn't feel too generous nor too strict. However, the game does feel a bit unadorned and monotonous due to a lack of features that I was originally intending to add but lacked the time to do so.(Queue and Hold, Modifiers, Building structures, Bosses, Tutorial)

## How to run
1. Download unity and xcode
2. Download github repo
3. Open github repo in unity and open the anchors scene
4. Press build and run on repeat, fixing issues shown in console until xcode starts giving issues
5. Go to xcode and fix all the issues (like changing com.unity.___ to com.[not unity].___)
6. Build to phone with dev mode on

### AR in Unity
[Extremely basic advice for people new to Unity of the steps I took](https://docs.google.com/document/d/1TtO515bt9dy3PUkCSp-U0Tk1L7XRlGaFhJ36UeXZ_IQ/edit?usp=sharing)
