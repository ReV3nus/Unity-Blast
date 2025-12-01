/*
 * © 2025 ReV3nus
 *
 * This code is primarily intended for **educational and research purposes**.
 * You are welcome to view, study, and modify it for personal learning or academic projects.
 *
 * **Recommended Restrictions:**
 * - Commercial use or for-profit distribution is not encouraged without explicit permission.
 * - Do not sell or redistribute for profit.
 *
 * While this code is provided “as-is” without any warranty, we appreciate acknowledgment if used in academic or learning projects.
 *
 * For commercial use or licensing inquiries, please contact the author.
 */

//! @file
//!
//! @brief Defines the API for the NvBlastUnityExt blast sdk extension

#ifndef NVBLASTUNITY_H
#define NVBLASTUNITY_H

#include "NvBlastExtAuthoringTypes.h"

namespace Nv
{
namespace Blast
{
class Mesh;
class VoronoiSitesGenerator;
class CutoutSet;
class RandomGeneratorBase;
class FractureTool;
class ConvexMeshBuilder;
class BlastBondGenerator;
class MeshCleaner;
class PatternGenerator;
class SpatialGrid;
class SpatialAccelerator;
class BooleanTool;
}  // namespace Blast
}  // namespace Nv


/*
    Util Functions
*/

NV_C_API void setSeed(int seed);
NV_C_API float _Debug_CheckCutoutConf(Nv::Blast::CutoutConfiguration conf);


/*
    Mesh Functions
*/

NV_C_API void NvBlastUnityExtMeshRelease(Nv::Blast::Mesh* mesh);

NV_C_API bool NvBlastUnityExtMeshIsValid(Nv::Blast::Mesh* mesh);

NV_C_API void NvBlastUnityExtMeshGetVertices(Nv::Blast::Mesh* mesh, void* data);

NV_C_API void NvBlastUnityExtMeshGetNormals(Nv::Blast::Mesh* mesh, void* data);

NV_C_API void NvBlastUnityExtMeshGetIndexes(Nv::Blast::Mesh* mesh, void* data);

NV_C_API void NvBlastUnityExtMeshGetUVs(Nv::Blast::Mesh* mesh, void* data);

NV_C_API uint32_t NvBlastUnityExtMeshGetVerticeCount(Nv::Blast::Mesh* mesh);

NV_C_API uint32_t NvBlastUnityExtMeshGetIndexCount(Nv::Blast::Mesh* mesh);


/*
    Mesh Cleaner Tool Functions
*/

NV_C_API void NvBlastUnityExtMeshCleanerRelease(Nv::Blast::MeshCleaner* cleaner);

NV_C_API Nv::Blast::Mesh* NvBlastUnityExtMeshCleanerCleanMesh(Nv::Blast::MeshCleaner* cleaner, Nv::Blast::Mesh* mesh);


/*
    VoronoiSitesGenerator Functions
*/

NV_C_API void NvBlastUnityExtVSGRelease(Nv::Blast::VoronoiSitesGenerator* tool);

NV_C_API Nv::Blast::VoronoiSitesGenerator* NvBlastUnityExtVSGCreate(Nv::Blast::Mesh* mesh);

NV_C_API void NvBlastUnityExtVSGUniformlyGenerateSitesInMesh(Nv::Blast::VoronoiSitesGenerator* tool, int count);

NV_C_API void NvBlastUnityExtVSGClusteredSitesGeneration(Nv::Blast::VoronoiSitesGenerator* tool, uint32_t numberOfClusters, uint32_t sitesPerCluster, float clusterRadius);

NV_C_API void NvBlastUnityExtVSGAddSite(Nv::Blast::VoronoiSitesGenerator* tool, NvcVec3* sites);

NV_C_API uint32_t NvBlastUnityExtVSGGetSitesCount(Nv::Blast::VoronoiSitesGenerator* tool);

NV_C_API void NvBlastUnityExtVSGGetSites(Nv::Blast::VoronoiSitesGenerator* tool, void* arr);

NV_C_API void NvBlastUnityExtVSGSetMesh(Nv::Blast::VoronoiSitesGenerator* vsg, Nv::Blast::Mesh* mesh);

NV_C_API void NvBlastUnityExtVSGSetStencil(Nv::Blast::VoronoiSitesGenerator* vsg, Nv::Blast::Mesh* stencil);

NV_C_API void NvBlastUnityExtVSGClearStencil(Nv::Blast::VoronoiSitesGenerator* vsg);

NV_C_API void NvBlastUnityExtVSGGenerateInSphere(Nv::Blast::VoronoiSitesGenerator* vsg,
                                                 int count,
                                                 float radius,
                                                 NvcVec3 center);

NV_C_API void NvBlastUnityExtVSGDeleteInSphere(Nv::Blast::VoronoiSitesGenerator* vsg,
                                               float radius,
                                               NvcVec3 center,
                                               float prob);

