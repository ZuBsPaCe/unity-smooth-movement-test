# unity-smooth-movement-test
Try and compare movement types in Unity

### About

There are many ways to move a character in Unity. The Smooth Movement Test Application lets you play around with different settings. You can read more about the different strategies in the [blog entry](https://www.zubspace.com/blog/smooth-movement-in-unity).

![Example of the smooth movement test application.](example.jpg)

You can add multiple lanes with different parameters and compare the result.

### Settings

**Speed**

Changes the target velocity of all characters, specified in Units per Second. All movement types which directly set the position or the velocity will use this exact value. The movement types AddForce, AddImpulse and AddVelocity will use a simple [PID Controller](https://en.wikipedia.org/wiki/PID_controller) which smoothly adjusts the current velocity according to deviation from the specified target velocity.

**VSync**

Enables or disables Vertical Sychronization of the renderer and the screen. If you enable VSync, the frame rate cannot be higher than the refresh rate of your screen. This reduces frame time variance leading to stuttering and screen tearing.

**Frame Rate**

By disabling VSync you will be able to set a target frame rate.

**Balls**

You can add some "Balls" to the simulation. Spheres with a Dynamic Rigidbody2D, which will collide with the character.

**Lanes**

Each lane can have different lane settings. That way you can compare the smoothness of the motion and the effect of the lane settings on the balls.

**Display**

Sprite shows you the position of the character which the player will see in the game, possibly interpolated. Dragging the slider will display the rigidbody position, indicated by a red sqare. This is the current position of the character used by the physic engine.

**Restart**

Restarts the simulation with the same settings.

**Log**

This will output the information about the Update and FixedUpdate cycles. You can inspect their order and 3 values: 1) the real duration of the frame measured by System.Diagnostic.Stopwatch. 2) Time.deltaTime. 3) Time.time.

In the browser application you need to open the developer console to see the output (Press F12). In the standalone application a file will be written alongside the executable called "SmoothMovement.log".

**Settings**

You can specify settings per lane or all lanes can share the same settings. Shared settings are useful to measure the performance of the specified lane settings.

**Follow**

The camera will follow the character sprite with a yellow highlight. Press F to switch this setting on or off. You can jump through the lanes with the up and down arrow keys while following. The camera position is set in LateUpdate.

**Reset**

Resets all settings and restarts the simulation.

**FPS**

Shows the amount of Update cycles per Second, measured by a Stopwatch instance. The value is updated twice per second.

**Variance**

Shows (half) the difference between the smallest and longest Update cycle duration. The value is updated twice per second. A high variance value can lead to stuttering motion.

**Method (Lane setting)**

The movement type will either be applied in Update or FixedUpdate according to this setting. Unity advises us to use FixedUpdate for a stable physic simulation. FixedUpdate also enables the interpolation setting.

**Body Type (Lane setting)**

Switch between a kinematic or dynamic rigidbody used for the character. A dynamic rigidbody enables the usage of movement type AddForce and AddImpulse.

**Movement (Lane setting)**

Select the method used for moving the rigidbody of the character. The procedure is repeated in Update or FixedUpdate depending on the Method set above.

* **GameObject SetPosition** moves the character by setting transform.position directly.
* **Rigidbody SetPosition** moves the character by setting Rigidbody2D.position directly.
* **Rigidbody MovePosition** moves the character by calling Rigidbody2D.MovePosition().
* **Rigidbody SetVelocity** moves the character by setting Rigidbody2D.velocity.
* **Rigidbody AddVelocity** moves the character by adjusting Rigidbody2D.velocity smoothly over time with a simple PID Controller.
* **Rigidbody AddForce** moves the character by calling Rigidbody2D.AddForce() with ForceMode2D.Force. The force is smoothly adjusted over time with a simple PID Controller.
* **Rigidbody AddImpulse** moves the character by calling Rigidbody2D.AddForce() with ForceMode2D.Impulse. The force is smoothly adjusted over time with a simple PID Controller.

**Interpolation (Lane setting)**

Method FixedUpdate enables you to assign an interpolation type to the character sprite.

* **None** disabled interpolation. The sprite will always be at the rigidbody position.
* **Rigidbody interpolate** uses the standard Unity interpolation setting of Rigidbody2D.
* **Rigidbody extrapolate** uses the standard Unity extrapolation setting of Rigidbody2D.
* **Custom bad** uses a simple self made interpolation, which lerps the position of the sprite from the previous rigidbody position to the last calculated rigidbody position. The last calculated rigidbody position is incorrect, because it is determined in FixedUpdate itself, before the "internal physic update" of Unity.
* **Custom good** uses a simple self made interpolation, which lerps the position of the sprite from the previous rigidbody position to the last calculated rigidbody position. The last calculated rigidbody position is retrieved correctly after the "internal physic update" of Unity. The trick is to get the final rigidbody position in a Coroutine, which yields WaitForFixedUpdate().