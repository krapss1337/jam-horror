using UnityEngine;
using UnityEngine.UI;
using System.Runtime.InteropServices;

public class DcHyperlink : MonoBehaviour
{
    public void OpenInviteLink(){
        #if !UNITY_EDITOR
        openWindow("https://www.youtube.com/watch?v=xvFZjo5PgG0")
        #endif
    }

    [DllImport("__Internal")]
    private static extern void openWindow(string url);
}
