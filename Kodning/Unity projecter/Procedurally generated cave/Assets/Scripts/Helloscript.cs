using UnityEngine;

public class Helloscript : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        string[] letters = {"J","O","H","N"};
        for(int i = 0; i<4; i++)
        {
            if(letters[i] == "O")
            break;
               Debug.Log(letters[i]);
        }
    }
}
