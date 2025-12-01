using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.UIElements;

[StructLayout(LayoutKind.Sequential)]
public struct NvVertex
{
    public Vector3 p;
    public Vector3 n;
    public Vector2 uv0;
};

[StructLayout(LayoutKind.Sequential)]
public struct NoiseConfiguration
{
    public float amplitude;//0 - disabled
    public float frequency;//:1
    public int octaveNumber;//:1
    public int surfaceResolution;//:1
};

[StructLayout(LayoutKind.Sequential)]
public struct SlicingConfiguration
{
    public Vector3Int slices;
    public float offset_variations;//0-1:0
    public float angle_variations;//0-1:0
    public NoiseConfiguration noise;
};

public struct NvcTransform
{
    public Quaternion q;
    public Vector3 p;
};


[StructLayout(LayoutKind.Sequential)]
public struct CutoutConfiguration
{
    /**
        Set of grouped convex loop patterns for cutout in normal direction.
        Not required for PLANE_ONLY mode
    */
    public IntPtr cutoutSet;// = nullptr;

    /**
        Transform for initial pattern position and orientation.
        By default 2d pattern lies in XY plane (Y is up) the center of pattern is (0, 0)
    */
    public NvcTransform transform;// = { { 0, 0, 0, 1 }, { 0, 0, 0 } };

    /**
        Scale for pattern. Unscaled pattern has size (1, 1).
        For negative scale pattern will be placed at the center of chunk and scaled with max distance between points of
       its AABB
    */
    public Vector2 scale;// = { -1, -1 };

    /**
        Conic aperture in degree, for cylindric cutout set it to 0.
    */
    public float aperture;// = 0.f;

    /**
        If relative transform is set - position will be displacement vector from chunk's center. Otherwise from global
       origin.
    */
    public bool isRelativeTransform;// = true;

    /**
    Add generatad faces to the same smoothing group as original face without noise
    */
    public bool useSmoothing;// = false;

    /**
        Noise parameters for cutout surface, see NoiseConfiguration.
    */
    public NoiseConfiguration noise;
};


[StructLayout(LayoutKind.Sequential)]
public struct BlastConfiguration
{
    public Vector3      blastPoint;
    public Vector3      blastNormal;

    public uint         innerSites;
    public float        innerRadius;
    public float        innerBias;

    public uint         transitionSites;
    public float        transitionRadius;
    public float        transitionBias;

    public uint         outersites;

    public float        radialRadius;
    public uint         radialRadSteps;
    public uint         radialAngSteps;
    public float        radialAngleOffset;
    public float        radialNormalOffset;
    public float        radialVariability;
};

public static class NvBlastUnityExtTypes
{
    public static CutoutConfiguration GetDefaultCutoutConf()
    {
        //Default value initialization
        return new CutoutConfiguration
        {
            cutoutSet = IntPtr.Zero,
            transform = new NvcTransform
            {
                q = Quaternion.identity,
                p = Vector3.zero
            },
            scale = new Vector2(-1, -1),
            aperture = 0f,
            isRelativeTransform = true,
            useSmoothing = false,
            noise = new NoiseConfiguration
            {
                amplitude = 0f,
                frequency = 1f,
                octaveNumber = 1,
                surfaceResolution = 1
            }
        };
    }
}


public class NvMesh : DisposablePtr
{
    public const string DLL_NAME = "NvBlastUnityExt" + NvBlastWrapper.DLL_POSTFIX + NvBlastWrapper.DLL_PLATFORM;
    public const string AUTHORING_DLL_NAME = "NvBlastExtAuthoring" + NvBlastWrapper.DLL_POSTFIX + NvBlastWrapper.DLL_PLATFORM;


    [DllImport(DLL_NAME)]
    private static extern void NvBlastUnityExtMeshRelease(IntPtr mesh);

    [DllImport(DLL_NAME)]
    private static extern void NvBlastUnityExtMeshGetVertices(IntPtr mesh, [In, Out] Vector3[] arr);

