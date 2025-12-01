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


#include "NvBlastExtAuthoring.h"
#include "NvBlastTypes.h"
#include "NvBlastIndexFns.h"
#include "NvBlast.h"
#include "NvBlastAssert.h"
#include "NvBlastGlobals.h"
#include "NvBlastExtAssetUtils.h"
#include "NvBlastExtAuthoringPatternGeneratorImpl.h"
#include "NvBlastExtAuthoringBooleanToolImpl.h"
#include "NvBlastExtAuthoringAcceleratorImpl.h"
#include "NvBlastExtAuthoringMeshImpl.h"
#include "NvBlastExtAuthoringMeshCleanerImpl.h"
#include "NvBlastExtAuthoringFractureToolImpl.h"
#include "NvBlastExtAuthoringBondGeneratorImpl.h"
#include "NvBlastExtAuthoringCollisionBuilderImpl.h"
#include "NvBlastExtAuthoringCutoutImpl.h"
#include "NvBlastExtAuthoringInternalCommon.h"
#include "NvBlastNvSharedHelpers.h"
#include "NvBlastUnityExtension.h"

#include <algorithm>
#include <memory>
#include <random>
#include <vector>
#include <cstdint>

using namespace Nv::Blast;
using namespace nvidia;


/*
    Util Functions
*/


class RandomGeneratorForUnity : public RandomGeneratorBase
{
public:
    RandomGeneratorForUnity(int32_t seed = 114514)
    {
        engine.seed(seed);
    }

    float getRandomValue() override
    {
        // Return random float in [0,1]
        return dist(engine);
    }

    void seed(int32_t s) override
    {
        engine.seed(s);
    }

private:
    std::mt19937 engine;
    std::uniform_real_distribution<float> dist = 
        std::uniform_real_distribution<float>(0.0f, 1.0f);
};
RandomGeneratorForUnity rnd;

void setSeed(int seed)
{
    printf("setseed %d", seed);
	rnd.seed((int32_t)seed);
}

float _Debug_CheckCutoutConf(CutoutConfiguration conf)
{
    return conf.transform.q.z;
}




/*
    Mesh Functions
*/

void NvBlastUnityExtMeshRelease(Mesh* mesh)
{
    mesh->release();
}

bool NvBlastUnityExtMeshIsValid(Mesh* mesh)
{
    return mesh->isValid();
}

void NvBlastUnityExtMeshGetVertices(Mesh* mesh, void* data)
{
	const Vertex* verts = mesh->getVertices();
	NvcVec3* dataPx = (NvcVec3*)data;

	for (uint32_t i = 0; i < mesh->getVerticesCount(); i++)
	{
		dataPx[i] = verts[i].p;
	}
}

void NvBlastUnityExtMeshGetNormals(Mesh* mesh, void* data)
{
	const Vertex* verts = mesh->getVertices();
	NvcVec3* dataPx = (NvcVec3*)data;

	for (uint32_t i = 0; i < mesh->getVerticesCount(); i++)
	{
		dataPx[i] = verts[i].n;
	}
}

void NvBlastUnityExtMeshGetIndexes(Mesh* mesh, void* data)
{
	const Edge* edges = mesh->getEdges();
	const Facet* facet = mesh->getFacetsBuffer();
	uint32_t* dataPx = (uint32_t*)data;

	for (uint32_t i = 0; i < mesh->getFacetCount(); i++)
	{
		dataPx[i * 3 + 0] = edges[facet[i].firstEdgeNumber + 0].s;
		dataPx[i * 3 + 1] = edges[facet[i].firstEdgeNumber + 1].s;
		dataPx[i * 3 + 2] = edges[facet[i].firstEdgeNumber + 2].s;
	}
}

void NvBlastUnityExtMeshGetUVs(Mesh* mesh, void* data)
{
	const Vertex* verts = mesh->getVertices();
	NvcVec2* dataPx = (NvcVec2*)data;

	for (uint32_t i = 0; i < mesh->getVerticesCount(); i++)
	{
		dataPx[i] = verts[i].uv[0];
	}
}

uint32_t NvBlastUnityExtMeshGetVerticeCount(Mesh* mesh)
{
    return mesh->getVerticesCount();
}

uint32_t NvBlastUnityExtMeshGetIndexCount(Mesh* mesh)
{
    return mesh->getFacetCount() * 3;
}

/*
    Mesh Cleaner Tool Functions
*/

void NvBlastUnityExtMeshCleanerRelease(MeshCleaner* cleaner)
{
	cleaner->release();
}

Mesh* NvBlastUnityExtMeshCleanerCleanMesh(MeshCleaner* cleaner, Mesh* mesh)
{
	return cleaner->cleanMesh(mesh);
}

/*
    VoronoiSitesGenerator Functions
*/


void NvBlastUnityExtVSGRelease(VoronoiSitesGenerator* vsg)
{
    vsg->release();
}

VoronoiSitesGenerator* NvBlastUnityExtVSGCreate(Mesh* mesh)
{
	return NvBlastExtAuthoringCreateVoronoiSitesGenerator(mesh, &rnd);
}

