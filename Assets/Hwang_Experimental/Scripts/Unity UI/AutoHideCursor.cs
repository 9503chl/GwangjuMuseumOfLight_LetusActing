using System;
using UnityEngine;

namespace UnityEngine.UI
{
    [DisallowMultipleComponent]
    public class AutoHideCursor : MonoBehaviour
    {
        [Range(0f, 60f)]
        public float HideCursorDelay = 5f;

        [Tooltip("Shows the cursor when any key is input.\nSet this value to false to ignore button input from game controllers.")]
        public bool ShowByAnyKey = true;

        [NonSerialized]
        private bool isHideCursor = false;

        [NonSerialized]
        private float inputlessTime = 0f;

        [NonSerialized]
        private Vector2 lastMousePosition = Vector2.zero;

        private void OnDisable()
        {
            inputlessTime = 0f;
            if (isHideCursor)
            {
                isHideCursor = false;
                Cursor.visible = true;
            }
        }

        private void Update()
        {
            if ((ShowByAnyKey && Input.anyKey) || DidMouseMoveOrWheelScroll())
            {
                inputlessTime = 0f;
                if (isHideCursor)
                {
                    isHideCursor = false;
                    Cursor.visible = true;
                }
            }
            else if (!isHideCursor)
            {
                inputlessTime += Time.unscaledDeltaTime;
                if (inputlessTime > HideCursorDelay)
                {
                    isHideCursor = true;
                    Cursor.visible = false;
                }
            }
        }

        private bool DidMouseMoveOrWheelScroll()
        {
            Vector2 mouseMovement = (Vector2)Input.mousePosition - lastMousePosition;
            lastMousePosition = Input.mousePosition;
            return Mathf.Abs(mouseMovement.x) >= 2f || Mathf.Abs(mouseMovement.y) >= 2f || Input.mouseScrollDelta != Vector2.zero;
        }
    }
}
