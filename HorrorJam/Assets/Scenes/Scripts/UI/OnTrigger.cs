using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Video;

public class OnTrigger : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public GameObject gifObj;

    private void Start() {
        gifObj.GetComponent<VideoPlayer>().playbackSpeed = 0f;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        gifObj.GetComponent<VideoPlayer>().playbackSpeed = 1f;
    }

    public void OnPointerExit(PointerEventData eventData){
        gifObj.GetComponent<VideoPlayer>().playbackSpeed = 0f;
    
    }
    
    
    /*
    
    You don't have to worry about these. It just makes the mp4 plays repeatedly when cursor is over the button. I added placeholder mp4 videos for that, we can add in-game scenes or
    something like that.
    
    You can change the video. Find ImageInfo under Canvas obj, then select one of the child obj's and change the video clip by dragging the new video to video player component.
    */

}
