//first release
//You can freely edit, distribute and steal part of this script without giving me credits, I will not get mad.
//Credits for this script: in the reddint page 

//to be able to move mouse cursor position we need this line below:
using System.Runtime.InteropServices;

using System;
using UnityEngine;
using System.Collections.Generic;


namespace MouseAndKeyboardMovement.tools
{

    public class MouseAndKeyboardMovement : MVRScript
    {
        #region slider variables and variables
        //to customize hotkeys, credits to the guy who made omniTrigger script
        List<string> KeyCodes = new List<string>() { "None", "mouse 0", "mouse 1", "mouse 2", "mouse 3", "mouse 4", "space", "left shift",
            "right shift", "left ctrl", "right ctrl", "left alt", "right alt", "tab", "backspace", "escape", "0", "1", "2", "3", "4", "5", "6", "7",
            "8", "9", "[0]", "[1]", "[2]", "[3]", "[4]", "[5]", "[6]", "[7]", "[8]", "[9]", "a", "b", "c", "d", "e", "f", "g", "h", "i", "j", "k",
            "l", "m", "n", "o", "p", "q", "r", "s", "t", "u", "v", "w", "x", "y", "z", "insert", "delete", "home", "end", "page up", "page down",
             "numlock", "caps lock", "scroll lock", "pause", "clear", "return", "up", "down", "left", "right" };
        private JSONStorableStringChooser grabKey;
        private JSONStorableStringChooser zAxisKey;
        private JSONStorableStringChooser enable2Key;
        private JSONStorableStringChooser enable1Key;
        private JSONStorableStringChooser focusKey;
        private JSONStorableStringChooser increaseKey;
        private JSONStorableStringChooser decreaseKey;
        private UIDynamicPopup dp;
        private JSONStorableFloat grabScale;
        private JSONStorableFloat stepHotkeyGrabScale;
        private JSONStorableFloat sensitivityY;
        private JSONStorableFloat pokeForce;
        private JSONStorableBool focusOn;
        private FreeControllerV3 oldSelectedController;
        //these 3 lines below are for windows mouse cursor trick, unityengine cannot move the cursor, so...
        [DllImport("user32.dll")]
        static extern bool SetCursorPos(int X, int Y);
        private Vector3 cursorPos;
        //to be able to comunicate variables between methods, functions and shit below here:
        private Atom clothGrabSphereSelected;
        private Vector3 screenPoint;
        private Vector3 offset;
        private Vector3 offTest;
        private Vector3 v;
        private bool controllerPositionWasOff = false;
        private bool grabbing = false;
        private bool grabbingAlt = false;
        private bool enablescriptgrabber = false;
        private bool secondStep = false;
        private bool thirdStep = false;
        private bool zAxisMode = false;
        private float moveZ = 0.0f;
        private bool rotatingCamera = false;
        #endregion 

