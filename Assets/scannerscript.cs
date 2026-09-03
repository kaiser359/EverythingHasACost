using UnityEngine;

public class scannerscript : MonoBehaviour
{
    public AstarPath path;
    public float scanDelay = 3f;

    void Update()
    {
   
    }
    
    void Start()
    {
        Invoke(nameof(ScanAndExpandGrid), scanDelay);
    }

    void ScanAndExpandGrid()
    {
        if (path != null)
        {
            var gridGraph = path.data.gridGraph;
            if (gridGraph != null)
            {
                // Get current dimensions and node size
                int currentWidth = gridGraph.width;
                int currentDepth = gridGraph.depth;
                float nodeSize = gridGraph.nodeSize;
                
                // Calculate new dimensions by adding 300
                int newWidth = currentWidth + 300;
                int newDepth = currentDepth + 300;
                
                // Use SetDimensions to properly update the grid
                gridGraph.SetDimensions(newWidth, newDepth, nodeSize);
                
                // Scan the pathfinding graph
                path.Scan();
            }
        }
    }
}