void NvBlastUnityExtVSGUniformlyGenerateSitesInMesh(VoronoiSitesGenerator* vsg, int count)
{
    vsg->uniformlyGenerateSitesInMesh(count);
}

void NvBlastUnityExtVSGClusteredSitesGeneration(VoronoiSitesGenerator* vsg, uint32_t numberOfClusters, uint32_t sitesPerCluster, float clusterRadius)
{
    vsg->clusteredSitesGeneration(numberOfClusters, sitesPerCluster, clusterRadius);
}

void NvBlastUnityExtVSGAddSite(VoronoiSitesGenerator* vsg, NvcVec3* sites)
{
    vsg->addSite(*sites);
}

uint32_t NvBlastUnityExtVSGGetSitesCount(VoronoiSitesGenerator* vsg)
{
	const NvcVec3* sites;
    return vsg->getVoronoiSites(sites);
}

void NvBlastUnityExtVSGGetSites(VoronoiSitesGenerator* vsg, void* data)
{
    NvcVec3* vertArr = (NvcVec3*)data;

	const NvcVec3* sites;
    uint32_t total = vsg->getVoronoiSites(sites);
	for (uint32_t i = 0; i < total; i++)
	{
		vertArr[i] = sites[i];
	}
}

void NvBlastUnityExtVSGSetMesh(VoronoiSitesGenerator* vsg, Mesh* mesh)
{
    vsg->setBaseMesh(mesh);
}
void NvBlastUnityExtVSGSetStencil(VoronoiSitesGenerator* vsg, Mesh* stencil)
{
    vsg->setStencil(stencil);
}
void NvBlastUnityExtVSGClearStencil(VoronoiSitesGenerator* vsg)
{
    vsg->clearStencil();
}
void NvBlastUnityExtVSGGenerateInSphere(VoronoiSitesGenerator* vsg, int count, float radius, NvcVec3 center)
{
    vsg->generateInSphere(count, radius, center);
}
void NvBlastUnityExtVSGDeleteInSphere(VoronoiSitesGenerator* vsg, float radius, NvcVec3 center, float prob)
{
    vsg->deleteInSphere(radius, center, prob);
}
void NvBlastUnityExtVSGRadialPattern(VoronoiSitesGenerator* vsg, NvcVec3 center, NvcVec3 normal, float radius, int angularSteps, int radialSteps, float angleOffset, float variability)
{
    vsg->radialPattern(center, normal, radius, (int32_t)angularSteps, (int32_t)radialSteps, angleOffset, variability);
}
void NvBlastUnityExtVSGBlastPattern(VoronoiSitesGenerator* vsg, BlastConfiguration conf)
{
    vsg->blastPattern(conf);
}


int32_t NvBlastUnityExtVSGGetNeighbors(VoronoiSitesGenerator* vsg, void* data, int bufferSize)
{
    NvcVec2* vertArr = (NvcVec2*)data;

    const NvcVec3* sites = nullptr;
    uint32_t sitesCount = vsg->getVoronoiSites(sites);

    std::vector<NvcVec3> cellPoints(sitesCount);
    for (uint32_t i = 0; i < sitesCount; ++i)
    {
        cellPoints[i] = sites[i];
    }
    std::vector<std::vector<std::pair<int32_t, int32_t>>> neighbors;

    const int32_t neighborCount = findCellBasePlanes(cellPoints, neighbors);

    if (neighborCount > bufferSize)
    {
        printf("ERROR! INSUFFICIENT SPACE FOR BUFFER!");
        return neighborCount;
    }

    int curBufIdx = 0;
    for (int i = 0; i < (int)sitesCount; i++)
    {
        for (int j = 0; j < neighbors[i].size(); j++)
        {
            std::pair<int32_t, int32_t> neighbor = neighbors[i][j];
            int celln = neighbor.first;
            if (celln < i)
            {
                continue;
            }
            NvcVec2 vec;
            vec.x = i;
            vec.y = celln;
            vertArr[curBufIdx] = vec;
            curBufIdx++;
        }
    }

    return neighborCount;
}




/*
    Fracture Tool Functions
*/


void NvBlastUnityExtFractureToolRelease(FractureTool* tool)
{
    tool->release();
}

void NvBlastUnityExtFractureToolSetRemoveIslands(FractureTool* tool, bool flag)
{
    tool->setRemoveIslands(flag);
}

bool NvBlastUnityExtFractureToolSetSourceMesh(FractureTool* tool, Mesh* mesh)
{
    Mesh const* meshes[1] = { mesh };
    return tool->setSourceMeshes(meshes, 1);
}

bool NvBlastUnityExtFractureToolVoronoiFracturing(FractureTool* tool, uint32_t chunkId, VoronoiSitesGenerator* vsg)
{
    const NvcVec3* sites = nullptr;
    uint32_t sitesCount = vsg->getVoronoiSites(sites);

    return tool->voronoiFracturing(chunkId, sitesCount, sites, false);
}

