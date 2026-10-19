using UnityEngine;

public class Proceduralgeneration : MonoBehaviour
{
    public int width;
    public int height;
    public string seed;
    public bool useRandomSeed;
    [Range(0,100)]
    public int randomFillPercent;
    int[,] map;

    void Start ()
    {
        GenerateMap();
    }
    void GenerateMap()
    {
        map = new int[width,height];
    }
    void RandomFillMap()
    {
        if (useRandomSeed)
        {
            seed = Time.time.ToString();
        }
        System.Random prng = new System.Random(seed.GetHashCode());
    }
}
