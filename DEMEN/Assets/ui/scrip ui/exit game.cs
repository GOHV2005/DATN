using UnityEngine;

public class ExitGame : MonoBehaviour
{
    public void Exit()
    {
        Debug.Log("Thoát game"); // để test trong Editor
        Application.Quit();
    }
}