bool NvBlastUnityExtFractureToolSlicing(FractureTool* tool, int chunkId, SlicingConfiguration conf, bool replaceChunk)
{
    return tool->slicing(chunkId, conf, replaceChunk, &rnd);
}

void NvBlastUnityExtFractureToolFinalizeFracturing(FractureTool* tool)
{
    tool->finalizeFracturing();
}

uint32_t NvBlastUnityExtFractureToolGetChunkCount(FractureTool* tool)
{
    return tool->getChunkCount();
}

Mesh* NvBlastUnityExtFractureToolGetChunkMesh(FractureTool* tool, int chunkId, bool inside)
{
    Triangle* tris = nullptr;
    uint32_t s = tool->getBaseMesh(chunkId, tris);
    if (!tris || s == 0)
    {
        printf("Warning: getBaseMesh returned null or empty.\n");
        return nullptr;
    }

    NvcVec3* pos = new NvcVec3[s * 3];
    NvcVec3* norm = new NvcVec3[s * 3];
    NvcVec2* uv = new NvcVec2[s * 3];
    std::vector<uint32_t> idx;
    inside = inside;

    for (uint32_t i = 0; i < s; i++)
    {
        pos[i * 3 + 0] = tris[i].a.p;
        pos[i * 3 + 1] = tris[i].b.p;
        pos[i * 3 + 2] = tris[i].c.p;

        norm[i * 3 + 0] = tris[i].a.n;
        norm[i * 3 + 1] = tris[i].b.n;
        norm[i * 3 + 2] = tris[i].c.n;

        uv[i * 3 + 0] = tris[i].a.uv[0];
        uv[i * 3 + 1] = tris[i].b.uv[0];
        uv[i * 3 + 2] = tris[i].c.uv[0];

        idx.push_back(i * 3 + 0);
        idx.push_back(i * 3 + 1);
        idx.push_back(i * 3 + 2);
    }
    return NvBlastExtAuthoringCreateMesh(pos, norm, uv, s * 3, idx.data(), idx.size());
}

bool NvBlastUnityExtFractureToolSetSourceMeshes(FractureTool* tool, Mesh const* const* meshes, int meshesSize)
{
    return tool->setSourceMeshes(meshes, meshesSize);
}

int32_t NvBlastUnityExtFractureToolSetChunkMesh(FractureTool* tool, Mesh* mesh, int parentId)
{
    return tool->setChunkMesh(mesh, parentId);
}

Mesh* NvBlastUnityExtFractureToolCreateChunkMesh(FractureTool* tool, int32_t chunkInfoIndex)
{
    return tool->createChunkMesh(chunkInfoIndex);
}

int32_t NvBlastUnityExtFractureToolCut(FractureTool* tool, int chunkId, NvcVec3 normal, NvcVec3 position, NoiseConfiguration noise, bool replaceChunk)
{
    return tool->cut(chunkId, normal, position, noise, replaceChunk, &rnd);
}

int32_t NvBlastUnityExtFractureToolCutout(FractureTool* tool, int chunkId, CutoutConfiguration conf, bool replaceChunk)
{
    return tool->cutout(chunkId, conf, replaceChunk, &rnd);
}

int32_t NvBlastUnityExtFractureToolGetChunkInfoIndex(FractureTool* tool, int chunkId)
{
    return tool->getChunkInfoIndex(chunkId);
}

int32_t NvBlastUnityExtFractureToolGetChunkId(FractureTool* tool, int chunkInfoIndex)
{
    return tool->getChunkId(chunkInfoIndex);
}

int32_t NvBlastUnityExtFractureToolGetChunkDepth(FractureTool* tool, int chunkId)
{
    return tool->getChunkDepth(chunkId);
}

int32_t NvBlastUnityExtFractureToolIslandDetectionAndRemoving(FractureTool* tool, int chunkId)
{
    return tool->islandDetectionAndRemoving(chunkId);
}

bool NvBlastUnityExtFractureToolIsMeshContainOpenEdges(FractureTool* tool, Mesh* mesh)
{
    return tool->isMeshContainOpenEdges(mesh);
}

bool NvBlastUnityExtFractureToolDeleteChunkSubhierarchy(FractureTool* tool, int chunkId, bool deleteRoot)
{
    return tool->deleteChunkSubhierarchy(chunkId, deleteRoot);
}

bool NvBlastUnityExtFractureToolSetApproximateBonding(FractureTool* tool, int chunkId, bool useApproximateBonding)
{
    return tool->setApproximateBonding(chunkId, useApproximateBonding);
}

void NvBlastUnityExtFractureToolFitUvToRect(FractureTool* tool, float side, int chunkId)
{
    tool->fitUvToRect(side, chunkId);
}

void NvBlastUnityExtFractureToolFitAllUvToRect(FractureTool* tool, float side)
{
    tool->fitAllUvToRect(side);
}

int NvBlastUnityExtFractureToolGetCrackEdgesCount(FractureTool* tool)
{
    return tool->getCrackCount();
}

void NvBlastUnityExtFractureToolGetCrackEdges(Nv::Blast::FractureTool* tool, void* data)
{
    tool->getCracks(data);
}
