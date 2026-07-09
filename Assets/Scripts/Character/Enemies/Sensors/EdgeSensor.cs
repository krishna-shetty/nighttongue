using UnityEngine;

public class EdgeSensor : ISensor
{
    private LayerMask _edgeLayer;
    public EdgeSensor(LayerMask edgeLayer)
    {
        this._edgeLayer = edgeLayer;
    }

    public void Sense(IContext info)
    {
        if (info is IEdgeInfo edgeInfo)
        {
            edgeInfo.IsEdgeLeft = !Physics.CheckBox(edgeInfo.TransformPosition.position - new Vector3 (edgeInfo.MeshDimensions.Item1 / 2 + 0.1f, edgeInfo.MeshDimensions.Item2 / 2, 0), new Vector3 (0.1f, 0.1f, 0.1f), Quaternion.identity, _edgeLayer);
            edgeInfo.IsEdgeRight = !Physics.CheckBox(edgeInfo.TransformPosition.position - new Vector3(0, edgeInfo.MeshDimensions.Item2 / 2, 0) + new Vector3(edgeInfo.MeshDimensions.Item1 / 2 + 0.1f, 0, 0), new Vector3(0.1f, 0.1f, 0.1f), Quaternion.identity, _edgeLayer);
        }

    }
}
