using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PointCamFollow : MonoBehaviour
{
    public Rigidbody2D target;
    public float smoothSpeedX = 0.1f;
    public float smoothSpeedY = 0.1f;
    public float gizmoRadius = 0.1f;

    FocusArea focusArea;

    void Start()
    {
        focusArea = new FocusArea(target.GetComponent<Collider2D>().bounds.center, smoothSpeedX, smoothSpeedY);
    }

    void FixedUpdate()
    {

        focusArea.Update(target.GetComponent<Collider2D>().bounds.center);
        Vector3 focusPosition = focusArea.center; //+ Vector2.up * verticalOffset;

        transform.position = focusPosition + Vector3.forward * -10;
    }

    void OnDrawGizmos()
    {
        Gizmos.color = new Color(1, 0, 0);
        Gizmos.DrawWireSphere(focusArea.center, gizmoRadius);
    }

    struct FocusArea
    {
        public Vector3 center;
        float speedX;
        float speedY;

        public FocusArea(Vector3 areaCenter, float smoothSpeedX, float smoothSpeedY)
        {
            speedX = smoothSpeedX;
            speedY = smoothSpeedY;
            center = areaCenter;
        }

        public void Update(Vector3 newCenter)
        {

            //calculate camera velocity from distance between camera and collider center.
            float velocityX = (newCenter.x - center.x) * speedX;
            float velocityY = (newCenter.y - center.y) * speedY;
            Vector3 velocity = new Vector3(velocityX, velocityY, 0);

            center += velocity;
        }

        public void SetCenter(Vector3 c)
        {
            center = c;
        }
    }
}
