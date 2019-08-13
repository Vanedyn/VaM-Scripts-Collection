using System;
using UnityEngine;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using SimpleJSON;
using Valve.VR;

namespace OmniTriggerSpace
{
	public class OmniTrigger : MVRScript {

        protected JSONStorableStringChooser CameraReceiverJSON;
		protected List<string> getControllers(Atom receivingAtom){
			List<string> choices = new List<string>();
				if (receivingAtom != null){
					foreach (ForceReceiver fr in receivingAtom.forceReceivers) {
						choices.Add(fr.name);
					}
					FreeControllerV3[] freeControllers = receivingAtom.GetComponentsInChildren<FreeControllerV3>(true);
					foreach (var c in freeControllers){
						choices.Add(c.name);
					}
				}
			return choices;
		}
		protected void SyncAtomChoices() {
			List<string> atomChoices = new List<string>();
			atomChoices.Add("None");
			foreach (string atomUID in SuperController.singleton.GetAtomUIDs()) {
				atomChoices.Add(atomUID);
			}
			atomJSON.choices = atomChoices;
		}
		protected void SyncAtomChoices2() {
			List<string> atomChoices = new List<string>();
			atomChoices.Add("None");
			foreach (string atomUID in SuperController.singleton.GetAtomUIDs()) {
				atomChoices.Add(atomUID);
			}
			distanceAtomJSON.choices = atomChoices;
		}
		protected Atom receivingAtom;
		protected void SyncAtom(string atomUID) {
			List<string> receiverChoices = new List<string>();
			receiverChoices.Add("None");
			if (atomUID != null) {
				receivingAtom = SuperController.singleton.GetAtomByUid(atomUID);
				if (receivingAtom != null) {
					foreach (string receiverChoice in receivingAtom.GetStorableIDs()) {
						receiverChoices.Add(receiverChoice);
						//SuperController.LogMessage("Found receiver " + receiverChoice);
					}
				}
			} else {
				receivingAtom = null;
			}
			receiverJSON.choices = receiverChoices;
			receiverJSON.val = "None";
		}
		protected Atom distanceReceivingAtom;
		protected void SyncAtom2(string atomUID) {
			List<string> receiverChoices = new List<string>();
			receiverChoices.Add("None");
			if (atomUID != null) {
				distanceReceivingAtom = SuperController.singleton.GetAtomByUid(atomUID);
				if (distanceReceivingAtom != null) {
					foreach (ForceReceiver fr in distanceReceivingAtom.forceReceivers){
						receiverChoices.Add(fr.name);
					}
					FreeControllerV3[] freeControllers = distanceReceivingAtom.GetComponentsInChildren<FreeControllerV3>(true);
					foreach (var c in freeControllers){
						receiverChoices.Add(c.name);
					}
				}
			} else {
				distanceReceivingAtom = null;
			}
			distanceReceiverJSON.choices = receiverChoices;
			distanceReceiverJSON.val = "None";
		}
		protected JSONStorableStringChooser atomJSON;
		protected string _missingReceiverStoreId = "";
		protected void CheckMissingReceiver() {
			if (_missingReceiverStoreId != "" && receivingAtom != null) {
				JSONStorable missingReceiver = receivingAtom.GetStorableByID(_missingReceiverStoreId);
				if (missingReceiver != null) {
					//Debug.Log("Found late-loading receiver " + _missingReceiverStoreId);
					string saveTargetName = _receiverTargetName;
					SyncReceiver(_missingReceiverStoreId);
					_missingReceiverStoreId = "";
					insideRestore = true;
					receiverTargetJSON.val = saveTargetName;
					insideRestore = false;
				}
			}
		}
				protected JSONStorable receiver;
				protected void SyncReceiver(string receiverID) {
					List<string> receiverTargetChoices = new List<string>();
					receiverTargetChoices.Add("None");
					if (receivingAtom != null && receiverID != null) {
						receiver = receivingAtom.GetStorableByID(receiverID);
						if (receiver != null) {
							foreach (string floatParam in receiver.GetFloatParamNames()) {
								receiverTargetChoices.Add(floatParam);
							}
						} else if (receiverID != "None") {
							_missingReceiverStoreId = receiverID;
						}
					} else {
						receiver = null;
					}
					receiverTargetJSON.choices = receiverTargetChoices;
					receiverTargetJSON.val = "None";
				}
				protected JSONStorableStringChooser receiverJSON;
				protected string _receiverTargetName;
				protected JSONStorableFloat receiverTarget;
				protected void SyncReceiverTarget(string receiverTargetName) {
					_receiverTargetName = receiverTargetName;
					receiverTarget = null;
					if (receiver != null && receiverTargetName != null) {
						receiverTarget = receiver.GetFloatJSONParam(receiverTargetName);
						if (receiverTarget != null) {
							lowerValueJSON.min = receiverTarget.min;
							lowerValueJSON.max = receiverTarget.max;
							upperValueJSON.min = receiverTarget.min;
							upperValueJSON.max = receiverTarget.max;
							currentValueJSON.min = receiverTarget.min;
							currentValueJSON.max = receiverTarget.max;
							targetValueJSON.min = receiverTarget.min;
							targetValueJSON.max = receiverTarget.max;
							if (!insideRestore) {
								lowerValueJSON.val = receiverTarget.val;
								upperValueJSON.val = receiverTarget.max;
								currentValueJSON.val = receiverTarget.val;
								targetValueJSON.val = receiverTarget.val;
							}
						}
					}
				}
				protected JSONStorableStringChooser receiverTargetJSON;

