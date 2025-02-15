using UnityEngine;

public class FlipSprite : MonoBehaviour
{
    public PolygonCollider2D polygonCollider;

    void Start()
    {
        // Flip the collider once when the enemy spawns
        FlipCollider();
    }

    void FlipCollider()
    {
        // Get the current points (vertices) of the collider
        Vector2[] points = polygonCollider.points;

        // Flip the collider by inverting the X coordinate of each point across the Y-axis
        for (int i = 0; i < points.Length; i++)
        {
            points[i].x = -points[i].x;  // Invert the X axis for each point to flip across the Y-axis
        }

        // Update the collider with the new flipped points
        polygonCollider.points = points;
    }
}