﻿using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayerController : MonoBehaviour
{
    enum Platform
    {
        Windows,
        Android,
        IOS
    }
    [SerializeField] private Platform curentSystemPlatform;
    [SerializeField] private HandleWindows handleWindows;

    //UI labels
    [SerializeField] private GameObject textRings;

    [SerializeField] private float speedOfRotate;
    [SerializeField] private float speedOfForwardMovement;
    [SerializeField] private Vector2 maxSpeed;
    [SerializeField] private Vector2 minSpeed;

    private IHandleInput handleInput;
    private Thrust thrust;
    private Rigidbody rb;

    private float currentRotateDirection = 0.0f;
    private bool bounce = false;


    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.tag == "Bounce")
        {
            // Debug.Log("Bounce !");
            if (textRings)
            {
                textRings.GetComponent<TMPro.TextMeshProUGUI>().text = "Rings: " + ProceduralGeneration.instance.GetBallRing();
            }
            Puddle puddle = collision.gameObject.GetComponent<Puddle>();
            if (puddle)
            {
                puddle.AffectBall(gameObject);
            }
            bounce = true;
        }
        else if (collision.gameObject.tag == "Trampoline")
        {
            collision.gameObject.GetComponentInParent<Trampoline>().BumpBall(gameObject);
            // Debug.Log("Trampoline !");
            if (textRings)
            {
                textRings.GetComponent<TMPro.TextMeshProUGUI>().text = "Rings: " + ProceduralGeneration.instance.GetBallRing();
            }
            bounce = true;
        }
    }

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        handleWindows = GetComponent<HandleWindows>();

        thrust = GetComponent<Thrust>();

        if (curentSystemPlatform == Platform.Windows)
        {
            handleInput = handleWindows;
        }
    }

    void ManageInput()
    {
        if (handleInput.IsLeft())
        {
            currentRotateDirection -= speedOfRotate * Time.deltaTime * handleInput.GetSingleAngleRotate();
            if (currentRotateDirection < 0)
            {
                currentRotateDirection += 2 * Mathf.PI;
            }
        }
        else if (handleInput.IsRight())
        {
            currentRotateDirection += speedOfRotate * Time.deltaTime * handleInput.GetSingleAngleRotate();
            if (currentRotateDirection > 2 * Mathf.PI)
            {
                currentRotateDirection -= 2 * Mathf.PI;
            }
        }
        else if (handleInput.IsThrust())
        {
            float force = thrust.ToThrust();
            //rb.AddRelativeForce(force);
            transform.Translate(Vector3.forward * force);
        }
    }

    public float GetSpeedOfForwardMovement()
    {
        return speedOfForwardMovement;
    }

    public void IncreaseSpeedOfForwardMovement(float value)
    {
        if (speedOfForwardMovement + value < maxSpeed.x && 
            speedOfForwardMovement + value > minSpeed.x)
        {
            speedOfForwardMovement += value;
        }
        else if (speedOfForwardMovement + value > maxSpeed.x)
        {
            speedOfForwardMovement = maxSpeed.x;
        }
        else if (speedOfForwardMovement + value > minSpeed.x)
        {
            speedOfForwardMovement = minSpeed.x;
        }
    }

    public Vector3 getBallVelocity()
    {
        return rb.velocity;
    }

    void Update()
    {
        ManageInput();
        transform.eulerAngles = new Vector3(0, currentRotateDirection * (180 / Mathf.PI), 0);

        // Movement 
        rb.velocity = new Vector3(
            transform.forward.x * speedOfForwardMovement,
            rb.velocity.y,
            transform.forward.z * speedOfForwardMovement);

        bounce = false;
    }
}