				private Vector3 sp;
				private Vector3 startPos;
				private Vector3 oldPosition;

				List<string> KeyCodes = new List<string>() {"None","space","return","up","down","left","right","left shift","right shift","left ctrl","right ctrl","left alt","right alt","tab","backspace","escape","0","1","2","3","4","5","6","7","8","9","[0]","[1]","[2]","[3]","[4]","[5]","[6]","[7]","[8]","[9]","a","b","c","d","e","f","g","h","i","j","k","l","m","n","o","p","q","r","s","t","u","v","w","x","y","z","insert","delete","home","end","page up","page down","numlock","caps lock","scroll lock","pause","clear"};
				private Atom target;
				private float step;

				private JSONStorableFloat lengthSlider;
				private JSONStorableFloat strengthSlider;
				private JSONStorableFloat speedSlider;
				private JSONStorableFloat pumpValueJSON;

				protected JSONStorableBool increaseOnce;
				protected JSONStorableBool decreaseOnce;
				protected JSONStorableStringChooser keyIncrease;
				protected JSONStorableStringChooser keyDecrease;
				protected JSONStorableStringChooser keyPump;
				protected JSONStorableStringChooser keyRandom;
				protected JSONStorableBool keyContinuous;
				protected JSONStorableBool randomValue;
				protected JSONStorableBool pumpContinuously;
				protected JSONStorableBool loopPump;
				protected JSONStorableBool autoPump;
				protected JSONStorableBool disablePump;

				protected JSONStorableBool triggerByTrigger;
				protected JSONStorableBool triggerDownTrigger;
				protected JSONStorableBool triggerLockTrigger;
				protected JSONStorableBool pumpByTrigger;
				protected JSONStorableFloat triggerSensitivitySlider;
				protected JSONStorableStringChooser viveChoicesJSON;

				protected JSONStorableBool triggerByDistance;
				protected JSONStorableStringChooser distanceAtomJSON;
				protected JSONStorableStringChooser distanceReceiverJSON;
				protected JSONStorableFloat defMinDistSlider;
				protected JSONStorableFloat defDistSlider;

				private Dictionary<string, int> axisDict;
				protected JSONStorableBool triggerByAngle;
				protected JSONStorableStringChooser axisChooser;
				protected JSONStorableFloat angleMin;
				protected JSONStorableFloat angleMax;

				protected JSONStorableBool triggerBySpeed;
				protected JSONStorableFloat speedMax;

				protected JSONStorableStringChooser mathChooser1;
				protected JSONStorableStringChooser mathChooser2;
				protected JSONStorableStringChooser mathChooser3;
				protected JSONStorableStringChooser mathChooser4;
				protected JSONStorableStringChooser mathChooser5;

				protected JSONStorableBool allowOffLimits;
				protected JSONStorableFloat downSpeedJSON;
				protected JSONStorableFloat upSpeedJSON;
				protected JSONStorableFloat durationJSON;

