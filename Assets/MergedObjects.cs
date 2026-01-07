using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
public class RobustMeshCombiner : MonoBehaviour
{
    [ContextMenu("Combine Meshes Now")]
    public void CombineMeshesInEditor()
    {
        CombineMeshes();
    }

    void Start()
    {
        //CombineMeshes();
    }

    void CombineMeshes()
    {
        var materialToMeshList = new Dictionary<Material, List<CombineInstance>>();
        var meshRenderers = GetComponentsInChildren<MeshRenderer>(true);

        if (meshRenderers.Length <= 1)
        {
            Debug.LogWarning("結合対象の子オブジェクトが見つかりません。", this);
            return;
        }

        foreach (var meshRenderer in meshRenderers)
        {
            if (meshRenderer.transform == this.transform) continue;
            var meshFilter = meshRenderer.GetComponent<MeshFilter>();
            if (meshFilter == null || meshFilter.sharedMesh == null) continue;
            var materials = meshRenderer.sharedMaterials;
            for (int i = 0; i < materials.Length; i++)
            {
                Material mat = materials[i];
                if (mat == null) continue;
                if (!materialToMeshList.ContainsKey(mat))
                {
                    materialToMeshList.Add(mat, new List<CombineInstance>());
                }
                CombineInstance combineInstance = new CombineInstance
                {
                    mesh = meshFilter.sharedMesh,
                    subMeshIndex = i,
                    transform = this.transform.worldToLocalMatrix * meshRenderer.transform.localToWorldMatrix
                };
                materialToMeshList[mat].Add(combineInstance);
            }
            meshRenderer.gameObject.SetActive(false);
        }

        var finalCombineInstances = new List<CombineInstance>();
        var finalMaterials = new List<Material>();

        foreach (var pair in materialToMeshList)
        {
            Mesh subMesh = new Mesh();

            // ★★★★★★★★★★ ここにも追加が必要でした ★★★★★★★★★★
            // 一時的なメッシュも大きな名簿(UInt32)を使うように設定
            subMesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
            // ★★★★★★★★★★★★★★★★★★★★★★★★★★★★★★★★★

            subMesh.CombineMeshes(pair.Value.ToArray(), true);
            finalCombineInstances.Add(new CombineInstance { mesh = subMesh });
            finalMaterials.Add(pair.Key);
        }

        var finalMesh = new Mesh();
        finalMesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
        finalMesh.CombineMeshes(finalCombineInstances.ToArray(), false);

        GetComponent<MeshFilter>().sharedMesh = finalMesh;
        GetComponent<MeshRenderer>().sharedMaterials = finalMaterials.ToArray();

        if (finalMesh.vertexCount > 0)
        {
            Debug.Log($"メッシュマージ成功！ 頂点数: {finalMesh.vertexCount}, マテリアル数: {finalMaterials.Count}", this);
        }
        else
        {
            Debug.LogError("メッシュマージに失敗しました。結合後の頂点数が0です。モデルのRead/Write設定を確認してください。", this);
        }
    }
}