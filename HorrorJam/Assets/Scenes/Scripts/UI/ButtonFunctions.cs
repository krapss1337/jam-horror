using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class ButtonFunctions : MonoBehaviour
{
    public int mainSceneIndex; //Check the File > Build Settings to find the index number of a scene.
    public GameObject buttonPannel;
    public GameObject optionCanvas;

    public GameObject pages;
    public GameObject creditsCanvas;

    

    //Don't forget to "Find" objects on Start, just incase.
    

    public void StartButtonFunc(){

        StartCoroutine(LoadTime());

    }

    IEnumerator LoadTime(){
        yield return new WaitForSeconds(5f);
        SceneManager.LoadScene(mainSceneIndex);
    }

    public void OptionButtonFunc(){
        optionCanvas.SetActive(true);
    }

    public void CloseButton() {

        if(optionCanvas.activeSelf == true){
            for (int i = 0; i < pages.transform.childCount; i++)
            {
                pages.transform.GetChild(i).gameObject.SetActive(false);
            }
            optionCanvas.SetActive(false);
        }

        if(creditsCanvas.activeSelf == true){
            creditsCanvas.SetActive(false);
        }
        
    }

    public void CreditsButtonFunc(){
        creditsCanvas.SetActive(true);
    }

    private void Update() {
        if(optionCanvas.activeSelf == true || creditsCanvas.activeSelf == true){
            for (int i = 0; i < buttonPannel.transform.childCount; i++)
            {
                buttonPannel.transform.GetChild(i).GetComponent<Button>().interactable = false;
            }
        }
        else{
            for (int i = 0; i < buttonPannel.transform.childCount; i++)
            {
                buttonPannel.transform.GetChild(i).GetComponent<Button>().interactable = true;
            }
        } 
    }
}
