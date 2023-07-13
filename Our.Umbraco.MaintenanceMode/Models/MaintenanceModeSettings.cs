﻿using Newtonsoft.Json.Serialization;
using Newtonsoft.Json;

namespace Our.Umbraco.MaintenanceMode.Models
{
    [JsonObject(NamingStrategyType = typeof(DefaultNamingStrategy))]
    public class MaintenanceModeSettings
    {
        public bool AllowBackOfficeUsersThrough { get; set; } = false;
        public string TemplateName { get; set; } = "MaintenancePage";

        /// <summary>
        ///  users who can get past the content freeze 
        /// </summary>
        public string UnfrozenUsers { get; set; } = "";
        public MaintenanceMode ViewModel { get; set; }
    }
}