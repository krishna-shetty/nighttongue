using UnityEngine;

public static class MeshSize
{
    public static (float, float) GetSize(GameObject currGameObject)
    {
        MeshFilter currMeshFilter = currGameObject.GetComponent<MeshFilter>();
        if (currMeshFilter != null)
        {
            Bounds bounds = currMeshFilter.sharedMesh.bounds;
            Vector3 scaled = Vector3.Scale(bounds.size, currMeshFilter.transform.lossyScale);
            return (scaled.x, scaled.y);
        }
        else
        {
            return (float.NaN, float.NaN);
        }
    }
}