				protected JSONStorableFloat lowerValueJSON;
				protected JSONStorableFloat upperValueJSON;
				protected JSONStorableFloat targetValueJSON;
				protected JSONStorableFloat currentValueJSON;
				protected UIDynamicButton speedController;
				protected JSONStorableBool pumpOnce;
				private UIDynamicPopup dp;
				private int currentLoop;
				private float toElapse;


		protected OmniTriggerSpace.SteamVR_Controller.Device RC;
		protected OmniTriggerSpace.SteamVR_Controller.Device LC;
		private GameObject rightHandController;
		private GameObject Rctrl;
		private GameObject leftHandController;
		private GameObject Lctrl;

		private void detectViveControllers()
		{
			GameObject Rctrl = GameObject.Find("Controller (right)");
			if (Rctrl != null)
			{
					SteamVR_TrackedObject Ro = Rctrl.GetComponent<SteamVR_TrackedObject>();
					RC = SteamVR_Controller.Input((int)Ro.index);
					rightHandController = Rctrl;
			}
			GameObject Lctrl = GameObject.Find("Controller (left)");
			if (Lctrl != null)
			{
					SteamVR_TrackedObject Ro = Lctrl.GetComponent<SteamVR_TrackedObject>();
					LC = SteamVR_Controller.Input((int)Ro.index);
					leftHandController = Lctrl;
			}
		}

		private void randomPump() {
			pumpValueJSON.val = UnityEngine.Random.Range(0f,1f);
		}

