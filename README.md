# Unity Blast

## Intro

[`Blast`](https://nvidia-omniverse.github.io/PhysX/blast/index.html) is a NVIDIA destruction library, which can efficiently calculate the effects of object breaking, fragmentation, fracturing, and real-time physics simulation.

> Blast is a NVIDIA Omniverse destruction library. It consists of three layers: the low-level (NvBlast), a high-level “toolkit” wrapper (NvBlastTk), and extensions (prefixed with NvBlastExt). This layered API is designed to allow short ramp-up time for first usage (through the Ext and Tk APIs) while also allowing for customization and optimization by experienced users through the low-level API.

In recent years, NVIDIA has integrated `Blast` into `Omniverse` as part of the PhysX physics simulation project. Here's the [Github Page](https://github.com/NVIDIA-Omniverse/PhysX). Also included within Omniverse are the fluid simulation project `Flow` and the physics simulation project `PhysX`.

However, Blast is a `C++` project. This makes it highly compatible with C++ projects like Houdini and Unreal Engine, but less so with C# projects like Unity. While Blast has C-style APIs consisting of stateless functions at higher levels, some lower-level methods lack exposed C-style APIs for various reasons. 

Although there are many excellent projects available, such as [`unity-fracture`](https://github.com/ElasticSea/unity-fracture) and the [forum thread](https://discussions.unity.com/t/nvidia-blast/665733), these projects have not been updated for a long time, and the Blast versions they rely on are somewhat outdated.

Therefore, in this project, I will be working on porting the Blast method from the latest version of `PhysX` and `Blast`*(Blast SDK version 5.0.6)*, including exposing the C-style interface and implementing the C# wrapper function, so that Blast's efficient and excellent methods for simulating breakage and fragmentation can be used in Unity.

Theoretically, this can be used to break any object that could originally be supported by Blast [list here](https://nvidia-omniverse.github.io/PhysX/blast/docs/api/introduction.html#support-model) as long as it has readable meshes and uvs.



## Features

- Complete rewrite and extention of the wrapper scripts and interfaces to support the latest Unity and `Blast`.
- Some of the original logic has been rewritten and extended to support the acquisition of neighbor relationships and surface cracks with very low overhead.
- Created **Blast Pattern** fracturing method with configurable params.
- ...

## Usage

There is already a x64 version of libraries. You can also use following instructions to get your compiled libraries of corresponding operating system:

- Put the `\blast` folder to `\PhysX\blast` or `\blast` and replace existing files
- Open the `\blast` folder and build SDK according to [Blast Repo](https://github.com/NVIDIA-Omniverse/PhysX/tree/main/blast)
- Put all compiled dlls to your Unity project. The file strcture should be similar to the folder `\PhysXTools\Blast` in this repository.
- You can replace folder `x64` with your own operating systems, but don't forget to modify names and path in wrapper scripts.

After that, you should be able to use all of the functions in these wrapper scripts and create your own effects.

## Examples

I've implemented another Unity project which used these scripts to implement more elaborate shattering effects. You can visit it here:

[**Better Unity Fracture**](https://github.com/ReV3nus/Better-Unity-Fracture/tree/main)


Here are some pictures:

**Basic Voronoi Uniformly Fracturing:**

<img height="800" alt="image" src="https://github.com/user-attachments/assets/b7a1a1c9-0973-422f-b8ca-a27deae0e3da" />

**Acquisition of blasting patterns and crack data**

<img height="800" alt="image" src="https://github.com/user-attachments/assets/f92d1347-cfa0-47cf-b1ea-7cc263bc5c6d" />
