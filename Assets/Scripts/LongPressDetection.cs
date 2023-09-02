using UnityEngine;
using UnityEngine.Events;

public class LongPressDetection : MonoBehaviour
{
    public float longPressDuration = 1.0f; // Adjust the duration as needed.
    public UnityEvent onLongPress;
    public UnityEvent onShortPress;

    private float touchStartTime = 0f;
    public bool isDetectingLongPress = false;
    
    void Update()
    {
        if (Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);

            switch (touch.phase)
            {
                case TouchPhase.Began:
                    touchStartTime = Time.time;
                    isDetectingLongPress = true;
                    break;

                case TouchPhase.Stationary:
                    if (isDetectingLongPress && Time.time - touchStartTime >= longPressDuration)
                    {
                        Debug.Log("Long Press FIRE!");
                        onLongPress.Invoke();
                        isDetectingLongPress = false;
                    }
                    break;

                case TouchPhase.Moved:
                case TouchPhase.Ended:
                    /*if (isDetectingLongPress && Time.time - touchStartTime < longPressDuration)
                    {
                        Debug.Log("Short Press FIRE!");
                        onShortPress.Invoke();
                    }
                    isDetectingLongPress = false;
                    break;*/
                case TouchPhase.Canceled:
                    isDetectingLongPress = false;
                    break;
            }
        }
    }
}