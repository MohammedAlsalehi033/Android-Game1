using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Creatathing : MonoBehaviour


{
    public GameObject[] things  = {} ;
    
    public int numberofthings;
    public int currentnumber;
    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()

    
    {
        int random = Random.Range(0,things.Length);
        if (currentnumber < numberofthings / 2)
        {
            Instantiate(things[0], Getrandom(), things[random].transform.rotation);
            Instantiate(things[1], Getrandom(), things[random].transform.rotation);
            currentnumber++;
        }

    }
    Vector2 Getrandom()
    {
        float Randomx = Random.Range(8.27f, -8.27f);
        float Randomy = Random.Range(4.4f, -4.4f);
        return new Vector2(Randomx, Randomy);

    }
    
}