		public override void Init() {
			try
			{
				currentLoop = 0;
				toElapse = 0;
				axisDict = new Dictionary<string, int>();
				smoothLoops = new Dictionary<int, float>();
				oldPosition = containingAtom.transform.position;

				pumpOnce = new JSONStorableBool("Pump once", false, (bool on)=>{
					pumpOnce.val = false;
					toElapse = durationJSON.val;
				});
				RegisterBool(pumpOnce);

				randomValue = new JSONStorableBool("Random value", false, (bool on)=>{
					randomValue.val = false;
					randomPump();
				});
				RegisterBool(randomValue);

				increaseOnce = new JSONStorableBool("Increase once", false, (bool on)=>{
					increaseOnce.val = false;
					pumpValueJSON.val += upSpeedJSON.val/2;
				});
				RegisterBool(increaseOnce);

				decreaseOnce = new JSONStorableBool("Decrease once", false, (bool on)=>{
					decreaseOnce.val = false;
					pumpValueJSON.val -= downSpeedJSON.val/2;
				});
				RegisterBool(decreaseOnce);


				speedController = CreateButton("Pump", false);
				speedController.button.onClick.AddListener(delegate (){
						toElapse = durationJSON.val;
				});

				pumpValueJSON = new JSONStorableFloat("Pump value", 0f, 0f, 1f, false);
				RegisterFloat(pumpValueJSON);
				UIDynamicSlider dsi = CreateSlider(pumpValueJSON, false);
				dsi.defaultButtonEnabled = false;
				dsi.quickButtonsEnabled = false;
				targetValueJSON = new JSONStorableFloat("targetValue", 0f, 0f, 1f, false, false);
				currentValueJSON = new JSONStorableFloat("Current value", 0f, 0f, 1f, false, false);
				UIDynamicSlider ds = CreateSlider(currentValueJSON, false);
				ds.defaultButtonEnabled = false;
				ds.quickButtonsEnabled = false;

				CreateSpacer().height = 10f;
				disablePump = new JSONStorableBool("Disable pump up/down speeds", false);
				RegisterBool(disablePump);
				CreateToggle((disablePump), false);

				upSpeedJSON = new JSONStorableFloat("Pump up speed", 0.1f, 0f, 10f, true);
				RegisterFloat(upSpeedJSON);
				CreateSlider(upSpeedJSON, false);

				downSpeedJSON = new JSONStorableFloat("Pump down speed", 0.1f, 0f, 10f, true);
				RegisterFloat(downSpeedJSON);
				CreateSlider(downSpeedJSON, false);

				durationJSON = new JSONStorableFloat("Duration", 50f, 1f, 1000f, true);
				RegisterFloat(durationJSON);
				CreateSlider(durationJSON, false);

				keyIncrease = new JSONStorableStringChooser("Increase key", KeyCodes, "None", "Increase key");
				RegisterStringChooser(keyIncrease);
				dp = CreateScrollablePopup(keyIncrease,false);
				dp.popupPanelHeight = 960f;

				keyDecrease = new JSONStorableStringChooser("Decrease key", KeyCodes, "None", "Decrease key");
				RegisterStringChooser(keyDecrease);
				dp = CreateScrollablePopup(keyDecrease,false);
				dp.popupPanelHeight = 960f;

				keyContinuous = new JSONStorableBool("Increase/decrease continuous", true);
				RegisterBool(keyContinuous);
				CreateToggle((keyContinuous), false);

				keyPump = new JSONStorableStringChooser("Pump key", KeyCodes, "None", "Pump key");
				RegisterStringChooser(keyPump);
				dp = CreateScrollablePopup(keyPump,false);
				dp.popupPanelHeight = 960f;

				keyRandom = new JSONStorableStringChooser("Randomize key", KeyCodes, "None", "Randomize key");
				RegisterStringChooser(keyRandom);
				dp = CreateScrollablePopup(keyRandom,false);
				dp.popupPanelHeight = 960f;

				autoPump = new JSONStorableBool("Autopump when 0", false);
				RegisterBool(autoPump);
				CreateToggle((autoPump), false);

				loopPump = new JSONStorableBool("Loop / 0 when 1", false);
				RegisterBool(loopPump);
				CreateToggle((loopPump), false);

				pumpContinuously = new JSONStorableBool("Pump infinitely", false);
				RegisterBool(pumpContinuously);
				CreateToggle((pumpContinuously), false);

				CreateSpacer().height = 10f;

				List<string> choices = new List<string>();
				choices.Add("None");
				choices.Add("Reverse");
				choices.Add("Sin(x*\u03C0)");
				choices.Add("Cos(x*\u03C0)");
				choices.Add("Tan(x*\u03C0)");
				choices.Add("*2");
				choices.Add("x^2");
				choices.Add("*30*Mathf.Deg2Rad");

				mathChooser1 = new JSONStorableStringChooser("Math operation 1", choices, "None", "Math operation 1");
				mathChooser2 = new JSONStorableStringChooser("Math operation 2", choices, "None", "Math operation 2");
				mathChooser3 = new JSONStorableStringChooser("Math operation 3", choices, "None", "Math operation 3");
				mathChooser4 = new JSONStorableStringChooser("Math operation 4", choices, "None", "Math operation 4");
				mathChooser5 = new JSONStorableStringChooser("Math operation 5", choices, "None", "Math operation 5");
				RegisterStringChooser(mathChooser1);
				RegisterStringChooser(mathChooser2);
				RegisterStringChooser(mathChooser3);
				RegisterStringChooser(mathChooser4);
				RegisterStringChooser(mathChooser5);
				CreatePopup(mathChooser1, false);
				CreatePopup(mathChooser2, false);
				CreatePopup(mathChooser3, false);
				CreatePopup(mathChooser4, false);
				CreatePopup(mathChooser5, false);

				allowOffLimits = new JSONStorableBool("Disable pump/value limits for math", false);
				RegisterBool(allowOffLimits);
				CreateToggle((allowOffLimits), false);

				atomJSON = new JSONStorableStringChooser("atom", SuperController.singleton.GetAtomUIDs(), null, "Atom", SyncAtom);
				RegisterStringChooser(atomJSON);
				SyncAtomChoices();
				dp = CreateScrollablePopup(atomJSON,true);
				dp.popupPanelHeight = 1100f;
				dp.popup.onOpenPopupHandlers += SyncAtomChoices;

				receiverJSON = new JSONStorableStringChooser("receiver", null, null, "Receiver", SyncReceiver);
				RegisterStringChooser(receiverJSON);
				dp = CreateScrollablePopup(receiverJSON,true);
				dp.popupPanelHeight = 960f;

				receiverTargetJSON = new JSONStorableStringChooser("receiverTarget", null, null, "Target", SyncReceiverTarget);
				RegisterStringChooser(receiverTargetJSON);
				dp = CreateScrollablePopup(receiverTargetJSON,true);
				dp.popupPanelHeight = 820f;

				atomJSON.val = containingAtom.uid;
				receiverJSON.val = "geometry";

				lowerValueJSON = new JSONStorableFloat("lower value", 0f, 0f, 1f, false);
				RegisterFloat(lowerValueJSON);
				CreateSlider(lowerValueJSON, true);

				upperValueJSON = new JSONStorableFloat("upper value", 1f, 0.1f, 1f, false);
				RegisterFloat(upperValueJSON);
				CreateSlider(upperValueJSON, true);

				CreateSpacer(true).height = 10f;

				smoothLoopFactor = new JSONStorableFloat("Value smoothing factor", 20f, 1f, 100f, true);
				RegisterFloat(smoothLoopFactor);
				CreateSlider(smoothLoopFactor, true);

				smoothLoopFactorMult = new JSONStorableFloat("Smooth factor multiplier", 1f, 1f, 100f, true);
				RegisterFloat(smoothLoopFactorMult);
				CreateSlider(smoothLoopFactorMult, true);

				CreateSpacer(true).height = 10f;

				triggerByTrigger = new JSONStorableBool("Use vive trigger", false);
				RegisterBool(triggerByTrigger);
				CreateToggle((triggerByTrigger), true);

				List<string> viveChoices = new List<string>();
				viveChoices.Add("Trigger");
				viveChoices.Add("Touchpad X axis");
				viveChoices.Add("Touchpad Y axis");

				viveChoicesJSON = new JSONStorableStringChooser("Using", viveChoices, "Trigger", "Using");
				RegisterStringChooser(viveChoicesJSON);
				dp = CreateScrollablePopup(viveChoicesJSON,true);
				dp.popupPanelHeight = 960f;

				triggerDownTrigger = new JSONStorableBool("Use pump for downSpeed", false);
				RegisterBool(triggerDownTrigger);
				CreateToggle((triggerDownTrigger), true);

				pumpByTrigger = new JSONStorableBool("Trigger > 90% = Pump", false);
				RegisterBool(pumpByTrigger);
				CreateToggle((pumpByTrigger), true);

				triggerLockTrigger = new JSONStorableBool("Grip+Trigger = lock value", false);
				RegisterBool(triggerLockTrigger);
				CreateToggle((triggerLockTrigger), true);

				triggerSensitivitySlider = new JSONStorableFloat("Vive trigger sensitivity", 1f, 0.1f, 10f, true);
				triggerSensitivitySlider.storeType = JSONStorableParam.StoreType.Full;
				RegisterFloat(triggerSensitivitySlider);
				CreateSlider(triggerSensitivitySlider, true);

				UIDynamicButton detectViveButton = CreateButton("(re)Detect Controllers", true);
				detectViveButton.button.onClick.AddListener(delegate () {
						detectViveControllers();
				});
				detectViveControllers();

				CreateSpacer(true).height = 10f;

				CameraReceiverJSON = new JSONStorableStringChooser("Reference atom", getControllers(GetContainingAtom()), "head", "Local reference atom");
				RegisterStringChooser(CameraReceiverJSON);
				dp = CreateScrollablePopup(CameraReceiverJSON,true);
				dp.popupPanelHeight = 960f;

				triggerByDistance = new JSONStorableBool("Override pump by distance", false);
				RegisterBool(triggerByDistance);
				CreateToggle((triggerByDistance), true);

				distanceAtomJSON = new JSONStorableStringChooser("distant atom", SuperController.singleton.GetAtomUIDs(), "[CameraRig]", "Distant Atom", SyncAtom2);
				RegisterStringChooser(distanceAtomJSON);
				SyncAtomChoices2();
				dp = CreateScrollablePopup(distanceAtomJSON,true);
				dp.popupPanelHeight = 1100f;
				dp.popup.onOpenPopupHandlers += SyncAtomChoices2;

				distanceReceiverJSON = new JSONStorableStringChooser("distant receiver", null, "None", "Distant receiver");
				RegisterStringChooser(distanceReceiverJSON);
				dp = CreateScrollablePopup(distanceReceiverJSON,true);
				dp.popupPanelHeight = 960f;

				defMinDistSlider = new JSONStorableFloat("Minimum distance", 0.2f, 0, 3f, true);
				defMinDistSlider.storeType = JSONStorableParam.StoreType.Full;
				RegisterFloat(defMinDistSlider);
				CreateSlider(defMinDistSlider, true);

				defDistSlider = new JSONStorableFloat("Variation distance", 0.2f, 0, 3f, true);
				defDistSlider.storeType = JSONStorableParam.StoreType.Full;
				RegisterFloat(defDistSlider);
				CreateSlider(defDistSlider, true);

				CreateSpacer(true).height = 10f;

				triggerByAngle = new JSONStorableBool("Override pump by angle of reference", false);
				RegisterBool(triggerByAngle);
				CreateToggle((triggerByAngle), true);

				axisDict.Add("X", 0);
				axisDict.Add("Y", 1);
				axisDict.Add("Z", 2);

				axisChooser = new JSONStorableStringChooser("Axis", axisDict.Keys.ToList(), "X", "Axis");
				RegisterStringChooser(axisChooser);
				CreatePopup(axisChooser, true);

				angleMin = new JSONStorableFloat("lower angle", 0f, 0f, 360f, false);
				RegisterFloat(angleMin);
				CreateSlider(angleMin, true);

				angleMax = new JSONStorableFloat("upper angle", 0f, 0f, 360f, false);
				RegisterFloat(angleMax);
				CreateSlider(angleMax, true);

				CreateSpacer(true).height = 10f;

				triggerBySpeed = new JSONStorableBool("Override pump by speed measure", false);
				RegisterBool(triggerBySpeed);
				CreateToggle((triggerBySpeed), true);

				speedMax = new JSONStorableFloat("Speed sensitivity", 1f, 0f, 5f, false);
				RegisterFloat(speedMax);
				CreateSlider(speedMax, true);

				CreateSpacer(true).height = 900f;
			}

			catch (Exception e) {
				SuperController.LogError("Exception caught: " + e);
			}
		}