    [DllImport(DLL_NAME)]
    private static extern void NvBlastUnityExtMeshGetNormals(IntPtr mesh, [In, Out] Vector3[] arr);

    [DllImport(DLL_NAME)]
    private static extern void NvBlastUnityExtMeshGetIndexes(IntPtr mesh, [In, Out] int[] arr);

    [DllImport(DLL_NAME)]
    private static extern void NvBlastUnityExtMeshGetUVs(IntPtr mesh, [In, Out] Vector2[] arr);

    [DllImport(DLL_NAME)]
    private static extern int NvBlastUnityExtMeshGetVerticeCount(IntPtr mesh);

    [DllImport(DLL_NAME)]
    private static extern int NvBlastUnityExtMeshGetIndexCount(IntPtr mesh);

    [DllImport(AUTHORING_DLL_NAME)]
    private static extern IntPtr NvBlastExtAuthoringCreateMesh(Vector3[] positions, Vector3[] normals, Vector2[] uv, Int32 verticesCount, Int32[] indices, Int32 indicesCount);

    public NvMesh(IntPtr mesh)
    {
        Initialize(mesh);
    }

    public NvMesh(Vector3[] positions, Vector3[] normals, Vector2[] uv, Int32 verticesCount, Int32[] indices, Int32 indicesCount)
    {
        Initialize(NvBlastExtAuthoringCreateMesh(positions, normals, uv, verticesCount, indices, indicesCount));
    }

    public Vector3[] getVertices()
    {
        Vector3[] v = new Vector3[getVerticesCount()];
        NvBlastUnityExtMeshGetVertices(this.ptr, v);
        return v;
    }
    public Vector3[] getNormals()
    {
        Vector3[] v = new Vector3[getVerticesCount()];
        NvBlastUnityExtMeshGetNormals(this.ptr, v);
        return v;
    }
    public Vector2[] getUVs()
    {
        Vector2[] v = new Vector2[getVerticesCount()];
        NvBlastUnityExtMeshGetUVs(this.ptr, v);
        return v;
    }
    public int[] getIndexes()
    {
        int[] v = new int[getIndexesCount()];
        NvBlastUnityExtMeshGetIndexes(this.ptr, v);
        return v;
    }

    public int getVerticesCount()
    {
        return NvBlastUnityExtMeshGetVerticeCount(this.ptr);
    }

    public int getIndexesCount()
    {
        return NvBlastUnityExtMeshGetIndexCount(this.ptr);
    }

    protected override void Release()
    {
        NvBlastUnityExtMeshRelease(this.ptr);
    }

    //Unity Helper Functions
    public Mesh toUnityMesh()
    {
        Mesh m = new Mesh();
        m.vertices = getVertices();
        m.normals = getNormals();
        m.uv = getUVs();
        m.SetIndices(getIndexes(), MeshTopology.Triangles, 0, true);
        return m;
    }
}

public class NvMeshCleaner : DisposablePtr
{
    public const string DLL_NAME = "NvBlastUnityExt" + NvBlastWrapper.DLL_POSTFIX + NvBlastWrapper.DLL_PLATFORM;
    public const string AUTHORING_DLL_NAME = "NvBlastExtAuthoring" + NvBlastWrapper.DLL_POSTFIX + NvBlastWrapper.DLL_PLATFORM;

    [DllImport(DLL_NAME)]
    private static extern void NvBlastUnityExtMeshCleanerRelease(IntPtr cleaner);

    [DllImport(DLL_NAME)]
    private static extern IntPtr NvBlastUnityExtMeshCleanerCleanMesh(IntPtr cleaner, IntPtr mesh);

    [DllImport(AUTHORING_DLL_NAME)]
    private static extern IntPtr NvBlastExtAuthoringCreateMeshCleaner();

