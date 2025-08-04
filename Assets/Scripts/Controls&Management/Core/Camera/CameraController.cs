using UnityEngine;

public class CameraController : MonoBehaviour
{
    public float dragSpeed = 2f;
    private Vector3 dragOrigin;
    private bool isDragging = false;

    private float minX;
    private float maxX;

    // Call this to update bounds when buildings spawn
    public void UpdateBounds()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        GameObject enemy = GameObject.FindGameObjectWithTag("Enemy");

        if (player != null && enemy != null)
        {
            float camHalfWidth = Camera.main.orthographicSize * Camera.main.aspect;
            
            minX = player.transform.position.x + camHalfWidth;
            maxX = enemy.transform.position.x - camHalfWidth;
        }
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            dragOrigin = Input.mousePosition;
            isDragging = true;
        }

        if (Input.GetMouseButtonUp(0))
        {
            isDragging = false;
        }

        if (isDragging)
        {
            Vector3 difference = Camera.main.ScreenToViewportPoint(Input.mousePosition - dragOrigin);
            float moveX = -difference.x * dragSpeed;

            Vector3 newPosition = transform.position + new Vector3(moveX, 0, 0);
            newPosition.x = Mathf.Clamp(newPosition.x, minX, maxX);
            transform.position = newPosition;

            dragOrigin = Input.mousePosition;
        }
    }
}