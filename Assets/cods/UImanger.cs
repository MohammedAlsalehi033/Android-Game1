using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class UImanger : MonoBehaviour
{ Proberties proberties;
    Animator Clock;
    public GameObject thing;
    timer timer2;
    private void Awake()

    {
        Application.targetFrameRate = 60;
                        Time.timeScale = 1;

        
        Clock = GetComponentInChildren<Animator>();

        timer2 = FindObjectOfType<timer>();
                proberties =FindObjectOfType<Proberties>();


    }
    private void Update()
    {

        if (proberties.timeremining.ToString("0") == "0")
        {
            if (GetComponentInChildren<Animator>()) { Clock.enabled = false; }
            disable();
        }



    }
    public void gamePause (){
        Time.timeScale = 0;
    }
     public void gameReusme (){
        Time.timeScale = 1;
    }
    public void nextscreen()
    {
        if (SceneManager.sceneCountInBuildSettings > SceneManager.GetActiveScene().buildIndex + 1)
        {
            SceneManager.LoadScene(sceneBuildIndex: SceneManager.GetActiveScene().buildIndex + 1);
        }
        else { SceneManager.LoadScene("Main Menu"); }

    }

    public void toScreen(int screen)
{
    SceneManager.LoadScene(sceneBuildIndex: screen);
}
    public void enabled(GameObject s){
        s.SetActive(true);
    }

    public void disable()
    {

       
        thing.SetActive(true);




    }

    public void replay (){
        SceneManager.LoadScene(sceneBuildIndex: SceneManager.GetActiveScene().buildIndex);
    }
    public void disable(GameObject s)
    {

        s.SetActive(false);
        




    }
    // Start is called before the first frame update

}