    public NvMeshCleaner()
    {
        Initialize(NvBlastExtAuthoringCreateMeshCleaner());
    }

    public NvMesh cleanMesh(NvMesh mesh)
    {
        return new NvMesh(NvBlastUnityExtMeshCleanerCleanMesh(this.ptr, mesh.ptr));
    }

    protected override void Release()
    {
        NvBlastUnityExtMeshCleanerRelease(this.ptr);
    }
}

public class NvVoronoiSitesGenerator : DisposablePtr
{
    public const string DLL_NAME = "NvBlastUnityExt" + NvBlastWrapper.DLL_POSTFIX + NvBlastWrapper.DLL_PLATFORM;
    public const string AUTHORING_DLL_NAME = "NvBlastExtAuthoring" + NvBlastWrapper.DLL_POSTFIX + NvBlastWrapper.DLL_PLATFORM;

    [DllImport(DLL_NAME)]
    private static extern void NvBlastUnityExtVSGRelease(IntPtr site);

    [DllImport(DLL_NAME)]
    private static extern IntPtr NvBlastUnityExtVSGCreate(IntPtr mesh);

    [DllImport(DLL_NAME)]
    private static extern IntPtr NvBlastUnityExtVSGUniformlyGenerateSitesInMesh(IntPtr vsg, int count);

    [DllImport(DLL_NAME)]
    private static extern IntPtr NvBlastUnityExtVSGAddSite(IntPtr vsg, [In] Vector3 site);

    [DllImport(DLL_NAME)]
    private static extern bool NvBlastUnityExtVSGClusteredSitesGeneration(IntPtr vsg, int numberOfClusters, int sitesPerCluster, float clusterRadius);

    [DllImport(DLL_NAME)]
    private static extern int NvBlastUnityExtVSGGetSitesCount(IntPtr vsg);

    [DllImport(DLL_NAME)]
    private static extern void NvBlastUnityExtVSGGetSites(IntPtr vsg, [In, Out] Vector3[] arr);

    [DllImport(DLL_NAME)]
    private static extern void NvBlastUnityExtVSGSetMesh(IntPtr vsg, IntPtr mesh);

    [DllImport(DLL_NAME)]
    private static extern void NvBlastUnityExtVSGSetStencil(IntPtr vsg, IntPtr stencil);

    [DllImport(DLL_NAME)]
    private static extern void NvBlastUnityExtVSGClearStencil(IntPtr vsg);

    [DllImport(DLL_NAME)]
    private static extern void NvBlastUnityExtVSGGenerateInSphere(IntPtr vsg, int count, float radius, [In] Vector3 center);

    [DllImport(DLL_NAME)]
    private static extern void NvBlastUnityExtVSGDeleteInSphere(IntPtr vsg, float radius, [In] Vector3 center, float prob);

    [DllImport(DLL_NAME)]
    private static extern void NvBlastUnityExtVSGRadialPattern(IntPtr vsg, [In] Vector3 center, [In] Vector3 normal, float radius, int angularSteps, int radialSteps, float angleOffset, float variability);

    [DllImport(DLL_NAME)]
    private static extern void NvBlastUnityExtVSGBlastPattern(IntPtr vsg, [In] BlastConfiguration conf);

    [DllImport(DLL_NAME)]
    private static extern int NvBlastUnityExtVSGGetNeighbors(IntPtr vsg, [In, Out] Vector2[] arr, int bufferSize);


    public NvVoronoiSitesGenerator(NvMesh mesh)
    {
        Initialize(NvBlastUnityExtVSGCreate(mesh.ptr));
    }

    public void uniformlyGenerateSitesInMesh(int count)
    {
        NvBlastUnityExtVSGUniformlyGenerateSitesInMesh(this.ptr, count);
    }

    public void addSite(Vector3 site)
    {
        NvBlastUnityExtVSGAddSite(this.ptr, site);
    }

