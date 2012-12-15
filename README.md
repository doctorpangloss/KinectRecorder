KinectRecorder
==============

Record Kinect particle data into upscale, high-resolution, full-color and full-motion sequences you can use directly in Autodesk Maya.

Examples
========

Early, low resolution test work can be watched [here](https://vimeo.com/31375961). Check out [my thesis](https://vimeo.com/42359967) for its use in a full narrative film. The objective is to do something like the dream sequences in *Prometheus*, except without the use of Trapcode Particular.

Run
===

 1. `git clone http://github.com/doctorpangloss/MayaCacheIO.git`
 1. `git clone http://github.com/doctorpangloss/KinectRecorder.git`
 2. `cd KinectRecorder`
 3. Download and extract http://downloads.sourceforge.net/project/emgucv/emgucv/2.4.2/libemgucv-windows-x64-gpu-2.4.2.1777.zip to the root directory.
 4. Download and install the [latest Kinect SDK](http://www.microsoft.com/en-us/kinectforwindows/develop/overview.aspx)
 5. Open the Visual Studio project.
 6. Add MayaCacheIO as your missing reference.
 7. Compile & run.

How do you get such high-resolution particles?
==============================================

I transform the depth frame into the Skeleton space basis (x_depth, y_depth, depth) => (x',y',z'). I store (x', y', z') skeleton points into a floating-point RGB image. Since (x_depth, y_depth) corresponds to (x_color, y_color), I scale up the depth frame nearest neighbor to match the color frame's resolution. Ain't that clever? I'm really dating myself here.

How can I use this in my own work?
==================================

This software is GPLv3 licensed with the following amendment: Any products created by this software or derivatives of this software, such as, but not limited to, the converted particle sequences, cannot be used in commercial work without my express prior written permission. Non-commercial work that uses this tool or its derivatives in this way must be Creative Commons NC-BY-SA 3.0 licensed.

In other words, if you use this to make a sequence for a music video, television spot, or film for which you are compensated with money or favors, please get my permission to use the software first. Other purposes, like evaluation, research, creating your own, non-commercial work, just requires that the tool be attributed to me.
