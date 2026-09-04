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
                float nodeSize = 0.64f;
                
                // Calculate new dimensions by adding 300
                int newWidth = currentWidth + 200;
                int newDepth = currentDepth + 200;
                
                // Use SetDimensions to properly update the grid
                gridGraph.SetDimensions(newWidth, newDepth, nodeSize);
                
                // Scan the pathfinding graph
                path.Scan();
            }
        }
    }
}
