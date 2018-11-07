using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PointCamFollow : MonoBehaviour
{
    public Rigidbody2D target;
    public float smoothSpeedX = 0.1f;
    public float smoothSpeedY = 0.1f;
    public float gizmoRadius = 0.1f;
    public bool cameraBoundsOn = false;
    public GameObject boundPrefab;

    public float dampTime = 0.15f;
    private Vector2 velocity = Vector2.zero;

    public bool doSmoothing;

    [SerializeField] private Transform objectToFollow;

    FocusArea focusArea;

    GameObject collisionBound;

    bool blockedX;
    bool blockedY;

    void Start()
    {
        //create camera focus area
        focusArea = new FocusArea(target.GetComponent<Collider2D>().bounds.center, smoothSpeedX, smoothSpeedY);

        //create bounding colliders(used for limiting camera movement). Bounds are tied to the focusarea.
        RespawnBound();

        Debug.Log("WIDTH: " + Camera.main.pixelWidth + " HEIGHT:" + Camera.main.pixelHeight);
    }

    void LateUpdate()
    {

        if (doSmoothing)
            SmoothCameraMove();
        else
        {
            Vector3 e = objectToFollow.transform.position;
            e.z = -10;
            this.transform.position = e;
        }



    }

    void OnDrawGizmos()
    {
        if (focusArea != null)
        {
            Gizmos.color = new Color(1, 0, 0);
            Gizmos.DrawWireSphere(focusArea.areaGameObject.transform.position, gizmoRadius);
        }
    }

    void RespawnBound()
    {
        //destroy current bound if it exist.
        if (collisionBound != null)
            Destroy(collisionBound);

        //Create bounding camera collider.
        collisionBound = Instantiate(boundPrefab, new Vector3(0, 0, 0), Quaternion.identity);

        //make bound child of the focusarea to make its position relative to this at all times.
        collisionBound.transform.parent = focusArea.areaGameObject.transform;

        //calculate the current screen size.
        Vector2 screenSize = GetOrthographicCameraSize();

        //resize and position bound to fit the screen.
        collisionBound.transform.position = new Vector3(0, 0, 0);
        collisionBound.transform.localScale = new Vector3(screenSize.x * 2, screenSize.y * 2, collisionBound.transform.localScale.z);

        //delegate block responsibility
        collisionBound.GetComponent<CamBoundTrigger>().XblockDelegate = SetBlockedX;
        collisionBound.GetComponent<CamBoundTrigger>().YblockDelegate = SetBlockedY;
    }

    Vector2 GetOrthographicCameraSize()
    {
        float screenX = Camera.main.orthographicSize * Screen.width / Screen.height;
        float screenY = Camera.main.orthographicSize;
        return new Vector2(screenX, screenY);
    }

    private Vector3 GetMovementVector()
    {
        Vector2 newCenter = objectToFollow.position;
        Vector2 camCenter = this.transform.position;

        //calculate camera velocity from distance and smoothing-speed.
        float velX = 0;
        float velY = 0;
        if (!blockedX || !cameraBoundsOn)
            velX = (newCenter.x - camCenter.x) * smoothSpeedX;
        if (!blockedY  || !cameraBoundsOn)
            velY = (newCenter.y - camCenter.y) * smoothSpeedY;

        Vector3 velocity = new Vector3(velX, velY, 0);
        return velocity;
    }

    void SmoothCameraMove()
    {
        if (objectToFollow != null)
        {
            //TODO change this.
            Vector2 delta = objectToFollow.position - Camera.main.WorldToViewportPoint(new Vector2 (0.5f, 0.5f));
            Vector2 destination = (Vector2)transform.position + delta;

            transform.position = Vector2.SmoothDamp(transform.position, destination, ref velocity, dampTime);
        }
    }

    public void SetBlockedY(bool b){ blockedY = b; }
    public void SetBlockedX(bool b){ blockedX = b; }

    private class FocusArea
    {
        Vector3 center;
        readonly float speedX;
        readonly float speedY;

        public GameObject areaGameObject;

        public FocusArea(Vector3 areaCenter, float smoothSpeedX, float smoothSpeedY)
        {
            speedX = smoothSpeedX;
            speedY = smoothSpeedY;
            center = areaCenter;

            areaGameObject = new GameObject("CamFocusArea");
        }

        public void Update(Vector3 newCenter)
        {
            //calculate camera velocity from distance between camera and collider center.
            float velocityX = (newCenter.x - center.x) * speedX;
            float velocityY = (newCenter.y - center.y) * speedY;

            Vector3 velocity = new Vector3(velocityX, velocityY, 0);
            center += velocity;

            areaGameObject.transform.position = center;
        }
    }
}
