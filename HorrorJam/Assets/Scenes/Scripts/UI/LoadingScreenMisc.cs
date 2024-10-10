using System;
using TMPro;
using UnityEngine;

public class LoadingScreenMisc : MonoBehaviour
{
    public GameObject tipText;
    public GameObject loadBarProg;
    [SerializeField] string[] tipList;
    // Start is called before the first frame update
    
    void Start()
    {
        tipText = GameObject.Find("TipText");
        loadBarProg = GameObject.Find("LoadProgress");

        //loading bar speed.
        loadBarProg.GetComponent<Animator>().speed = 0.2f;

        InvokeRepeating("replaceTip", 0, 1.2f);
    }

    //Random tip.
    public void replaceTip(){

        System.Random rand = new System.Random(); 
        
        int i = rand.Next(0,tipList.Length);
        
        for (int a = 0; a < tipList.Length; a++)
        {
            if(i == a){
                tipText.GetComponent<TextMeshProUGUI>().text = tipList[a];
            }
        }
    }


}