    public void clusteredSitesGeneration(int numberOfClusters, int sitesPerCluster, float clusterRadius)
    {
        NvBlastUnityExtVSGClusteredSitesGeneration(this.ptr, numberOfClusters, sitesPerCluster, clusterRadius);
    }

    public Vector3[] getSites()
    {
        Vector3[] v = new Vector3[getSitesCount()];
        NvBlastUnityExtVSGGetSites(this.ptr, v);
        return v;
    }

    public int getSitesCount()
    {
        return NvBlastUnityExtVSGGetSitesCount(this.ptr);
    }

    public void setMesh(NvMesh mesh)
    {
        NvBlastUnityExtVSGSetMesh(this.ptr, mesh.ptr);
    }
    public void setUtensil(NvMesh utensil)
    {
        NvBlastUnityExtVSGSetStencil(this.ptr, utensil.ptr);
    }
    public void clearUtensil()
    {
        NvBlastUnityExtVSGClearStencil(this.ptr);
    }
    public void generateInSphere(int count, float radius, Vector3 center)
    {
        NvBlastUnityExtVSGGenerateInSphere(this.ptr, count, radius, center);
    }
    public void deleteInSphere(float radius, Vector3 center, float prob)
    {
        NvBlastUnityExtVSGDeleteInSphere(this.ptr, radius, center, prob);
    }
    public void radialPattern(Vector3 center, Vector3 normal, float radius, int angularSteps, int radialSteps, float angleOffset, float variability)
    {
        NvBlastUnityExtVSGRadialPattern(this.ptr, center, normal, radius, angularSteps, radialSteps, angleOffset, variability);
    }

    public void blastPattern(BlastConfiguration conf)
    {
        NvBlastUnityExtVSGBlastPattern(this.ptr, conf);
    }
    public int getNeighbors(Vector2[] buffer, int bufferSize)
    {
        return NvBlastUnityExtVSGGetNeighbors(this.ptr, buffer, bufferSize);
    }



    protected override void Release()
    {
        NvBlastUnityExtVSGRelease(this.ptr);
    }

