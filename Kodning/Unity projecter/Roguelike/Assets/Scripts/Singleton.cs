using UnityEngine;
using System.Collections.Generic;
  
public class Singleton : MonoBehaviour
{
    
    public static Singleton instance = null;
    public static List<Singleton> allInstances;
    public int level;
    public int AmountOfEnemies;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        
            else if(instance != null)
        
        {
            Destroy(gameObject);
        }
    }
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    public static void SpawnEnemy()
    {
        int number;
        
    }

}
