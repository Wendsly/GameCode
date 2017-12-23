using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[RequireComponent (typeof(BoxCollider2D))]

public class RaycastController : MonoBehaviour {
    //this is the maount of rays horizontally and vertically
    public int horizontalRayCount = 4;
    public int verticalRayCount = 4;
    //these controll the spacing
    [HideInInspector]
    public float HorizontalRaySpacing;
    [HideInInspector]
    public float VerticalRaySpacing;
    public RaycastOrigins raycastOrigins;
    //The skin width is used to put the ray origin slightly inside of the player object it's not super important but helps offset some bugs
    public const float skinWidth = .015f;
    //seting up functionallity with built in unity systems
    [HideInInspector]
    public BoxCollider2D collider;
    public virtual void Awake()
    {
        collider = GetComponent<BoxCollider2D>();
        
    }
    public virtual void Start()
    {
        CalculateRaySpacing();
    }
    public void UpdateRaycastOrigins()
    {
        //This gets the bounds of where the raycasts come from the more we have the more raycasts are shown
        //it can't be below 2 and they come form the bottom of the object. 
        Bounds bounds = collider.bounds;
        bounds.Expand(skinWidth * -2);

        raycastOrigins.bottomLeft = new Vector2(bounds.min.x, bounds.min.y);
        raycastOrigins.bottomRight = new Vector2(bounds.max.x, bounds.min.y);
        raycastOrigins.topLeft = new Vector2(bounds.min.x, bounds.max.y);
        raycastOrigins.topRight = new Vector2(bounds.max.x, bounds.max.y);
    }

    public void CalculateRaySpacing()
    {
        Bounds bounds = collider.bounds;
        bounds.Expand(skinWidth * -2);

        horizontalRayCount = Mathf.Clamp(horizontalRayCount, 2, int.MaxValue);
        verticalRayCount = Mathf.Clamp(horizontalRayCount, 2, int.MaxValue);

        HorizontalRaySpacing = bounds.size.y / (horizontalRayCount - 1);
        VerticalRaySpacing = bounds.size.x / (verticalRayCount - 1);
    }
    public struct RaycastOrigins
    {
        public Vector2 topLeft, topRight;
        public Vector2 bottomLeft, bottomRight;
    }
}
