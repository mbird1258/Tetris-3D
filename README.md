AR Tetris 3D with Unity
==============================

## Premise
My plan for this project was to create Tetris 3D with Unity using the AR Foundation Samples github repository. 

## Mechanics
### Soft Bodies
The soft bodies were created using the [Ideefixze's Softbodies repository's riggedbody feature](https://github.com/Ideefixze/Softbodies/tree/master/Softbodies/Assets/Riggedbody) as well as an armature made in blender. Essentially, the script works by creating rigidbodies at each joint of the armature and attaching springs between them to create a jelly effect, and it seemed to work quite well for the tetris pieces. 

### Rotation
There were two main parts to this problem. First, we have to determine which axis we wish to rotate by finding all points on each of the three axes that have a slope/gradient matching our first touch movement. To accomplish this, we take the derivative of all three axes and create a function for x and y with respect to dy and dx.([desmos](https://www.desmos.com/calculator/oclrhpxkwi)) After this, we just take the closest point to determine our selected axis. 

After this is done, we have to continuously update the projection of our finger's position on screen to the axis, and then use this to determine our desired change in rotation. ([desmos](https://www.desmos.com/calculator/jhaxiuzhz4))

### Falling
To accomplish falling physics, the script moves the tetris piece down at a set amount of units every time step until it collides with something. To avoid intersection, the script

### Hard fall
### Overlaying cameras
### Layer clearing

Everything here is completely based off of the AR Foundation Samples github repository. Anything new is usually in a folder called Original Content. 
