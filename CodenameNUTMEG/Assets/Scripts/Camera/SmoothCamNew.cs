using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SmoothCamNew : MonoBehaviour {

    [SerializeField] private Transform target;
    [SerializeField] private bool smoothMovement = true;
    [SerializeField] private float dampTime = 0.15f;
    [SerializeField] private GameObject boundPrefab;
    [SerializeField] private bool checkBounds = true;


    private Vector2 velocityX;
    private Vector2 velocityY;
    private bool blockedX;
    private bool blockedY;
    private GameObject bound;

    //camera bounds, set at runtime by bound-collider triggers.
    private float? xMin;
    private float? yMin;
    private float? xMax;
    private float? yMax;


    private void Awake()
    {
        RespawnBound();

        Debug.Log("WIDTH: " + Camera.main.pixelWidth + " HEIGHT:" + Camera.main.pixelHeight);
    }


    private void OnDrawGizmos()
    {

        //draw bounds
        if (yMin.HasValue)
        {
            Gizmos.color = new Color(1, 0, 0);
            Vector2 pos = new Vector2(target.position.x, yMin.Value);
            Gizmos.DrawWireSphere(pos, 0.2f);
        }
        if (yMax.HasValue)
        {
            Gizmos.color = new Color(1, 0, 0);
            Vector2 pos = new Vector2(target.position.x, yMax.Value);
            Gizmos.DrawWireSphere(pos, 0.2f);
        }
        if (xMin.HasValue)
        {
            Gizmos.color = new Color(1, 0, 0);
            Vector2 pos = new Vector2(xMin.Value, target.position.y);
            Gizmos.DrawWireSphere(pos, 0.2f);
        }
        if (xMax.HasValue)
        {
            Gizmos.color = new Color(1, 0, 0);
            Vector2 pos = new Vector2(xMax.Value, target.position.y);
            Gizmos.DrawWireSphere(pos, 0.2f);
        }
    }

    private void LateUpdate()
    {
        if (target)
        {
            if (smoothMovement)
            {
                Vector2 delta = target.transform.position - GetComponent<Camera>().ViewportToWorldPoint(new Vector2(0.5f, 0.5f));
                Vector2 destination = (Vector2)transform.position + delta;

                Vector2 interX = new Vector2(transform.position.x, 0);
                Vector2 interY = new Vector2(0, transform.position.y);

                interX = Vector2.SmoothDamp(interX, destination, ref velocityX, dampTime);
                interY = Vector2.SmoothDamp(interY, destination, ref velocityY, dampTime);

                Vector3 inter = new Vector3(interX.x, interY.y);

                if(checkBounds)
                    AdjustForBounds(ref inter);

                //float difInterX = Mathf.Abs(inter.x - target.position.x);
                //float difInterY = Mathf.Abs(inter.y - target.position.y);

                //if (difInterX < 0.0001f && difInterX < 0.0001f)
                //{
                //    inter = target.position;
                //}

                inter.z = -10;
                transform.position = inter;
            }
            else
            {
                Vector3 targetPos = target.transform.position;
                targetPos.z = -10;
                this.transform.position = targetPos;
            }
        }
    }


    void AdjustForBounds(ref Vector3 newPos)
    {
        //check for bounds
        if (xMin.HasValue && newPos.x < xMin)
            newPos.x = xMin.Value;
        else if (xMax.HasValue && newPos.x > xMax)
            newPos.x = xMax.Value;

        if (yMin.HasValue && newPos.y < yMin)
            newPos.y = yMin.Value;
        else if (yMax.HasValue && newPos.y > yMax)
            newPos.y = yMax.Value;
    }

    void RespawnBound()
    {
        //destroy current bound if it exist.
        if (bound != null)
            Destroy(bound);

        //Create bounding camera collider.
        bound = Instantiate(boundPrefab, target.position, Quaternion.identity);

        //calculate the current screen size.
        Vector2 camSize = GetOrthographicCameraSize();

        //rescale bound to fit the screen.
        bound.transform.localScale = new Vector3(camSize.x * 2, camSize.y * 2, bound.transform.localScale.z);

        //Make bound child of the target to make its position relative to it at all times. 
        //This is done after scaling to correct scale even if target has non-standard scale values.
        bound.transform.parent = target.transform;

        //delegate block responsibility
        CamBoundTriggerNew boundScript = bound.GetComponent<CamBoundTriggerNew>();
        boundScript.D_SetXMin = SetXMin;
        boundScript.D_SetXMax = SetXMax;
        boundScript.D_SetYMin = SetYMin;
        boundScript.D_SetYMax = SetYMax;

    }

    Vector2 GetOrthographicCameraSize()
    {
        float screenX = Camera.main.orthographicSize * Screen.width / Screen.height;
        float screenY = Camera.main.orthographicSize;
        return new Vector2(screenX, screenY);
    }

    #region boundSetters
    void SetXMin(float? x)
    {
        xMin = x.HasValue ? x + GetOrthographicCameraSize().x : null;
    }
    void SetXMax(float? x)
    {
        xMax = x.HasValue ? x - GetOrthographicCameraSize().x : null;
    }
    void SetYMin(float? y)
    {
        yMin = y.HasValue ? y + GetOrthographicCameraSize().y : null;
    }
    void SetYMax(float? y)
    {
        yMax = y.HasValue ? y - GetOrthographicCameraSize().y : null;
    }
    #endregion
}