    //Unity Specific
    public void boneSiteGeneration(SkinnedMeshRenderer smr)
    {
        if (smr == null)
        {
            Debug.Log("No Skinned Mesh Renderer");
            return;
        }

        Animator anim = smr.transform.root.GetComponent<Animator>();
        if (anim == null)
        {
            Debug.Log("Missing Animator");
            return;
        }

        if (anim.GetBoneTransform(HumanBodyBones.Head)) addSite(anim.GetBoneTransform(HumanBodyBones.Head).position);
        if (anim.GetBoneTransform(HumanBodyBones.Neck)) addSite(anim.GetBoneTransform(HumanBodyBones.Neck).position);

        //if (anim.GetBoneTransform(HumanBodyBones.LeftShoulder)) addSite(anim.GetBoneTransform(HumanBodyBones.LeftShoulder).position);
        //if (anim.GetBoneTransform(HumanBodyBones.RightShoulder)) addSite(anim.GetBoneTransform(HumanBodyBones.RightShoulder).position);

        if (anim.GetBoneTransform(HumanBodyBones.LeftUpperArm)) addSite(anim.GetBoneTransform(HumanBodyBones.LeftUpperArm).position);
        if (anim.GetBoneTransform(HumanBodyBones.RightUpperArm)) addSite(anim.GetBoneTransform(HumanBodyBones.RightUpperArm).position);

        if (anim.GetBoneTransform(HumanBodyBones.LeftLowerArm)) addSite(anim.GetBoneTransform(HumanBodyBones.LeftLowerArm).position);
        if (anim.GetBoneTransform(HumanBodyBones.RightLowerArm)) addSite(anim.GetBoneTransform(HumanBodyBones.RightLowerArm).position);

        if (anim.GetBoneTransform(HumanBodyBones.LeftHand)) addSite(anim.GetBoneTransform(HumanBodyBones.LeftHand).position);
        if (anim.GetBoneTransform(HumanBodyBones.RightHand)) addSite(anim.GetBoneTransform(HumanBodyBones.RightHand).position);

        if (anim.GetBoneTransform(HumanBodyBones.Chest)) addSite(anim.GetBoneTransform(HumanBodyBones.Chest).position);
        if (anim.GetBoneTransform(HumanBodyBones.Spine)) addSite(anim.GetBoneTransform(HumanBodyBones.Spine).position);
        if (anim.GetBoneTransform(HumanBodyBones.Hips)) addSite(anim.GetBoneTransform(HumanBodyBones.Hips).position);

        if (anim.GetBoneTransform(HumanBodyBones.LeftUpperLeg)) addSite(anim.GetBoneTransform(HumanBodyBones.LeftUpperLeg).position);
        if (anim.GetBoneTransform(HumanBodyBones.RightUpperLeg)) addSite(anim.GetBoneTransform(HumanBodyBones.RightUpperLeg).position);

        if (anim.GetBoneTransform(HumanBodyBones.LeftLowerLeg)) addSite(anim.GetBoneTransform(HumanBodyBones.LeftLowerLeg).position);
        if (anim.GetBoneTransform(HumanBodyBones.RightLowerLeg)) addSite(anim.GetBoneTransform(HumanBodyBones.RightLowerLeg).position);

        if (anim.GetBoneTransform(HumanBodyBones.LeftFoot)) addSite(anim.GetBoneTransform(HumanBodyBones.LeftFoot).position);
        if (anim.GetBoneTransform(HumanBodyBones.RightFoot)) addSite(anim.GetBoneTransform(HumanBodyBones.RightFoot).position);

        //if (anim.GetBoneTransform(HumanBodyBones.LeftEye)) addSite(anim.GetBoneTransform(HumanBodyBones.LeftEye).position);
        //if (anim.GetBoneTransform(HumanBodyBones.RightEye)) addSite(anim.GetBoneTransform(HumanBodyBones.RightEye).position);
    }
}

public class NvFractureTool : DisposablePtr
{
    public const string DLL_NAME = "NvBlastUnityExt" + NvBlastWrapper.DLL_POSTFIX + NvBlastWrapper.DLL_PLATFORM;
    public const string AUTHORING_DLL_NAME = "NvBlastExtAuthoring" + NvBlastWrapper.DLL_POSTFIX + NvBlastWrapper.DLL_PLATFORM;

    [DllImport(DLL_NAME)]
    private static extern void NvBlastUnityExtFractureToolRelease(IntPtr tool);

    [DllImport(AUTHORING_DLL_NAME)]
    private static extern IntPtr NvBlastExtAuthoringCreateFractureTool();

    [DllImport(DLL_NAME)]
    private static extern void NvBlastUnityExtFractureToolSetSourceMesh(IntPtr tool, IntPtr mesh);

    [DllImport(DLL_NAME)]
    private static extern void NvBlastUnityExtFractureToolSetRemoveIslands(IntPtr tool, bool remove);

    [DllImport(DLL_NAME)]
    private static extern bool NvBlastUnityExtFractureToolVoronoiFracturing(IntPtr tool, int chunkId, IntPtr vsg);

    [DllImport(DLL_NAME)]
    private static extern bool NvBlastUnityExtFractureToolSlicing(IntPtr tool, int chunkId, [In] SlicingConfiguration conf, bool replaceChunk);

    [DllImport(DLL_NAME)]
    private static extern void NvBlastUnityExtFractureToolFinalizeFracturing(IntPtr tool);

    [DllImport(DLL_NAME)]
    private static extern int NvBlastUnityExtFractureToolGetChunkCount(IntPtr tool);

    [DllImport(DLL_NAME)]
    private static extern IntPtr NvBlastUnityExtFractureToolGetChunkMesh(IntPtr tool, int chunkId, bool inside);

