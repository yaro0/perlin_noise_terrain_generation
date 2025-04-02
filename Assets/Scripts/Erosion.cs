using UnityEngine;
using System.Collections.Generic;

public class Erosion : MonoBehaviour
{
    [Header("Erosion Settings")]
    public int erosionRadius = 3;
    public int numParticles = 50000;
    public int particleLifetime = 15;
    public float inertia = 0.15f;
    public float sedimentCapacityFactor = 1f;
    public float minSedimentCapacity = 0.05f;
    public float depositSpeed = 0.5f;
    public float erodeSpeed = 0.05f;
    public float evaporateSpeed = 0.01f;
    public float gravity = 2f;
    public float friction = 0.15f;
    public float initialSpeed = 1f;
    public float initialWaterVolume = 1f;
    public int seed = 0;

    private float[,] map;
    private int width;
    private int height;
    private float[] erosionBrushWeights;
    private int[] erosionBrushOffsets;
    private System.Random random;

    void InitializeBrush() 
    {
        erosionBrushWeights = new float[erosionRadius * erosionRadius * 4];
        erosionBrushOffsets = new int[erosionRadius * erosionRadius * 4];
        
        int brushIndex = 0;
        for (int y = -erosionRadius; y <= erosionRadius; y++) {
            for (int x = -erosionRadius; x <= erosionRadius; x++) {
                float sqrDst = x * x + y * y;
                if (sqrDst < erosionRadius * erosionRadius) {
                    float brushWeight = 1 - Mathf.Sqrt(sqrDst) / erosionRadius;
                    erosionBrushWeights[brushIndex] = brushWeight;
                    erosionBrushOffsets[brushIndex] = y * width + x;
                    brushIndex++;
                }
            }
        }
        
        // Resize arrays to actual size used
        System.Array.Resize(ref erosionBrushWeights, brushIndex);
        System.Array.Resize(ref erosionBrushOffsets, brushIndex);
    }

    // Apply hydraulic erosion to a height map
    public float[,] Erode(float[,] heightMap, int seed = 0, float preservationFactor = 0.9f)
    {
        // Store original heightmap for later blending
        float[,] originalMap = new float[heightMap.GetLength(0), heightMap.GetLength(1)];
        System.Array.Copy(heightMap, originalMap, heightMap.Length);
        
        this.map = heightMap;
        this.width = heightMap.GetLength(0);
        this.height = heightMap.GetLength(1);
        this.random = new System.Random(seed);
        
        InitializeBrush();
        
        for (int i = 0; i < numParticles; i++) {
            ErodePoint();
        }
        
        // Blend eroded map with original to preserve some features
        for (int y = 0; y < height; y++) {
            for (int x = 0; x < width; x++) {
                map[x, y] = map[x, y] * (1 - preservationFactor) + originalMap[x, y] * preservationFactor;
            }
        }
        
        return map;
    }

