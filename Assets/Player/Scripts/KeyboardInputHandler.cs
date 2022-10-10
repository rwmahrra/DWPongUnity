using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KeyboardInputHandler : MonoBehaviour
{

    public bool driveFromMQTT = false;

    public float speed = 0.6f; // reflect value in original of 6px velocity per tick 
    public float maxOffset = 8.1f; // (game width / 2 - paddle width / 2)

    // abstracting buttons allows for 2 controllers on the same keyboard
    public KeyCode leftButton = KeyCode.LeftArrow;
    public KeyCode rightButton = KeyCode.RightArrow;

    private Transform myTransform;
    private float MQTTInput;

    public MQTTReceiver _eventSender;

    // Start is called before the first frame update
    void Start()
    {

        // putting this here for now
        Application.targetFrameRate = 60;
        myTransform = gameObject.transform;

        if (_eventSender == null)
        {
            _eventSender = GetComponent<MQTTReceiver>();
        }
        _eventSender.OnConnectionSucceeded += OnConnectionSucceedHandler;
        _eventSender.OnMessageArrived += OnMessageArrivedHandler;
    }

    private void OnConnectionSucceedHandler(bool success)
    {
        //Debug.Log("Event Fired. Connected = " + success);
    }

    private void OnMessageArrivedHandler(string newMsg)
    {
        //Debug.Log("Event Fired. The message is = " + newMsg);
        MQTTInput = float.Parse(newMsg);
    }

    // Update is called once per frame
    void Update()
    {
        // looking for specific keycodes is rudimentary and Unity's Input system is a better way to handle this
        // but all control will eventually be networked so this script is only for prototyping.

        if (!driveFromMQTT)
        {
            //drive position from listening to key presses
            HandleKeyInput();
        }
        else
        {
            HandleMQTTInput();
        }
    }

    private void HandleKeyInput()
    {

        var buttonDown = false;
        float newSpeed = 0.0f;
        if (Input.GetKey(rightButton))
        {
            newSpeed += speed;
            buttonDown = true;
        }

        if (Input.GetKey(leftButton))
        {
            newSpeed += -speed;
            buttonDown = true;
        }

        if (buttonDown)
        {
            Vector3 newPosition = myTransform.position;
            newPosition.x += newSpeed;
            newPosition.x = Mathf.Clamp(newPosition.x, -maxOffset, maxOffset);
            myTransform.position = newPosition;

            if (_eventSender.isConnected)
            {
                //Debug.Log("Sending paddle position");
                _eventSender.Publish("paddle/position", myTransform.position.x.ToString()); // sending current position
            }
        }
    }

    private void HandleMQTTInput()
    {
        Vector3 newPosition = myTransform.position;
        newPosition.x = MQTTInput;
        myTransform.position = newPosition;

    }
}
