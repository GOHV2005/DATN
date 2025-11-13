using System.Collections;
using UnityEngine;

public class InputHandler : MonoBehaviour
{
    public Camera mainCamera;
    public GridManager gridManager;
    private Gem selectedGem = null;

    void Awake() { if (mainCamera == null) mainCamera = Camera.main; }

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Vector2 wp = mainCamera.ScreenToWorldPoint(Input.mousePosition);
            Collider2D col = Physics2D.OverlapPoint(wp);
            if (col != null)
            {
                Gem g = col.GetComponent<Gem>();
                if (g != null) selectedGem = g;
            }
        }

        if (Input.GetMouseButtonUp(0) && selectedGem != null)
        {
            Vector2 wp = mainCamera.ScreenToWorldPoint(Input.mousePosition);
            Collider2D col = Physics2D.OverlapPoint(wp);
            if (col != null)
            {
                Gem g = col.GetComponent<Gem>();
                if (g != null && g != selectedGem)
                {
                    StartCoroutine(gridManager.TrySwap(selectedGem, g, (success) => {
                        // optional: call GameManager to deduct moves
                    }));
                }
            }
            selectedGem = null;
        }
    }
}