    void ErodePoint() 
    {
        // Create water droplet at random position on map
        float posX = random.Next(0, width - 1);
        float posY = random.Next(0, height - 1);
        
        float dirX = 0;
        float dirY = 0;
        float sediment = 0;
        float speed = initialSpeed;
        float water = initialWaterVolume;
        
        for (int lifetime = 0; lifetime < particleLifetime; lifetime++) {
            int nodeX = (int)posX;
            int nodeY = (int)posY;
            
            // Calculate droplet's offset inside the cell (0,0) to (1,1)
            float cellOffsetX = posX - nodeX;
            float cellOffsetY = posY - nodeY;
            
            // Calculate droplet's height and direction of flow with bilinear interpolation of surrounding heights
            HeightAndGradient heightAndGradient = CalculateHeightAndGradient(posX, posY);
            
            // Update the droplet's direction and position (using normal)
            dirX = dirX * inertia - heightAndGradient.gradientX * (1 - inertia);
            dirY = dirY * inertia - heightAndGradient.gradientY * (1 - inertia);
            
            // Normalize direction
            float len = Mathf.Sqrt(dirX * dirX + dirY * dirY);
            if (len != 0) {
                dirX /= len;
                dirY /= len;
            }
            
            posX += dirX;
            posY += dirY;
            
            // Stop simulating if droplet is outside map
            if (posX < 0 || posX >= width - 1 || posY < 0 || posY >= height - 1) {
                break;
            }
            
            // Find the droplet's new height and calculate the deltaHeight
            float newHeight = CalculateHeightAndGradient(posX, posY).height;
            float deltaHeight = newHeight - heightAndGradient.height;
            
            // Calculate the droplet's sediment capacity (higher when moving fast down steep slopes)
            float sedimentCapacity = Mathf.Max(minSedimentCapacity, sedimentCapacityFactor * speed * -deltaHeight);
            
            // If carrying more sediment than capacity, or if flowing uphill, deposit a fraction of the excess sediment
            if (sediment > sedimentCapacity || deltaHeight > 0) {
                float amountToDeposit = deltaHeight > 0 ? 
                    Mathf.Min(deltaHeight, sediment) : // Going uphill, deposit all of deltaHeight
                    (sediment - sedimentCapacity) * depositSpeed; // Going downhill, deposit fraction of excess
                
                sediment -= amountToDeposit;
                
                // Add the sediment to the four nodes of the current cell using bilinear interpolation
                // Deposition is not distributed over a radius (like erosion) so that it's much more local
                int coordX = (int)posX;
                int coordY = (int)posY;
                
                // Calculate weights for each node based on the droplet's position
                float weightTL = (1 - cellOffsetX) * (1 - cellOffsetY);
                float weightTR = cellOffsetX * (1 - cellOffsetY);
                float weightBL = (1 - cellOffsetX) * cellOffsetY;
                float weightBR = cellOffsetX * cellOffsetY;
                
                // Distribute sediment to the nodes
                if (coordX >= 0 && coordX < width - 1 && coordY >= 0 && coordY < height - 1) {
                    map[coordX, coordY] += amountToDeposit * weightTL;
                    map[coordX + 1, coordY] += amountToDeposit * weightTR;
                    map[coordX, coordY + 1] += amountToDeposit * weightBL;
                    map[coordX + 1, coordY + 1] += amountToDeposit * weightBR;
                }
            } 
            else {
                // Erode a fraction of the droplet's current carrying capacity.
                // Clamp the erosion to the change in height so that it doesn't dig a hole
                float amountToErode = Mathf.Min((sedimentCapacity - sediment) * erodeSpeed, -deltaHeight);
                
                // Use erosion brush to erode from all nodes inside the droplet's radius
                for (int brushPointIndex = 0; brushPointIndex < erosionBrushWeights.Length; brushPointIndex++) {
                    int nodeIndex = nodeY * width + nodeX + erosionBrushOffsets[brushPointIndex];
                    
                    if (nodeIndex >= 0 && nodeIndex < width * height) {
                        int brushX = nodeIndex % width;
                        int brushY = nodeIndex / width;
                        float weighedErodeAmount = amountToErode * erosionBrushWeights[brushPointIndex];
                        float deltaSediment = map[brushX, brushY] < weighedErodeAmount ? map[brushX, brushY] : weighedErodeAmount;
                        map[brushX, brushY] -= deltaSediment;
                        sediment += deltaSediment;
                    }
                }
            }
            
            // Update droplet's speed and water content
            speed = Mathf.Sqrt(speed * speed + deltaHeight * gravity);
            speed = Mathf.Max(0, speed * (1 - friction));
            water *= (1 - evaporateSpeed);
        }
    }

    struct HeightAndGradient {
        public float height;
        public float gradientX;
        public float gradientY;
    }

    HeightAndGradient CalculateHeightAndGradient(float posX, float posY) {
        int coordX = (int)posX;
        int coordY = (int)posY;
        
        // Calculate droplet's offset inside the cell (0,0) to (1,1)
        float x = posX - coordX;
        float y = posY - coordY;
        
        // Calculate heights of the four nodes of the droplet's cell
        float heightNW = coordX >= 0 && coordY >= 0 && coordX < width && coordY < height ? map[coordX, coordY] : 0;
        float heightNE = coordX >= -1 && coordY >= 0 && coordX < width - 1 && coordY < height ? map[coordX + 1, coordY] : heightNW;
        float heightSW = coordX >= 0 && coordY >= -1 && coordX < width && coordY < height - 1 ? map[coordX, coordY + 1] : heightNW;
        float heightSE = coordX >= -1 && coordY >= -1 && coordX < width - 1 && coordY < height - 1 ? map[coordX + 1, coordY + 1] : heightNW;
        
        // Calculate droplet's direction of flow with bilinear interpolation of height difference along the edges
        float gradientX = (heightNE - heightNW) * (1 - y) + (heightSE - heightSW) * y;
        float gradientY = (heightSW - heightNW) * (1 - x) + (heightSE - heightNE) * x;
        
        // Calculate height with bilinear interpolation of the heights of the nodes of the cell
        float terrainHeight = heightNW * (1 - x) * (1 - y) + heightNE * x * (1 - y) + heightSW * (1 - x) * y + heightSE * x * y;
        
        HeightAndGradient result = new HeightAndGradient();
        result.height = terrainHeight;
        result.gradientX = gradientX;
        result.gradientY = gradientY;
        
        return result;
    }

    public static float[,] ApplyErosion(float[,] heightMap, int numParticles = 10000, int erosionRadius = 4, int seed = 0)
    {
        Erosion erosion = new Erosion();
        erosion.numParticles = numParticles;
        erosion.erosionRadius = erosionRadius;
        erosion.seed = seed;
        erosion.erodeSpeed = 0.12f;
        erosion.sedimentCapacityFactor = 2.0f; 
        erosion.depositSpeed = 0.0005f; 
        erosion.gravity = 1.4f;
        erosion.inertia = 0.2f;            
        erosion.particleLifetime = 25;  
        
        float[,] erodedMap = erosion.Erode(heightMap, seed, 0.9f);
        return erodedMap;
    }
}