		protected Transform localReferentPosition() {
			FreeControllerV3 referenceFC3      = null;
			Rigidbody        referenceRB       = null;
			Transform          referencePosition;

			referenceFC3 = containingAtom.GetStorableByID(CameraReceiverJSON.val) as FreeControllerV3;
			if (referenceFC3 != null) {
				referencePosition = referenceFC3.transform;
			}
			else
			{
				referenceRB = containingAtom.rigidbodies.First(rb => rb.name == CameraReceiverJSON.val);
				referencePosition = referenceRB.transform;
			}

			if (referencePosition == null){
				referencePosition = containingAtom.mainController.transform;
			}
			return referencePosition;
		}

		protected float distanceVariation() {
			Vector3 headPos = localReferentPosition().position;
			Atom distAtom = null;
			FreeControllerV3 distReceiver = null;
			Rigidbody distReceiverRB = null;
			Vector3 camPos;
			if (distanceAtomJSON.val != "[CameraRig]")
				distAtom = SuperController.singleton.GetAtomByUid(distanceAtomJSON.val);
			if (distAtom != null)
				distReceiver = distAtom.GetStorableByID(distanceReceiverJSON.val) as FreeControllerV3;
			if (distAtom != null && distReceiver == null && distanceReceiverJSON.val != "None")
				distReceiverRB = distAtom.rigidbodies.First(rb => rb.name == distanceReceiverJSON.val);
			if (distReceiver != null){
				camPos  = distReceiver.transform.position;
			}
			else if (distReceiverRB != null){
				camPos  = distReceiverRB.transform.position;
			}else {
				camPos  = SuperController.singleton.lookCamera.transform.position;
			}
			float dist = Vector3.Distance(camPos, headPos) - defMinDistSlider.val;
			if (dist < defDistSlider.val){
				return 1-(1/defDistSlider.val*dist);
			}
			return 0;
		}

