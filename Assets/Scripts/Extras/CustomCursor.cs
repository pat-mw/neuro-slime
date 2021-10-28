
using UnityEngine;
using System.Collections;
using Sirenix.OdinInspector;

public class CustomCursor : SerializedMonoBehaviour
{
    public enum Mode
    {
        Exact,
        Lazy
    }

    public Transform cursorImage;

    public Mode cursorMode = Mode.Exact;

    public bool isActive = true;

    public bool disableRealCursor = false;

    [Range(3, 20)] public float LazyCursorSpeed = 10;

    bool isLazy()
    {
        if (cursorMode == Mode.Lazy)
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    private void OnEnable()
    {

        EnableCursor();
    }


    private void OnDisable()
    {
        isActive = false;
    }

    public void EnableCursor()
    {
        isActive = true;
    }

    public void DisableCursor()
    {
        isActive = false;
    }

    private void Update()
    {
        // update the custom cursor by reading cursor position

        Vector3 screenPoint = Input.mousePosition;
        screenPoint.z = 10.0f; //distance of the plane from the camera
        //cursorImage.position = Camera.main.ScreenToWorldPoint(screenPoint);

        switch (cursorMode)
        {
            case Mode.Exact:
                cursorImage.position = Input.mousePosition;
                break;
            case Mode.Lazy:
                cursorImage.position = Vector3.Lerp(cursorImage.position, Input.mousePosition, Time.deltaTime * LazyCursorSpeed);
                break;
        }
       
        if (disableRealCursor)
        {
            if (mouseOverViewport(Camera.main, Camera.main))
            {
                Cursor.visible = false;
                return;
            }

            Cursor.visible = true;
        }
    }

    /*
         * Does viewport of the "local_cam" camera, which is inside the "main_cam" camera
         * currently contain the mouse?
         */
    bool mouseOverViewport(Camera main_cam, Camera local_cam)
    {
        if (!Input.mousePresent) return true; //always true if no mouse??

        Vector3 main_mou = main_cam.ScreenToViewportPoint(Input.mousePosition);
        return local_cam.rect.Contains(main_mou);
    }

}