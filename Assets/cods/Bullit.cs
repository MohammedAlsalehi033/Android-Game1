using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bullit : MonoBehaviour
{
    Proberties proberties;
    Torint proberties2;
     public float Xup;
    public float Xdown;
    public float Yup;
    public float Ydown;

    public bool Destory = false;

    Vector2 targetpositon;

    // Start is called before the first frame update
    private void Awake() {
        proberties = FindObjectOfType<Proberties>(); 
        proberties2 = FindObjectOfType<Torint>(); 
    }
    void Start()
    {
        targetpositon = Getrandom();
    }

    // Update is called once per frame
    void Update()
    {
        this.transform.position = Vector2.MoveTowards(this.transform.position , targetpositon ,proberties.difficulty*Time.deltaTime );
     if(proberties.timeremining.ToString("0") == "0" ){
         Destroy(this.gameObject);
     }
     
     if (this.transform.position.Equals(targetpositon) && Destory){
         Destroy(this.gameObject);
     }

      
    }
       Vector2 Getrandom (){
        float Randomx = Random.Range(Xup,Xdown);
        float Randomy = Random.Range(Yup,Ydown); 
       return new Vector2(Randomx,Randomy);

   }
}
