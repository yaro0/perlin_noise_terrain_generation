using UnityEngine;
using System.Collections.Generic;

public class Erosion : MonoBehaviour
{
    [Header("Erosion Settings")]
    public int erosionRadius = 3;
    public int numParticles = 10000;
    public int seed = 0;

    private float erodeSpeed = 0.0012f;
    private float sedimentCapacityFactor = 2.0f;
    private float minSedimentCapacity = 0.05f;
    private float depositSpeed = 0.005f;
    private float gravity = 1.4f;
    private float inertia = 0.2f;   
    private int particleLifetime = 25;

    public float evaporateSpeed = 0.01f;
    public float friction = 0.15f;
    public float initialSpeed = 1f;
    public float initialWaterVolume = 1f;

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

    public float[,] Erode(float[,] heightMap, int seed = 0, float preservationFactor = 0.5f)
    {
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
            
            float cellOffsetX = posX - nodeX;
            float cellOffsetY = posY - nodeY;

            HeightAndGradient heightAndGradient = CalculateHeightAndGradient(posX, posY);

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
            
            //Stop if droplet outside map
            if (posX < 0 || posX >= width - 1 || posY < 0 || posY >= height - 1) {
                break;
            }
            
            float newHeight = CalculateHeightAndGradient(posX, posY).height;
            float deltaHeight = newHeight - heightAndGradient.height;
            
            //droplet's sediment capacity higher when moving fast down steep slopes
            float sedimentCapacity = Mathf.Max(minSedimentCapacity, sedimentCapacityFactor * speed * -deltaHeight);
            
            //If carrying more sediment than capacity or if flowing uphill deposit a fraction of the excess sediment
            if (sediment > sedimentCapacity || deltaHeight > 0) {
                float amountToDeposit = deltaHeight > 0 ? 
                    Mathf.Min(deltaHeight, sediment) :
                    (sediment - sedimentCapacity) * depositSpeed;
                
                sediment -= amountToDeposit;

                int coordX = (int)posX;
                int coordY = (int)posY;
                
                //Calculate weights for each node based on the dropet's position
                float weightTL = (1 - cellOffsetX) * (1 - cellOffsetY);
                float weightTR = cellOffsetX * (1 - cellOffsetY);
                float weightBL = (1 - cellOffsetX) * cellOffsetY;
                float weightBR = cellOffsetX * cellOffsetY;
                
                //Distribute sediment
                if (coordX >= 0 && coordX < width - 1 && coordY >= 0 && coordY < height - 1) {
                    map[coordX, coordY] += amountToDeposit * weightTL;
                    map[coordX + 1, coordY] += amountToDeposit * weightTR;
                    map[coordX, coordY + 1] += amountToDeposit * weightBL;
                    map[coordX + 1, coordY + 1] += amountToDeposit * weightBR;
                }
            } 
            else {
                //Erode a fraction of the droplets current carrying capacity
                float amountToErode = Mathf.Min((sedimentCapacity - sediment) * erodeSpeed, -deltaHeight); //Clamp so doesn't dig a hole
                
                //erode from all nodes inside the dropet's radius
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

        float x = posX - coordX;
        float y = posY - coordY;
        
        //calculate heights of the four nodes of the droplets cell
        float heightNW = coordX >= 0 && coordY >= 0 && coordX < width && coordY < height ? map[coordX, coordY] : 0;
        float heightNE = coordX >= -1 && coordY >= 0 && coordX < width - 1 && coordY < height ? map[coordX + 1, coordY] : heightNW;
        float heightSW = coordX >= 0 && coordY >= -1 && coordX < width && coordY < height - 1 ? map[coordX, coordY + 1] : heightNW;
        float heightSE = coordX >= -1 && coordY >= -1 && coordX < width - 1 && coordY < height - 1 ? map[coordX + 1, coordY + 1] : heightNW;
        
        //Calculate droplets direction of flow with bilinear interpolation of height difference along edges
        float gradientX = (heightNE - heightNW) * (1 - y) + (heightSE - heightSW) * y;
        float gradientY = (heightSW - heightNW) * (1 - x) + (heightSE - heightNE) * x;
        
        //Calculate height with bilinear interpolation of the heights of the nodes of the cell
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
        
        float[,] erodedMap = erosion.Erode(heightMap, seed, 0.9f);
        return erodedMap;
    }
}