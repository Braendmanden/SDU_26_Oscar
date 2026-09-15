it iniusing UnityEngine;

public class My_new_script : MonoBehaviour
{
  bool isStunned = false;
  bool isRooted = false;
  Vector3 vector;
  public int[] stats = {16,24,10,3,8,9};
  public float[] floats = new float [10];



    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        bool isStunned = false;
        isStunned = true;
        Debug.Log(isStunned);
Debug.Log(stats[0]);
for (int i = 0; i < stats.Length; i++)
        {
            Debug.Log(stats[i]);
        }
    }

    // Update is called once per frame
    void Update()
    {
        bool isStunned;
        isStunned = false;
isStunned = false;

    }
}
