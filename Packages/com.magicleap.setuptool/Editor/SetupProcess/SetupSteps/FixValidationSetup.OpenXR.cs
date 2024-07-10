#if !USE_MLSDK
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using MagicLeap.SetupTool.Editor.Interfaces;
using MagicLeap.SetupTool.Editor.Utilities;
using UnityEditor;
using UnityEngine;


#if (OpenXR||USE_INPUT_SYSTEM_POSE_CONTROL)
using UnityEditor.XR.OpenXR;
using UnityEngine.XR.OpenXR;
using UnityEngine.XR.OpenXR.Features;
#endif

namespace MagicLeap.SetupTool.Editor.Setup
{
	public partial class FixValidationSetup: ISetupStep
	{

#if (OpenXR||USE_INPUT_SYSTEM_POSE_CONTROL)
		private static readonly List<OpenXRFeature.ValidationRule> _unityValidationFailures = new List<OpenXRFeature.ValidationRule>();
		private static List<OpenXRFeature.ValidationRule> _fixAllStack = new List<OpenXRFeature.ValidationRule>();
#endif

		private const string ADDITIONAL_STEP_LABEL = "Add Interaction Feature";
		private const string STEP_LABEL = "Apply XR Validation";
		private const string CONDITION_MISSING_TEXT = "Fix";
		private const string CONDITION_MET_LABEL = "Done";

		
		private bool _validationComplete;
		private bool _runningUnityValidationAutoFix;
		/// <inheritdoc />
		public Action OnExecuteFinished { get; set; }
		/// <inheritdoc />
		public bool Block => true;
		/// <inheritdoc />
		public bool Busy => _runningUnityValidationAutoFix;
		/// <inheritdoc />
		public bool IsComplete => _validationComplete;

		/// <inheritdoc />
		public bool CanExecute => MagicLeapPackageUtility.IsMagicLeapSDKInstalled &&  XRPackageUtility.HasSDKInstalled;

		private bool _hasInteractionFeature;
		private bool _automaticValidationStepsComplete;
		/// <inheritdoc />
		public bool Required => true;
		
		/// <inheritdoc />
		public void Refresh()
		{
#if (OpenXR||USE_INPUT_SYSTEM_POSE_CONTROL)
			OpenXRProjectValidation.GetCurrentValidationIssues(_unityValidationFailures, BuildTargetGroup.Android);
			_hasInteractionFeature = HasInteractionFeature();
			_automaticValidationStepsComplete = (NoValidationSteps()|| !_unityValidationFailures.Any(i => i.fixIt != null && i.fixItAutomatic));

			_validationComplete =_hasInteractionFeature && _automaticValidationStepsComplete;
		
	
#endif

		}
		private bool EnableGUI()
		{
			var correctBuildTarget = EditorUserBuildSettings.activeBuildTarget == BuildTarget.Android;
			var hasSdkInstalled = MagicLeapPackageUtility.IsMagicLeapSDKInstalled;
			return correctBuildTarget && hasSdkInstalled && XRPackageUtility.IsMagicLeapOpenXREnabled();

		}

		/// <inheritdoc />
		public bool Draw()
		{
			GUI.enabled = EnableGUI();
			if (!_automaticValidationStepsComplete)
			{
				if (CustomGuiContent.CustomButtons.DrawConditionButton(STEP_LABEL, _validationComplete,
					    CONDITION_MET_LABEL, CONDITION_MISSING_TEXT, Styles.FixButtonStyle))
				{

					Execute();
					return true;
				}
			}
			else
			{

				if (CustomGuiContent.CustomButtons.DrawConditionButton(
					    !_hasInteractionFeature ? ADDITIONAL_STEP_LABEL : STEP_LABEL, _validationComplete,
					    CONDITION_MET_LABEL, CONDITION_MISSING_TEXT, Styles.FixButtonStyle))
				{

					Execute();
					return true;
				}


			
			}
			return false;
		}

		/// <inheritdoc />
		public void Execute()
		{
			if (_runningUnityValidationAutoFix)
			{
				return;
			}

			if (_automaticValidationStepsComplete && !_hasInteractionFeature)
			{
				FixInteractionFeature();
				return;
			}

			RunAutoFix(BuildTargetGroup.Android);
		}

		public void FixInteractionFeature()
		{
			if (!XRPackageUtility.XRInteractionFeatureEnabled(BuildTargetGroup.Android,
				    "MagicLeapControllerProfile Android"))
			{
				XRPackageUtility.EnableXRInteractionFeature(BuildTargetGroup.Android,
					"MagicLeapControllerProfile Android");
				Debug.Log("Added [Magic Leap Controller Profile] to XR Interaction feature list.");
			}
		}

		public void RunAutoFix(BuildTargetGroup buildTargetGroup)
		{
			
#if (OpenXR||USE_INPUT_SYSTEM_POSE_CONTROL)
			_runningUnityValidationAutoFix = true;

			OpenXRProjectValidation.GetCurrentValidationIssues(_unityValidationFailures, buildTargetGroup);
			_fixAllStack = _unityValidationFailures.Where(i => i.fixIt != null && i.fixItAutomatic).ToList();
			EditorApplication.update += OnEditorUpdate;

#endif

		

		}

		private void OnEditorUpdate()
		{
			if (!_runningUnityValidationAutoFix)
			{
				EditorApplication.update -= OnEditorUpdate;
			}
#if (OpenXR||USE_INPUT_SYSTEM_POSE_CONTROL)
	
			if (_fixAllStack.Count > 0)
			{
				_fixAllStack[0].fixIt?.Invoke();
				_fixAllStack.RemoveAt(0);
			}

			if (_fixAllStack.Count == 0)
			{
				_runningUnityValidationAutoFix = false;
				FixInteractionFeature();
			}

#endif
		}


		public bool NoValidationSteps()
		{
#if (OpenXR||USE_INPUT_SYSTEM_POSE_CONTROL)
	return !_unityValidationFailures.Any(i => i.fixIt != null && i.fixItAutomatic);
#else
			return false;
#endif

		}

		public bool HasInteractionFeature()
		{
#if (OpenXR||USE_INPUT_SYSTEM_POSE_CONTROL)
			var settings = OpenXRSettings.GetSettingsForBuildTargetGroup(BuildTargetGroup.Android);
			return settings != null && settings.GetFeatures<OpenXRInteractionFeature>().Any(f => f.enabled);
#else
			return false;
#endif
		}




	}
}
#endif