NV_C_API void NvBlastUnityExtVSGRadialPattern(Nv::Blast::VoronoiSitesGenerator* vsg,
                                     NvcVec3 center,
                                     NvcVec3 normal,
                                     float radius,
                                     int angularSteps,
                                     int radialSteps,
                                     float angleOffset,
                                     float variability);

NV_C_API void NvBlastUnityExtVSGBlastPattern(Nv::Blast::VoronoiSitesGenerator* vsg, Nv::Blast::BlastConfiguration conf);


NV_C_API int32_t NvBlastUnityExtVSGGetNeighbors(Nv::Blast::VoronoiSitesGenerator* vsg, void* data, int bufferSize);


/*
    Fracture Tool Functions
*/

NV_C_API void NvBlastUnityExtFractureToolRelease(Nv::Blast::FractureTool* tool);

NV_C_API void NvBlastUnityExtFractureToolSetRemoveIslands(Nv::Blast::FractureTool* tool, bool flag);

NV_C_API bool NvBlastUnityExtFractureToolSetSourceMesh(Nv::Blast::FractureTool* tool, Nv::Blast::Mesh* mesh);

NV_C_API bool NvBlastUnityExtFractureToolVoronoiFracturing(Nv::Blast::FractureTool* tool,
                                                           uint32_t chunkId,
                                                           Nv::Blast::VoronoiSitesGenerator* vsg);

NV_C_API bool NvBlastUnityExtFractureToolSlicing(Nv::Blast::FractureTool* tool,
                                                 int chunkId,
                                                 Nv::Blast::SlicingConfiguration conf,
                                                 bool replaceChunk);

NV_C_API void NvBlastUnityExtFractureToolFinalizeFracturing(Nv::Blast::FractureTool* tool);

NV_C_API uint32_t NvBlastUnityExtFractureToolGetChunkCount(Nv::Blast::FractureTool* tool);

NV_C_API Nv::Blast::Mesh* NvBlastUnityExtFractureToolGetChunkMesh(Nv::Blast::FractureTool* tool, int chunkId, bool inside);

NV_C_API bool NvBlastUnityExtFractureToolSetSourceMeshes(Nv::Blast::FractureTool* tool,
                                                Nv::Blast::Mesh const* const* meshes,
                                                int meshesSize);

NV_C_API int32_t NvBlastUnityExtFractureToolSetChunkMesh(Nv::Blast::FractureTool* tool,
                                                         Nv::Blast::Mesh* mesh,
                                                         int parentId);

NV_C_API int32_t NvBlastUnityExtFractureToolCut(Nv::Blast::FractureTool* tool,
                                       int chunkId,
                                       NvcVec3 normal,
                                       NvcVec3 position,
                                       Nv::Blast::NoiseConfiguration noise,
                                       bool replaceChunk);

NV_C_API int32_t NvBlastUnityExtFractureToolCutout(Nv::Blast::FractureTool* tool,
                                          int chunkId,
                                          Nv::Blast::CutoutConfiguration conf,
                                          bool replaceChunk);

NV_C_API int32_t NvBlastUnityExtFractureToolGetChunkInfoIndex(Nv::Blast::FractureTool* tool, int chunkId);

NV_C_API int32_t NvBlastUnityExtFractureToolGetChunkId(Nv::Blast::FractureTool* tool, int chunkInfoIndex);

NV_C_API int32_t NvBlastUnityExtFractureToolGetChunkDepth(Nv::Blast::FractureTool* tool, int chunkId);

NV_C_API int32_t NvBlastUnityExtFractureToolIslandDetectionAndRemoving(Nv::Blast::FractureTool* tool, int chunkId);

NV_C_API bool NvBlastUnityExtFractureToolIsMeshContainOpenEdges(Nv::Blast::FractureTool* tool, Nv::Blast::Mesh* mesh);

NV_C_API bool NvBlastUnityExtFractureToolDeleteChunkSubhierarchy(Nv::Blast::FractureTool* tool,
                                                                 int chunkId,
                                                                 bool deleteRoot);

NV_C_API bool NvBlastUnityExtFractureToolSetApproximateBonding(Nv::Blast::FractureTool* tool,
                                                      int chunkId,
                                                      bool useApproximateBonding);

NV_C_API void NvBlastUnityExtFractureToolFitUvToRect(Nv::Blast::FractureTool* tool, float side, int chunkId);

NV_C_API void NvBlastUnityExtFractureToolFitAllUvToRect(Nv::Blast::FractureTool* tool, float side);


NV_C_API int NvBlastUnityExtFractureToolGetCrackEdgesCount(Nv::Blast::FractureTool* tool);

NV_C_API void NvBlastUnityExtFractureToolGetCrackEdges(Nv::Blast::FractureTool* tool, void* data);




#endif  // ifndef NVBLASTAUTHORING_H
