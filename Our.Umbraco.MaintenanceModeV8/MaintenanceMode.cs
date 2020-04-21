using System;
using System.Configuration;
using System.IO;
using System.Xml.Serialization;
using Our.Umbraco.MaintenanceModeV8.Models;
using Umbraco.Core.IO;
using Umbraco.Core.Logging;

namespace Our.Umbraco.MaintenanceModeV8
{
    public class MaintenanceMode
    {
        private readonly ILogger _logger;

        public MaintenanceMode(ILogger logger)
        {
            _logger = logger;
        }

        private static MaintenanceMode _instance;
        public static MaintenanceMode Current
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new MaintenanceMode();
                    _instance.InitializeCurrent();
                }

                return _instance;
            }
        }

        public MaintenanceModeStatus Status { get; private set; }

        private readonly string configFilePath;

        public MaintenanceMode()
        {
            configFilePath = IOHelper.MapPath(
                Path.Combine(SystemDirectories.Config, "MaintenanceMode.Config"));
            
        }

        internal void InitializeCurrent()
        {
            this.Status = InitializeStatus();
        }

        public void ToggleMaintenanceMode(bool maintenanceMode)
        {
            if (maintenanceMode != Status.IsInMaintenanceMode)
            {
                Status.IsInMaintenanceMode = maintenanceMode;
                PersistStatusToDisk();
            }
        }

        public void SaveSettings(MaintenanceModeSettings settings)
        {
            Status.Settings = settings;
            PersistStatusToDisk();
        }

        private MaintenanceModeStatus InitializeStatus()
        {
            MaintenanceModeStatus status = null;

            if (configFilePath != null && File.Exists(configFilePath))
            {
                try
                {
                    XmlSerializer serializer = new XmlSerializer(typeof(MaintenanceModeStatus));
                    string xml = File.ReadAllText(configFilePath);
                    using(TextReader reader = new StringReader(xml))
                    {
                        status = (MaintenanceModeStatus)serializer.Deserialize(reader);
                    }
                }
                catch(Exception ex)
                {
                    _logger.Warn<MaintenanceMode>("Failed to load maintance mode settings file: {0}", ex.Message);
                }
            }

            if (status == null) {
                status = new MaintenanceModeStatus
                {
                    IsInMaintenanceMode = false,
                    Settings = new MaintenanceModeSettings
                    {
                        AllowBackOfficeUsersThrough = false,
                        TemplateName = "MaintenancePage"
                    }
                };
            }

            status = CheckWebConfig(status);

            return status; 
        }

        private MaintenanceModeStatus CheckWebConfig(MaintenanceModeStatus status)
        {
            var webConfigSetting = ConfigurationManager.AppSettings["MaintenanceMode"];
            if (webConfigSetting != null)
            {
                if (bool.TryParse(webConfigSetting, out bool mode))
                {
                    status.IsInMaintenanceMode = mode;
                    status.UsingWebConfig = true;
                }
            }

            return status;
        }


        private void PersistStatusToDisk()
        {
            try
            {
                if (File.Exists(configFilePath))
                    File.Delete(configFilePath);

                XmlSerializer serializer = new XmlSerializer(typeof(MaintenanceModeStatus));

                using(StreamWriter w = new StreamWriter(configFilePath))
                {
                    serializer.Serialize(w, Status);
                }
            }
            catch(Exception ex)
            {
                _logger.Warn<MaintenanceMode>("Failed to save maintance mode settings file: {0}", ex.Message);
            }
        }
    }
}