        #region credits omniTrigger guy
        public override void Init()
        {
            try
            {
                enable1Key = new JSONStorableStringChooser("Hold key", KeyCodes, "left ctrl", "Hold key (needed for others hotkeys)");
                RegisterStringChooser(enable1Key);
                dp = CreateScrollablePopup(enable1Key, false);
                dp.popupPanelHeight = 960f;

                enable2Key = new JSONStorableStringChooser("Enable Script (needs Hold key)", KeyCodes, "b", "Enable Script (needs Hold key)");
                RegisterStringChooser(enable2Key);
                dp = CreateScrollablePopup(enable2Key, true);
                dp.popupPanelHeight = 960f;

                grabKey = new JSONStorableStringChooser("Grabbing key", KeyCodes, "g", "Grabbing key");
                RegisterStringChooser(grabKey);
                dp = CreateScrollablePopup(grabKey, false);
                dp.popupPanelHeight = 960f;

                focusKey = new JSONStorableStringChooser("Focus key", KeyCodes, "h", "Focus key");
                RegisterStringChooser(focusKey);
                dp = CreateScrollablePopup(focusKey, false);
                dp.popupPanelHeight = 960f;

                zAxisKey = new JSONStorableStringChooser("ZaxisModeKey", KeyCodes, "left alt", "ZaxisModeKey");
                RegisterStringChooser(zAxisKey);
                dp = CreateScrollablePopup(zAxisKey, false);
                dp.popupPanelHeight = 960f;

                grabScale = new JSONStorableFloat("Grab Sphere Size", 0.03f, 0.001f, 5f, true);
                RegisterFloat(grabScale);
                CreateSlider(grabScale, true);


                sensitivityY = new JSONStorableFloat("Sensitivity of Z axis", 0.01f, 0.05f, 0.5f, true);
                RegisterFloat(sensitivityY);
                CreateSlider(sensitivityY, true);

                pokeForce = new JSONStorableFloat("Poke Force when click", 0f, 0f, 1000f, true);
                RegisterFloat(pokeForce);
                CreateSlider(pokeForce, true);

                stepHotkeyGrabScale = new JSONStorableFloat("StepSizeBelowKeys", 0.1f, 0.01f, 1f, true);
                RegisterFloat(stepHotkeyGrabScale);
                CreateSlider(stepHotkeyGrabScale, false);

                increaseKey = new JSONStorableStringChooser("IncSizeKey(needs Hold key)", KeyCodes, "5", "IncSizeKey(needs Hold key)");
                RegisterStringChooser(increaseKey);
                dp = CreateScrollablePopup(increaseKey, false);
                dp.popupPanelHeight = 960f;

                decreaseKey = new JSONStorableStringChooser("DecSizeKey(needs Hold key)", KeyCodes, "4", "DecSizeKey(needs Hold key)");
                RegisterStringChooser(decreaseKey);
                dp = CreateScrollablePopup(decreaseKey, false);
                dp.popupPanelHeight = 960f;

            }
            catch (Exception e)
            {
                SuperController.LogError("Exception caught: " + e);
            }
        }
        #endregion

        #region Methods