		protected float pumpPower(float upSpeed, float downSpeed, float pumpVal) {
			float speed;
			if(toElapse > 0){
				speed = upSpeed;
				if (!pumpContinuously.val)
					toElapse--;
			}else{
				speed = -downSpeed;
			}
			if (toElapse<=0){
				toElapse = 0;
			}

			if (!disablePump.val)
				return pumpVal+speed/100f;

			return pumpVal;
		}

		protected float minMax(float minV, float maxV, float curVal, bool unlimited = false)
		{
			float result = curVal * (maxV - minV) + minV ;

			if (result > maxV && !unlimited)
				result = maxV;

			if (result < minV && !unlimited)
					result = minV;

			return result;
		}

		protected JSONStorableFloat smoothLoopFactor;
		protected JSONStorableFloat smoothLoopFactorMult;
		private Dictionary<int, float> smoothLoops;

		protected float valueSmoother(float result){
			int smoothFactor = (int)Math.Ceiling(smoothLoopFactor.val*smoothLoopFactorMult.val);
			smoothLoops[currentLoop] = result;

			int divider = 0;
			float total = 0;
			for (int i = 0; i < smoothFactor; i++){
				if (!smoothLoops.ContainsKey(i)){
					smoothLoops[i] = result;
				}
				total = total+smoothLoops[i];
				divider++;
			}
			if (currentLoop < smoothFactor){
				currentLoop++;
			}else{
				currentLoop = 0;
			}
			return total/divider;
		}