    [DllImport(DLL_NAME)]
    private static extern bool NvBlastUnityExtFractureToolSetSourceMeshes(IntPtr tool, IntPtr meshes, int meshesSize);

    [DllImport(DLL_NAME)]
    private static extern int NvBlastUnityExtFractureToolSetChunkMesh(IntPtr tool, IntPtr mesh, int parentId);

    [DllImport(DLL_NAME)]
    private static extern IntPtr NvBlastUnityExtFractureToolCreateChunkMesh(IntPtr tool, int chunkInfoIndex);


    [DllImport(DLL_NAME)]
    private static extern int NvBlastUnityExtFractureToolCut(IntPtr tool, int chunkId, [In] Vector3 normal, [In] Vector3 position, [In] NoiseConfiguration noise, bool replaceChunk);


    [DllImport(DLL_NAME)]
    private static extern int NvBlastUnityExtFractureToolCutout(IntPtr tool, int chunkId, [In] CutoutConfiguration conf, bool replaceChunk);


    [DllImport(DLL_NAME)]
    private static extern int NvBlastUnityExtFractureToolGetChunkInfoIndex(IntPtr tool, int chunkId);


    [DllImport(DLL_NAME)]
    private static extern int NvBlastUnityExtFractureToolGetChunkId(IntPtr tool, int chunkInfoIndex);


    [DllImport(DLL_NAME)]
    private static extern int NvBlastUnityExtFractureToolGetChunkDepth(IntPtr tool, int chunkId);


    [DllImport(DLL_NAME)]
    private static extern int NvBlastUnityExtFractureToolIslandDetectionAndRemoving(IntPtr tool, int chunkId);

    [DllImport(DLL_NAME)]
    private static extern bool NvBlastUnityExtFractureToolIsMeshContainOpenEdges(IntPtr tool, IntPtr mesh);

    [DllImport(DLL_NAME)]
    private static extern bool NvBlastUnityExtFractureToolDeleteChunkSubhierarchy(IntPtr tool, int chunkId, bool deleteRoot);


    [DllImport(DLL_NAME)]
    private static extern bool NvBlastUnityExtFractureToolSetApproximateBonding(IntPtr tool, int chunkId, bool useApproximateBonding);

    [DllImport(DLL_NAME)] 
    private static extern void NvBlastUnityExtFractureToolFitAllUvToRect(IntPtr tool, float side);

    [DllImport(DLL_NAME)] 
    private static extern void NvBlastUnityExtFractureToolFitUvToRect(IntPtr tool, float side, int chunkId);

    [DllImport(DLL_NAME)]
    private static extern int NvBlastUnityExtFractureToolGetCrackEdgesCount(IntPtr tool);

    [DllImport(DLL_NAME)]
    private static extern void NvBlastUnityExtFractureToolGetCrackEdges(IntPtr tool, [In, Out] NvVertex[] CrackEdgeVertices);


    public NvFractureTool()
    {
        Initialize(NvBlastExtAuthoringCreateFractureTool());
    }

    public void setSourceMesh(NvMesh mesh)
    {
        NvBlastUnityExtFractureToolSetSourceMesh(this.ptr, mesh.ptr);
    }

    public void setRemoveIslands(bool remove)
    {
        NvBlastUnityExtFractureToolSetRemoveIslands(this.ptr, remove);
    }

    public bool voronoiFracturing(int chunkId, NvVoronoiSitesGenerator vsg)
    {
        return NvBlastUnityExtFractureToolVoronoiFracturing(this.ptr, chunkId, vsg.ptr);
    }

    public bool slicing(int chunkId, SlicingConfiguration conf, bool replaceChunk)
    {
        return NvBlastUnityExtFractureToolSlicing(this.ptr, chunkId, conf, replaceChunk);
    }

    public void finalizeFracturing()
    {
        NvBlastUnityExtFractureToolFinalizeFracturing(this.ptr);
    }

