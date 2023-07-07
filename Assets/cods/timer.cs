using UnityEngine.SceneManagement;
using UnityEngine;
using UnityEngine.UI;
public class timer : MonoBehaviour
{
Proberties proberties;
    moving[] Moving;
    moving[] Movingspeed;
    Touchinh[] touchinhs;
    Touchinh touchinh;

    Bullit[] bullit;

    Creatathing creatathing;


    public Text text;
    private void Awake()
    {
        proberties =FindObjectOfType<Proberties>();
        Moving = FindObjectsOfType<moving>();
        Movingspeed = FindObjectsOfType<moving>();
        touchinhs = FindObjectsOfType<Touchinh>();
        bullit = FindObjectsOfType<Bullit>();
    }
    void Update()

    {
        if (proberties.timeremining > 0)
        {
            proberties.timeremining -= Time.deltaTime;
            InvokeRepeating("incrase_speed",0,10f);
        }
        else if (proberties.timeremining.ToString("0") == "0")
        {

            for (int i = 0; i < touchinhs.Length; i++)
            {
                touchinhs[i].enabled = false;
            }
            for (int i = 0; i < Moving.Length; i++)

            {
                
                Moving[i].enabled = false;
            }
            for (int i = 0; i < bullit.Length; i++)
            {
                bullit[i].enabled = false;
            }
        }

        text.text = proberties.timeremining.ToString("0");
    }

void incrase_speed(){
    for (int i = 0; i < Movingspeed.Length; i++)
    {
        Movingspeed[i].Speed = Movingspeed[i].Speed + proberties.difficulty*Time.deltaTime;
    }
 
}
}
