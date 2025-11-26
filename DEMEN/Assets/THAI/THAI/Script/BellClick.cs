using UnityEngine;

public class BellClick : MonoBehaviour
{
    public int bellIndex;              // set 0,1,2 in Inspector
    public BellGameManager gameManager;

    void OnMouseDown()
    {
        gameManager.ChooseBell(bellIndex);
    }
}