    public int getChunkCount()
    {
        return NvBlastUnityExtFractureToolGetChunkCount(this.ptr);
    }

    public NvMesh getChunkMesh(int chunkId, bool inside)
    {
        return new NvMesh(NvBlastUnityExtFractureToolGetChunkMesh(this.ptr, chunkId, inside));
    }

    //public bool setSourceMeshes(NvMesh[] meshes)
    //{
    //    return NvBlastUnityExtFractureToolSetSourceMeshes(this.ptr, meshes, meshes.Length); // TODO: create DisposablePtr for NvMesh[]
    //}

    public int setChunkMesh(NvMesh mesh, int parentId)
    {
        return NvBlastUnityExtFractureToolSetChunkMesh(this.ptr, mesh.ptr, parentId);
    }

    public NvMesh createChunkMesh(int chunkInfoIndex)
    {
        return new NvMesh(NvBlastUnityExtFractureToolCreateChunkMesh(this.ptr, chunkInfoIndex));
    }

    public int Cut(int chunkId, Vector3 normal, Vector3 position, NoiseConfiguration noise, bool replaceChunk)
    {
        return NvBlastUnityExtFractureToolCut(this.ptr, chunkId, normal, position, noise, replaceChunk);
    }
    public int Cutout(int chunkId, CutoutConfiguration conf, bool replaceChunk)
    {
        return NvBlastUnityExtFractureToolCutout(this.ptr, chunkId, conf, replaceChunk);
    }
    public int getChunkInfoIndex(int chunkId)
    {
        return NvBlastUnityExtFractureToolGetChunkInfoIndex(this.ptr, chunkId);
    }
    public int getChunkId(int chunkInfoIndex)
    {
        return NvBlastUnityExtFractureToolGetChunkId(this.ptr, chunkInfoIndex);
    }
    public int getChunkDepth(int chunkId)
    {
        return NvBlastUnityExtFractureToolGetChunkDepth(this.ptr, chunkId);
    }

    public int islandDetectionAndRemoving(int chunkId)
    {
        return NvBlastUnityExtFractureToolIslandDetectionAndRemoving(this.ptr, chunkId);
    }

    public bool isMeshContainOpenEdges(NvMesh mesh)
    {
        return NvBlastUnityExtFractureToolIsMeshContainOpenEdges(this.ptr, mesh.ptr);
    }
    public bool deleteChunkSubhierarchy(int chunkId, bool deleteRoot)
    {
        return NvBlastUnityExtFractureToolDeleteChunkSubhierarchy(this.ptr, chunkId, deleteRoot);
    }
    public bool setApproximateBonding(int chunkId, bool useApproximateBonding)
    {
        return NvBlastUnityExtFractureToolSetApproximateBonding(this.ptr, chunkId, useApproximateBonding);
    }
    public void fitUvToRect(float side, int chunkId)
    {
        NvBlastUnityExtFractureToolFitUvToRect(this.ptr, side, chunkId);
    }
    public void fitAllUvToRect(float side)
    {
        NvBlastUnityExtFractureToolFitAllUvToRect(this.ptr, side);
    }

    public int getCracksCount()
    {
        return NvBlastUnityExtFractureToolGetCrackEdgesCount(this.ptr);
    }

    public void getCrackVertices(NvVertex[] vertices)
    {
        NvBlastUnityExtFractureToolGetCrackEdges(this.ptr, vertices);
    }


    protected override void Release()
    {
        NvBlastUnityExtFractureToolRelease(this.ptr);
    }
}


public class NvBlastUnityExtWrapper
{
    public const string DLL_NAME = "NvBlastUnityExt" + NvBlastWrapper.DLL_POSTFIX + NvBlastWrapper.DLL_PLATFORM;


    [DllImport(DLL_NAME)]
    public static extern void setSeed(int seed);

    [DllImport(DLL_NAME)]
    public static extern float _Debug_CheckCutoutConf([In] CutoutConfiguration conf);


}
