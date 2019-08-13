using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using SimpleJSON;

namespace MVRPlugin
{
    public class AllowMouseAndKeyboardOnVR : MVRScript
    {
        bool noControllerMode;
        bool notAimingAtHUD;
        private void DoAllowMouse()
        {
                Input.GetMouseButtonDown(1);
                Input.GetMouseButtonUp(1);
                if (Input.GetMouseButton(1))
                {
                    Vector3 vector = SuperController.singleton.MonitorCenterCamera.transform.position + SuperController.singleton.MonitorCenterCamera.transform.forward * SuperController.singleton.focusDistance;
                    float axis = Input.GetAxis("Mouse X");
                    if (axis > 0.01f || axis < -0.01f)
                    {
                        SuperController.singleton.navigationRig.RotateAround(vector, SuperController.singleton.navigationRig.up, axis * 2f);
                    }
                    float axis2 = Input.GetAxis("Mouse Y");
                    if ((axis2 > 0.01f || axis2 < -0.01f) && SuperController.singleton.MonitorCenterCamera != null)
                    {
                        Vector3 position = SuperController.singleton.MonitorCenterCamera.transform.position;
                        Vector3 up = SuperController.singleton.navigationRig.up;
                        Vector3 a = position - up * axis2 * 0.1f * SuperController.singleton.focusDistance - vector;
                        a.Normalize();
                        Vector3 vector2 = vector + a * SuperController.singleton.focusDistance - position;
                        Vector3 vector3 = SuperController.singleton.navigationRig.position + vector2;
                        float num = Vector3.Dot(vector2, up);
                        vector3 += up * -num;
                        SuperController.singleton.navigationRig.position = vector3;
                        SuperController.singleton.playerHeightAdjust += num;
                        if (SuperController.singleton.MonitorCenterCamera != null)
                        {
                            SuperController.singleton.MonitorCenterCamera.transform.LookAt(vector);
                            Vector3 localEulerAngles = SuperController.singleton.MonitorCenterCamera.transform.localEulerAngles;
                            localEulerAngles.y = 0f;
                            localEulerAngles.z = 0f;
                            SuperController.singleton.MonitorCenterCamera.transform.localEulerAngles = localEulerAngles;
                        }
                    }
                }
                else if (Input.GetMouseButton(2))
                {
                    float axis3 = Input.GetAxis("Mouse X");
                    Vector3 vector4 = SuperController.singleton.navigationRig.position;
                    if (axis3 > 0.01f || axis3 < -0.01f)
                    {
                        vector4 += SuperController.singleton.MonitorCenterCamera.transform.right * -axis3 * 0.03f;
                    }
                    float axis4 = Input.GetAxis("Mouse Y");
                    if (axis4 > 0.01f || axis4 < -0.01f)
                    {
                        vector4 += SuperController.singleton.MonitorCenterCamera.transform.up * -axis4 * 0.03f;
                    }
                    Vector3 up2 = SuperController.singleton.navigationRig.up;
                    float num2 = Vector3.Dot(vector4 - SuperController.singleton.navigationRig.position, up2);
                    vector4 += up2 * -num2;
                    SuperController.singleton.navigationRig.position = vector4;
                    SuperController.singleton.playerHeightAdjust += num2;
                }
                float y = Input.mouseScrollDelta.y;
                if (!notAimingAtHUD && (y > 0.5f || y < -0.5f))
                {
                    float num3 = 0.1f;
                    if (y < -0.5f)
                    {
                        num3 = -num3;
                    }
                    Vector3 forward = SuperController.singleton.MonitorCenterCamera.transform.forward;
                    Vector3 vector5 = num3 * forward * SuperController.singleton.focusDistance;
                    Vector3 vector6 = SuperController.singleton.navigationRig.position + vector5;
                    SuperController.singleton.focusDistance *= 1f - num3;
                    Vector3 up3 = SuperController.singleton.navigationRig.up;
                    float num4 = Vector3.Dot(vector5, up3);
                    vector6 += up3 * -num4;
                    SuperController.singleton.navigationRig.position = vector6;
                    SuperController.singleton.playerHeightAdjust += num4;
                }
            
        }

        private void DoAllowKeyboard()
        {
            if (LookInputModule.singleton == null || !LookInputModule.singleton.inputFieldActive)
            {
                
                if (Input.GetKeyDown(KeyCode.E))
                {
                    SuperController.singleton.gameMode = SuperController.GameMode.Edit;
                }
                if (Input.GetKeyDown(KeyCode.P))
                {
                    SuperController.singleton.gameMode = SuperController.GameMode.Play;
                }
                if (Input.GetKeyDown(KeyCode.U))
                { 
                    SuperController.singleton.ToggleMainHUDMonitor();
                }
              
                if (Input.GetKeyDown(KeyCode.H))
                {
                    SuperController.singleton.ToggleShowHiddenAtoms();
                }
                if (Input.GetKeyDown(KeyCode.Tab))
                {
                    SuperController.singleton.SelectModeFreeMoveMouse();
                }
                if (Input.GetKeyDown(KeyCode.T))
                {
                    SuperController.singleton.ToggleTargetsOnWithButton();
                }
                if (Input.GetKeyDown(KeyCode.R))
                {
                    SuperController.singleton.ResetFocusPoint();
                }

            }
        }
        private void DoCheckIfAimingHUD()
        {
            
            if (LookInputModule.singleton != null)
            {
                if (SuperController.singleton.useLookSelect)
                {
                    LookInputModule.singleton.referenceCamera = SuperController.singleton.lookCamera;
                    LookInputModule.singleton.ProcessMain();
                }
                LookInputModule.singleton.referenceCamera = SuperController.singleton.MonitorCenterCamera;
                LookInputModule.singleton.ProcessMouseAlt();
                notAimingAtHUD = LookInputModule.singleton.mouseRaycastHit;
            }
        }

            
        

        protected void Update()
        {
            try
            {
                if (Input.GetKeyDown(KeyCode.L) && !noControllerMode)
                {
                    SuperController.singleton.ShowMainHUD(true, true);
                    noControllerMode = true;
                }
                else if (Input.GetKeyDown(KeyCode.L) && noControllerMode)
                {
                    SuperController.singleton.ShowMainHUD(true, false);
                    noControllerMode = false;
                }

                if (noControllerMode)
                { 
                    SuperController.singleton.focusDistance = 0.01f;
                    DoCheckIfAimingHUD();
                    DoAllowKeyboard();
                    DoAllowMouse();
                }
            }
            catch (Exception ex)
            {
                SuperController.LogError("Something went wrong: " + ex);
            }

        }

  

    }
}