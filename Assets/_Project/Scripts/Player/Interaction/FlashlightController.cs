﻿using System;
using UnityEngine;

namespace ThePatient
{
    public class FlashlightController : MonoBehaviour
    {

        [Header("References")]
        [SerializeField] GameObject flashlightObject;
        [SerializeField] InputReader _input;
        [SerializeField] CameraController cameraController;
        [SerializeField] Transform flashlightTransform;

        [Header("Flashlight Settings")]
        [SerializeField] float maxRotX = 40f;
        [SerializeField] float maxRotY = 20f;
        [SerializeField] float rotateSmooth = 5f;
 
        float mouseX = 0;
        float mouseY = 0;

        IPickupable flashlight;

        private void Start()
        {
            if(flashlightObject != null)
            {
                flashlight = flashlightObject.GetComponent<IPickupable>();
            }
        }

        private void OnEnable()
        {
            _input.Flashlight += ToggleFlashlight;
            _input.Look += FlashlightLook;
        }


        private void OnDisable()
        {
            _input.Flashlight -= ToggleFlashlight;
            _input.Look -= FlashlightLook;
        }

        private void ToggleFlashlight()
        {
            if (Inventory.Instance.HasItem(flashlight))
            {
                flashlightTransform.gameObject.SetActive(!flashlightTransform.gameObject.activeSelf);
            }
        }
        private void FlashlightLook(Vector2 lookInput, bool isDeviceMouse)
        {
            if (!flashlightTransform.gameObject.activeSelf) return;

            float _gamepadMultiplier = 20f;
            //Get the device multiplier
            float deviceMultiplier = isDeviceMouse ? Time.fixedDeltaTime : Time.deltaTime * _gamepadMultiplier;

            //input based on device currently active
            float flashlightrotateMultiplier = 1.5f;
            mouseX += lookInput.x * deviceMultiplier * flashlightrotateMultiplier;
            mouseY -= lookInput.y * deviceMultiplier * flashlightrotateMultiplier;

            mouseX = Mathf.Clamp(mouseX, -maxRotX, maxRotX);

            mouseY = Mathf.Clamp(mouseY, -maxRotY, maxRotY);

            if(mouseX >= .9f * maxRotX || mouseX <= .9f * -maxRotX || mouseY >= .9f * maxRotY || mouseY <= .9f * -maxRotY)
            {
                cameraController.CameraLook(lookInput, isDeviceMouse);
            }
            else
            {
                cameraController.CameraLook(Vector2.zero, isDeviceMouse);
            }

            var targetRotation = Quaternion.Euler(Vector3.up * mouseX) * Quaternion.Euler(Vector3.right * mouseY);

            flashlightTransform.localRotation = Quaternion.Lerp(flashlightTransform.localRotation, targetRotation, rotateSmooth * Time.deltaTime);
        }

    }
}
