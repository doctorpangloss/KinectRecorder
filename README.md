KinectRecorder
==============

Record Kinect particle data into upscale, high-resolution, full-color and full-motion sequences you can use directly in Autodesk Maya.

Examples
========

Early, low resolution test work can be watched [here](https://vimeo.com/31375961).

Run
===

 1. `git clone http://github.com/doctorpangloss/MayaCacheIO.git`
 1. `git clone http://github.com/doctorpangloss/KinectRecorder.git`
 2. `cd KinectRecorder`
 3. Download and extract http://downloads.sourceforge.net/project/emgucv/emgucv/2.4.2/libemgucv-windows-x64-gpu-2.4.2.1777.zip to the root directory.
 4. Download and install the [Kinect SDK v1.8](https://www.microsoft.com/en-us/download/details.aspx?id=40278)
 5. Open the Visual Studio project.
 6. Add MayaCacheIO as your missing reference.
 7. Compile & run.

Howto Use
=========

 2. Create a `Maya` scene with a `particleShape,` and rename it to something like `S1_ParticleShape_T1`.
 3. Change your time to 12 FPS for SVGA resolution Kinect video (higher quality, poorer motion), or 30 FPS for VGA or lower Kinect video.
 2. Create a new `nParticles` shape, and then delete it along with its `nucleus` node.
 3. Connect the newly created `npParticleCloud` or `npBlinnShader` to the `S1_ParticleShape` node to shade it. You achieved hardware color.
 4. Scale the transform node, `S1_Particle`, to `[810.12,810.12,810.12]`. This converts Kinect units to Maya units (even though it should theoretically just be 100).
 5. Cache the particles. You should now have a `(project root)/data/S1_ParticleShape` directory.
 2. Plugin your Kinect.
 2. Run the application.
 3. Choose the attached Kinect from the dropdown (multiple simultaneous recordings are supported).
 4. Choose your quality.
 5. Hit `On` to start the engine.
 6. Change your recording directory to the place where you want to store Kinect clips. Currently, this uses an internal format, instead of the new Microsoft Kinect clip format.
 7. The Scene number corresponds to the number after S. The Take number corresponds to the number after T. So add a note, "ParticleShape." This lets you record multiple actors, scenes, takes, etc. easily.
 8. Hit `Record` and `Stop`.
 9. Then switch to the `Process` tab.
 10. Check `Leg` for the legacy PDC format. `nCache` is not fully supported yet.
 11. Check `Act` if you only want the actors. Automatic green screening! Otherwise, leave it blank to process the whole scene.
 11. Click `Open Clip` and select the directory with all the clips you want to process.
 12. Click `nCache...` and choose the particle data folder, `(project root)/data/S1ParticleShape`.
 13. Click process, and wait.

How do you get such high-resolution particles?
==============================================

I transform the depth frame into the Skeleton space basis (x_depth, y_depth, depth) => (x',y',z'). I store (x', y', z') skeleton points into a floating-point RGB image. Since (x_depth, y_depth) corresponds to (x_color, y_color), I scale up the depth frame nearest neighbor to match the color frame's resolution.

How can I use this in my own work?
==================================

This software is GPLv3 licensed. You retain full rights to the content you produce.
