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
    }

    void FixedUpdate()
    {
        focusArea.Update(target.GetComponent<Collider2D>().bounds.center);
        Vector3 focusPosition = focusArea.areaGameObject.transform.position;

        if (!blockedX || !cameraBoundsOn)
            transform.position = new Vector3(focusPosition.x, transform.position.y, -10);
        if (!blockedY || !cameraBoundsOn)
            transform.position = new Vector3(transform.position.x, focusPosition.y, -10);
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
        collisionBound.GetComponent<CamBoundTrigger>().blockDelegate = SetBlocked;


    }

    Vector2 GetOrthographicCameraSize()
    {
        float screenX = Camera.main.orthographicSize * Screen.width / Screen.height;
        float screenY = Camera.main.orthographicSize;
        return new Vector2(screenX, screenY);
    }

    private class FocusArea
    {
        Vector3 center;
        readonly float speedX;
        readonly float speedY;

        public bool blockedX;
        public bool blockedY;

        public GameObject areaGameObject;

        public FocusArea(Vector3 areaCenter, float smoothSpeedX, float smoothSpeedY)
        {
            speedX = smoothSpeedX;
            speedY = smoothSpeedY;
            center = areaCenter;

            areaGameObject = new GameObject();
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

    public void SetBlockedX(bool b, Collider2D source)
    {
        blockedX = b;
    }
    public void SetBlocked(bool b, Collider2D source)
    {

        //get distance x and y.
        float distX = Mathf.Abs(this.transform.position.x - source.transform.position.x);
        float distY = Mathf.Abs(this.transform.position.y - source.transform.position.y);

        if (distX >= distY)
            blockedX = b;
        else
            blockedY = b;
    }
}
