using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class moving : MonoBehaviour
{

Collider2D collider2;
    Proberties proberties;
     float Xup = 8.27f;
     float Xdown = -8.27f;
     float Yup = 4.4f;
     float Ydown=-4.4f ;

    Transform cute;
    public Transform cute2;
    Vector2 Targetpositon;
    public float Speed;
    public bool isfoll = true;
    public bool iscla = true;

    public bool stopAfterAwhile = false;
    // Start is called before the first frame update
    private void Awake() {
        collider2 = GetComponent<Collider2D>();

        proberties =FindObjectOfType<Proberties>();
    }
    void Start()
    {
        cute = GetComponent<Transform>();
        Targetpositon = Getrandom();
    }

    // Update is called once per frame
    void Update()
    
    {
        
        
        if(cute.position.y != Targetpositon.y &&cute.position.x != Targetpositon.x && isfoll){
      cute.position = Vector2.MoveTowards(cute.position,Targetpositon,Speed*Time.deltaTime);
    }else if (cute.position.y != Targetpositon.y &&cute.position.x != Targetpositon.x && isfoll==false){
        cute.position =Vector2.MoveTowards(cute.position,cute2.position,Speed*Time.deltaTime);
    }
    else{Targetpositon = Getrandom();}
    
    
    }
   Vector2 Getrandom (){
        float Randomx = Random.Range(Xup,Xdown);
        float Randomy = Random.Range(Yup,Ydown); 
       return new Vector2(Randomx,Randomy);

   } 
private void OnTriggerEnter2D(Collider2D other) {
    if(other.tag=="thing" && iscla){
        SceneManager.LoadScene( SceneManager.GetActiveScene().name );}
}
public void mov(string s){
Debug.Log(s);
}
}