		protected float mathOperations(float variator, string operationChoice){
			float mathResult;

			switch (operationChoice){
				case "Sin(x*\u03C0)":
				mathResult = (float)Math.Sin(variator * (float)Math.PI);
				break;
				case "Cos(x*\u03C0)":
				mathResult = (float)Math.Cos(variator * (float)Math.PI);
				break;
				case "Tan(x*\u03C0)":
				mathResult = (float)Math.Tan(variator * (float)Math.PI);
				break;
				case "*2":
				mathResult = 2 * variator;
				break;
				case "x^2":
				mathResult = variator*variator;
				break;
				case "*30*Mathf.Deg2Rad":
				mathResult = variator*30*Mathf.Deg2Rad;
				break;
				case "Reverse":
				mathResult = 1-variator;
				break;
				default:
				mathResult = minMax(0,1,variator,allowOffLimits.val);
				break;
			}
			return mathResult;
		}


		protected float AngleVariator(){
			Vector3 sourceAngle = localReferentPosition().eulerAngles;
			float currentAngle = sourceAngle[axisDict[axisChooser.val]];

			float angleResult;
			if (currentAngle>angleMax.val){
					angleResult = 1f;
				} else if (currentAngle<angleMin.val){
					angleResult = 0f;
				} else{
					angleResult = (1f/(angleMax.val-angleMin.val)) * (currentAngle - angleMin.val);
				}
			return angleResult;
		}

		protected float speedVariator(){
			Vector3 newPosition = localReferentPosition().position;
			if (oldPosition == null)
				oldPosition = newPosition;
			float speed = (((newPosition - oldPosition).magnitude) / Time.deltaTime);
			return speed*speedMax.val;
		}

		private float tempDown = 0;
		private int avoidRepeatingKey = 0;
		private bool lockTrigger = false;
		private float variator;

