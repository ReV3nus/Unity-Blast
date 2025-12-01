# Unity Blast

## Intro

[`Blast`](https://nvidia-omniverse.github.io/PhysX/blast/index.html) is a NVIDIA destruction library, which can efficiently calculate the effects of object breaking, fragmentation, fracturing, and real-time physics simulation.

> Blast is a NVIDIA Omniverse destruction library. It consists of three layers: the low-level (NvBlast), a high-level “toolkit” wrapper (NvBlastTk), and extensions (prefixed with NvBlastExt). This layered API is designed to allow short ramp-up time for first usage (through the Ext and Tk APIs) while also allowing for customization and optimization by experienced users through the low-level API.

In recent years, NVIDIA has integrated `Blast` into `Omniverse` as part of the PhysX physics simulation project. Here's the [Github Page](https://github.com/NVIDIA-Omniverse/PhysX). Also included within Omniverse are the fluid simulation project `Flow` and the physics simulation project `PhysX`.

However, Blast is a `C++` project. This makes it highly compatible with C++ projects like Houdini and Unreal Engine, but less so with C# projects like Unity. While Blast has C-style APIs consisting of stateless functions at higher levels, some lower-level methods lack exposed C-style APIs for various reasons. 

Although there are many excellent projects available, such as [`unity-fracture`](https://github.com/ElasticSea/unity-fracture), these projects have not been updated for a long time, and the Blast versions they rely on are somewhat outdated.

Therefore, in this project, I will be working on porting the Blast method from the latest version of `PhysX` and `Blast`*(Blast SDK version 5.0.6)*, including exposing the C-style interface and implementing the C# wrapper function, so that Blast's efficient and excellent methods for simulating breakage and fragmentation can be used in Unity.


## Features

## Usage

## Examples

Thanks to repository (unity-fracture)[https://github.com/ElasticSea/unity-fracture] and the (forum thread)[https://discussions.unity.com/t/nvidia-blast/665733], I have implemented their methods using the newest Blast.

Here is the method *voronoiFracturing*'s effect in Unity.

<img width="1111" height="715" alt="image" src="https://github.com/user-attachments/assets/b7a1a1c9-0973-422f-b8ca-a27deae0e3da" />