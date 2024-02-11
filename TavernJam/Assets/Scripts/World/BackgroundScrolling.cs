using UnityEngine;

public class BackgroundScrolling : MonoBehaviour
{  public GameObject player;

    void Update()
    {
        if (player != null)
        {
            // Set the background position to match the player's position
            transform.position = new Vector3(transform.position.x, player.transform.position.y, transform.position.z);
        }
    }
}