		protected void pluginFixedRefresh(){
			try{
				if (receiverTargetJSON.val != "None"){
					if (pumpContinuously.val)
						toElapse = durationJSON.val;
					if (triggerByTrigger.val){
						if (RC != null){
							float tempValue;
							switch (viveChoicesJSON.val){
								case "Trigger" :
								rViveTrigger = RC.GetAxis(Valve.VR.EVRButtonId.k_EButton_SteamVR_Trigger).x * triggerSensitivitySlider.val;
								break;
								case "Touchpad X axis" :
								tempValue = (RC.GetAxis(Valve.VR.EVRButtonId.k_EButton_Axis0).x+1)/2 * triggerSensitivitySlider.val;
								if (tempValue == 0.5f){
									rViveTrigger = pumpValueJSON.val;
								}else{
									rViveTrigger = tempValue;
								}
								break;
								case "Touchpad Y axis" :
								tempValue = (RC.GetAxis(Valve.VR.EVRButtonId.k_EButton_Axis0).y+1)/2 * triggerSensitivitySlider.val;
								if (tempValue == 0.5f){
									rViveTrigger = pumpValueJSON.val;
								}else{
									rViveTrigger = tempValue;
								}
								break;
							}
							rViveGrip    = RC.GetPressDown(Valve.VR.EVRButtonId.k_EButton_Grip);
						}else{
							rViveTrigger = 0;
							rViveGrip = false;
						}
					}

					if (triggerByTrigger.val && rViveTrigger > 0 && ((triggerDownTrigger.val && rViveTrigger >= pumpValueJSON.val) || (!triggerDownTrigger.val)) ){
						if (pumpByTrigger.val){
							if (rViveTrigger > 0.9f)
								toElapse = durationJSON.val;
						}else{
							variator = rViveTrigger;
						}
						if (rViveGrip && lockTrigger == false && avoidRepeatingKey <= 0){
							lockTrigger = true;
							avoidRepeatingKey = 5;
						}else if (rViveGrip && avoidRepeatingKey <= 0){
							lockTrigger = false;
							avoidRepeatingKey = 5;
						}
					}else if (triggerByDistance.val){
						variator = distanceVariation();
					}else if (triggerByAngle.val){
						variator = AngleVariator();
					}else if (triggerBySpeed.val){
						variator = speedVariator();
					}else{
						variator = pumpPower(upSpeedJSON.val, downSpeedJSON.val, pumpValueJSON.val);
					}
					// Math operations
					float mathResult;
					mathResult = mathOperations(variator,   mathChooser1.val);
					mathResult = mathOperations(mathResult, mathChooser2.val);
					mathResult = mathOperations(mathResult, mathChooser3.val);
					mathResult = mathOperations(mathResult, mathChooser4.val);
					mathResult = mathOperations(mathResult, mathChooser5.val);

					if (loopPump.val && variator >= 0.999f)
						variator = 0;
					if (autoPump.val && variator <= 0.001f)
						toElapse = durationJSON.val;
					// pump slider indicator
					pumpValueJSON.val = minMax(0,1,variator,allowOffLimits.val);
					// create value relative to target settings, variator is now not in between 0 or 1
					variator = minMax(lowerValueJSON.val,upperValueJSON.val,mathResult,allowOffLimits.val);
					// average of previous results for smoother values
					variator = valueSmoother(variator);
					CheckMissingReceiver();
					if (receiverTarget != null) {
						if ((lockTrigger && triggerLockTrigger.val) || !triggerLockTrigger.val)
							receiverTarget.val  = currentValueJSON.val = variator;
					}
					if (avoidRepeatingKey > 0){
						avoidRepeatingKey--;
					}

					if (triggerBySpeed.val)
						oldPosition = localReferentPosition().position;
				}
			}
			catch (Exception e){
					RC = null;
					SuperController.LogError("Exception caught: " + e);
			}
		}


		private float rViveTrigger;
		private bool rViveGrip;

		void pluginRefresh(){
			try{
				if (receiverTargetJSON.val != "None"){
					if (keyPump.val != "None"){
						if (Input.GetKeyDown(keyPump.val)){
							toElapse = durationJSON.val;
						}
					}

					if (keyRandom.val != "None"){
						if (Input.GetKeyDown(keyRandom.val)){
							randomPump();
						}
					}
					if (keyContinuous.val){
						if (keyIncrease.val != "None"){
							if (Input.GetKey(keyIncrease.val)){
								pumpValueJSON.val = minMax(0,1,pumpValueJSON.val+upSpeedJSON.val/10);
							}
						}
						if (keyDecrease.val != "None"){
							if (Input.GetKey(keyDecrease.val)){
								pumpValueJSON.val = minMax(0,1,pumpValueJSON.val-downSpeedJSON.val/10);
							}
						}
					}else{
						if (keyIncrease.val != "None"){
							if (Input.GetKeyDown(keyIncrease.val)){
								pumpValueJSON.val = minMax(0,1,pumpValueJSON.val+upSpeedJSON.val);
							}
						}
						if (keyDecrease.val != "None"){
							if (Input.GetKeyDown(keyDecrease.val)){
								pumpValueJSON.val = minMax(0,1,pumpValueJSON.val-downSpeedJSON.val);
							}
						}
					}
				}
			}
			catch (Exception e){
					SuperController.LogError("Exception caught: " + e);
			}
		}

		protected void FixedUpdate(){
			pluginFixedRefresh();
		}

		void Update(){
			pluginRefresh();
		}

	}
}
