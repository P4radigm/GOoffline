using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using TMPro;

namespace UnityEngine.XR.ARFoundation.Samples
{
    /// <summary>
    /// Populates a drop down UI element with all the supported
    /// camera configurations and changes the active camera
    /// configuration when the user changes the selection in the dropdown.
    ///
    /// The camera configuration affects the resolution (and possibly framerate)
    /// of the hardware camera during an AR session.
    /// </summary>
    [RequireComponent(typeof(TMP_Dropdown))]
    public class CameraConfigController : MonoBehaviour
    {
        List<string> m_ConfigurationNames;

        TMP_Dropdown m_Dropdown;

        [SerializeField]
        [Tooltip("The ARCameraManager which will produce frame events.")]
        ARCameraManager m_CameraManager;

        /// <summary>
        /// Get or set the <c>ARCameraManager</c>.
        /// </summary>
        public ARCameraManager cameraManager
        {
            get => m_CameraManager;
            set => m_CameraManager = value;
        }

        private SettingsManager settingsManager;

        /// <summary>
        /// Callback invoked when <see cref="m_Dropdown"/> changes. This
        /// lets us change the camera configuration when the user changes
        /// the selection in the UI.
        /// </summary>
        /// <param name="dropdown">The <c>Dropdown</c> which changed.</param>
        public void OnDropdownValueChanged(TMP_Dropdown dropdown)
        {
            if ((cameraManager == null) || (cameraManager.subsystem == null) || !cameraManager.subsystem.running)
            {
                return;
            }

            var configurationIndex = dropdown.value;

            // Check that the value makes sense
            using (var configurations = cameraManager.GetConfigurations(Allocator.Temp))
            {
                if (configurationIndex >= configurations.Length)
                {
                    return;
                }

                // Get that configuration by index
                var configuration = configurations[configurationIndex];

                // Make it the active one
                cameraManager.currentConfiguration = configuration;
                cameraManager.focusMode = CameraFocusMode.Auto;
                cameraManager.autoFocusRequested = true;
                Debug.Log($"Auto Focus = {cameraManager.autoFocusEnabled}");

                //Send to Settings Manager
                settingsManager.ChangeCameraConfig(configurationIndex);

            }
        }

        public void SetupInitialValue(int index)
        {
            using (var configurations = cameraManager.GetConfigurations(Allocator.Temp))
            {
                if (index >= configurations.Length)
                {
                    return;
                }

                // Get that configuration by index
                var configuration = configurations[index];

                // Make it the active one
                cameraManager.currentConfiguration = configuration;
                cameraManager.autoFocusRequested = true;
                cameraManager.focusMode = CameraFocusMode.Auto;
                Debug.Log($"Auto Focus = {cameraManager.autoFocusEnabled}");
            }
        }

        void Awake()
        {
            m_Dropdown = GetComponent<TMP_Dropdown>();
            m_Dropdown.ClearOptions();
            m_Dropdown.onValueChanged.AddListener(delegate { OnDropdownValueChanged(m_Dropdown); });
            m_ConfigurationNames = new List<string>();
        }

        private void Start()
        {
            settingsManager = SettingsManager.instance;
        }

        void PopulateDropdown()
        {
            if ((cameraManager == null) || (cameraManager.subsystem == null) || !cameraManager.subsystem.running)
                return;

            // No configurations available probably means this feature
            // isn't supported by the current device.
            using (var configurations = cameraManager.GetConfigurations(Allocator.Temp))
            {
                if (!configurations.IsCreated || (configurations.Length <= 0))
                {
                    return;
                }

                // There are two ways to enumerate the camera configurations.

                // 1. Use a foreach to iterate over all the available configurations
                foreach (var config in configurations)
                {
                    m_ConfigurationNames.Add($"{config.width}x{config.height}{(config.framerate.HasValue ? $" at {config.framerate.Value} Hz" : "")}{(config.depthSensorSupported == Supported.Supported ? " depth sensor" : "")}");
                }
                m_Dropdown.AddOptions(m_ConfigurationNames);

                // 2. Use a normal for...loop
                var currentConfig = cameraManager.currentConfiguration;
                for (int i = 0; i < configurations.Length; ++i)
                {
                    // Find the current configuration and update the drop down value
                    if (currentConfig == configurations[i])
                    {
                        m_Dropdown.value = i;
                    }
                }
            }
        }

        void Update()
        {
            if (m_ConfigurationNames.Count == 0)
                PopulateDropdown();

            if(cameraManager != null && cameraManager.enabled)
            {
                //Force auto focus to be on when possible
                cameraManager.autoFocusRequested = true;
                cameraManager.focusMode = CameraFocusMode.Auto;
            }
        }
    }
}
