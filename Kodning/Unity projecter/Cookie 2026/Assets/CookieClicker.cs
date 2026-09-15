using UnityEngine;

public class CookieClicker : MonoBehaviour
{
    //type variable_name ;
    public int counter; //0 1 2 .....
    public double decimal_number_double= 0.4;
    public float decimal_number = 0.4f;
    public string text = "Oscar";
    public bool boolean = true;
public int Gangesystem = 2;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

Debug.Log(text);
Debug.Log("Hello World!");


    }

    // Update is called once per frame
    void Update()
    {
if(Input.GetKeyDown(KeyCode.Space))
{
counter = counter + 1;
//counter += 1 //adds whatever you specify
//counter++; //only adds one
 }
 Multiply(6,9);
 Print("Noget andet");
}
void Print(string text)
        {
            Debug.Log(text);
        }
int Double(int number)
{
    return number * 2;
}
int Multiply(int number1, int number2)
{
    return number1 * number2;
}
}