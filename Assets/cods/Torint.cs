using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Torint : MonoBehaviour
{    float Xup = 8.27f;
    Proberties proberties;

     float Xdown = -8.27f;
     float Yup = 4.4f;
     float Ydown=-4.4f ;
    public GameObject The_bullit; 
public float shotspeed;
    

    Vector2 random;
    GameObject b;
private void Awake() {
            proberties =FindObjectOfType<Proberties>();

}    // Start is called before the first frame update
    void Start()
    {       
    }

    // Update is called once per frame
    void Update()
    {

        InvokeRepeating("shot",shotspeed,1.0f);
    }
    void shot(){
           Instantiate(The_bullit , this.transform.position,this.transform.rotation);
        CancelInvoke();
    }
  
}