        void TeleportClothGrabSphere()
        {

            //I don't understand these filters but the best working one is this
            int layerMask = 38 << 39;
            layerMask = ~layerMask;
            //why these layers? dont ask me, maybe 8 << 9 works to

            RaycastHit hit;
            Ray rayCam = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(rayCam, out hit, Mathf.Infinity, layerMask))
            {
                Vector3 v = hit.point;
                //poke force
                hit.rigidbody.AddForceAtPosition(rayCam.direction * pokeForce.val, hit.point);
                //set atom new position to collision position
                clothGrabSphereSelected.mainController.transform.position = v;
                //mouse cursor position stuff
                screenPoint = Camera.main.WorldToScreenPoint(clothGrabSphereSelected.mainController.transform.position);
                offset = clothGrabSphereSelected.mainController.transform.position - Camera.main.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, screenPoint.y, screenPoint.z));
                //this is a vector difference to be used as a directional vector from the camera to the object position
                offTest = clothGrabSphereSelected.mainController.transform.position - Camera.main.transform.position;
            }
        }
        void ZAxisMode()
        {
            
            //oldSelectedController = SuperController.singleton.GetSelectedController();

            if (rotatingCamera == false && grabbingAlt == true)
            {
                moveZ += Input.GetAxis("Mouse Y") * sensitivityY.val;
                if (Input.GetAxis("Mouse Y") != 0 && grabbingAlt == true)
                {
                    oldSelectedController.transform.position += offTest * moveZ;
                    
                    moveZ = 0.0f;
                }
            }
        }
        void OnMouseDrag()
        { 
            if (rotatingCamera == false && grabbing == true)
            {
                Vector3 cursorScreenPoint = new Vector3(Input.mousePosition.x, Input.mousePosition.y, screenPoint.z);
                Vector3 cursorPosition = Camera.main.ScreenToWorldPoint(cursorScreenPoint) + offset;
                if (grabbingAlt == false && secondStep == false && thirdStep == false)
                {
                    clothGrabSphereSelected.mainController.transform.position = new Vector3(cursorPosition.x, cursorPosition.y, cursorPosition.z);
                }
                if (grabbingAlt == true)
                {
                    moveZ += Input.GetAxis("Mouse Y") * sensitivityY.val;
                    if (Input.GetAxis("Mouse Y") != 0 && grabbingAlt == true)
                    {
                        clothGrabSphereSelected.mainController.transform.position += offTest * moveZ;
                        moveZ = 0.0f;

                    }
                }

                if (grabbingAlt == false && secondStep == true)
                {
                    screenPoint = Camera.main.WorldToScreenPoint(clothGrabSphereSelected.mainController.transform.position);
                    offset = clothGrabSphereSelected.mainController.transform.position - Camera.main.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, screenPoint.z));
                    thirdStep = true;

                    secondStep = false;
                }
                if (grabbingAlt == false && thirdStep == true)
                {
                    cursorScreenPoint = new Vector3(Input.mousePosition.x, Input.mousePosition.y, screenPoint.z);
                    cursorPosition = Camera.main.ScreenToWorldPoint(cursorScreenPoint) + offset;
                    clothGrabSphereSelected.mainController.transform.position = new Vector3(cursorPosition.x, cursorPosition.y, cursorPosition.z);

                }

            }

        }
        void FocusGrabSphere()
        {
            secondStep = true;
            if (SuperController.singleton.GetSelectedController() != null)
            {
                oldSelectedController = SuperController.singleton.GetSelectedController();
            }
            SuperController.singleton.SelectController(clothGrabSphereSelected.mainController, true);
            SuperController.singleton.FocusOnSelectedController();
            CursorToTransform();
            SuperController.singleton.SelectController(oldSelectedController);
            oldSelectedController = null;
        }
        void CursorToTransform()
        {
            cursorPos = screenPoint;
            SetCursorPos((int)cursorPos.x, (int)cursorPos.y);
        }
        #endregion

        #region repeatedly executed part of script
        //probably is better to use some stuff on FixedUpdate or LateUpdate, but that is too advanced for me
        void Update()
        {
            try
            {
                if (Input.GetKey(enable1Key.val))
                {
                    if (Input.GetKeyDown(enable2Key.val))
                    {
                        grabbingAlt = false;
                        grabbing = false;
                        rotatingCamera = false;
                        secondStep = false;
                        thirdStep = false;
                        foreach (Atom atom in SuperController.singleton.GetAtoms())
                        {
                            if (atom.name == "ClothGrabSphere")
                            {
                                clothGrabSphereSelected = atom;
                                clothGrabSphereSelected.containingAtom.GetStorableByID("scale").SetFloatParamValue("scale", 0.03f);
                                clothGrabSphereSelected.SetOn(false);
                                clothGrabSphereSelected.mainController.currentPositionState = FreeControllerV3.PositionState.On;
                            }
                        }
                        enablescriptgrabber = !enablescriptgrabber;
                    }

                    if (Input.GetKeyDown(increaseKey.val) && enablescriptgrabber == true)
                    {
                        grabScale.val += stepHotkeyGrabScale.val;
                        clothGrabSphereSelected.containingAtom.GetStorableByID("scale").SetFloatParamValue("scale", grabScale.val);
                        return;
                    }

                    if (Input.GetKeyDown(decreaseKey.val)  && enablescriptgrabber == true)
                    {
                        grabScale.val -= stepHotkeyGrabScale.val;
                        clothGrabSphereSelected.containingAtom.GetStorableByID("scale").SetFloatParamValue("scale", grabScale.val);
                        return;
                    }
                }

                if (Input.GetKeyDown(grabKey.val) && enablescriptgrabber == true)
                {
                    
                    TeleportClothGrabSphere();
                    grabbing = true;
                    clothGrabSphereSelected.SetOn(true);
                    clothGrabSphereSelected.containingAtom.GetStorableByID("scale").SetFloatParamValue("scale", grabScale.val);
                    return;
                }

                if (Input.GetKeyUp(grabKey.val) && enablescriptgrabber == true)
                {
                    //when hotkey releases, disables all variables
                    grabbing = false;
                    clothGrabSphereSelected.SetOn(false);
                    grabbingAlt = false;
                    secondStep = false;
                    thirdStep = false;
                    rotatingCamera = false;
                    return;
                }


                if (Input.GetKeyDown(zAxisKey.val) && enablescriptgrabber == true && grabbing == true)
                {
                    //With this you can release zAxisKey, rotate the camera, move the mouse, and press zAxisKey again with no axis problems (in a spheric way)
                    offTest = clothGrabSphereSelected.mainController.transform.position - Camera.main.transform.position;

                        if (thirdStep == true)
                        {
                            thirdStep = false;
                        }
                        grabbingAlt = true;
                        return;

                }
                //when zAxisKey key is released:
                //this disables the Z translation by moving the  mouseY and activates the second step script to be able to continue moving normally (after releasing alt)
                if (Input.GetKeyUp(zAxisKey.val) && enablescriptgrabber == true && grabbing == true)
                {
                    CursorToTransform();
                    grabbingAlt = false;
                    secondStep = true;
                    return;
                }

                if (Input.GetKeyDown(KeyCode.Mouse1) && enablescriptgrabber == true && grabbing == true)
                {

                    //With this you can release zAxisKey, rotate the camera, move the mouse, and press alt again with no axis problems (in a spheric way)
                    rotatingCamera = true;
                    //wee need to declare a second step, because the mouse cursor position will be different from the object screen position
                    secondStep = true;
                    return;
                }
                if (Input.GetKeyUp(KeyCode.Mouse1) && enablescriptgrabber == true && grabbing == true)
                {
                    CursorToTransform();
                    rotatingCamera = false;
                    return;
                }
                if (Input.GetKeyDown(focusKey.val) && grabbing == true && enablescriptgrabber == true)
                {
                    FocusGrabSphere();
                    return;
                }
               
                if (Input.GetKeyDown(zAxisKey.val) && enablescriptgrabber == true && grabbing == false)
                {                     
                    foreach (FreeControllerV3 highlightedController in SuperController.singleton.GetAllFreeControllers())
                    {
                        if (highlightedController.highlighted && highlightedController.currentPositionState == FreeControllerV3.PositionState.Off)
                        {
                            oldSelectedController = highlightedController;
                            oldSelectedController.currentPositionState = FreeControllerV3.PositionState.On;
                            controllerPositionWasOff = true;
                        }
                        if (highlightedController.highlighted && highlightedController.currentPositionState != FreeControllerV3.PositionState.Off)
                        {
                            oldSelectedController = highlightedController;
                            screenPoint = Camera.main.WorldToScreenPoint(oldSelectedController.transform.position);
                            offset = oldSelectedController.transform.position - Camera.main.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, screenPoint.y, screenPoint.z));
                            offTest = oldSelectedController.transform.position - Camera.main.transform.position;
                            grabbingAlt = true;
                            zAxisMode = true;

                        }

                    }
                    return;
                }
                if (Input.GetKeyUp(zAxisKey.val) && enablescriptgrabber == true && grabbing == false)
                {

                    grabbingAlt = false;
                    zAxisMode = false;
                    if (controllerPositionWasOff == true)
                    {
                        oldSelectedController.currentPositionState = FreeControllerV3.PositionState.Off;
                        controllerPositionWasOff = false;
                        return;
                    }
                    return;

                }
                //this is a bucle, THIS MUST BE ONE OF THE LATEST FUNCTION under update() method
                if (grabbing == false && zAxisMode == true && enablescriptgrabber == true)
                {
                    ZAxisMode();
                    return;
                }
                //this is a bucle, THIS MUST BE THE LAST FUNCTION under update() method
                if (grabbing == true && enablescriptgrabber == true)
                {
                    OnMouseDrag();
                    return;
                }

            }
            catch (Exception e)
            {
                SuperController.LogError("Exception caught: " + e);
            }
        }
        #endregion

    }
}