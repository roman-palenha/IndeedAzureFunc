﻿using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FunctionApp1.Models
{
    // Root myDeserializedClass = JsonConvert.DeserializeObject<Root>(myJsonResponse);
    // Root myDeserializedClass = JsonConvert.DeserializeObject<Root>(myJsonResponse);
    public class Root
    {
        [JsonProperty("BusinessUnitId")]
        public string BusinessUnitId { get; set; }

        [JsonProperty("CorrelationId")]
        public string CorrelationId { get; set; }

        [JsonProperty("Depth")]
        public int Depth { get; set; }

        [JsonProperty("InitiatingUserAzureActiveDirectoryObjectId")]
        public string InitiatingUserAzureActiveDirectoryObjectId { get; set; }

        [JsonProperty("InitiatingUserId")]
        public string InitiatingUserId { get; set; }

        [JsonProperty("IsExecutingOffline")]
        public bool IsExecutingOffline { get; set; }

        [JsonProperty("IsInTransaction")]
        public bool IsInTransaction { get; set; }

        [JsonProperty("IsOfflinePlayback")]
        public bool IsOfflinePlayback { get; set; }

        [JsonProperty("IsolationMode")]
        public int IsolationMode { get; set; }

        [JsonProperty("MessageName")]
        public string MessageName { get; set; }

        [JsonProperty("Mode")]
        public int Mode { get; set; }

        [JsonProperty("OperationCreatedOn")]
        public DateTime OperationCreatedOn { get; set; }

        [JsonProperty("OperationId")]
        public string OperationId { get; set; }

        [JsonProperty("OrganizationId")]
        public string OrganizationId { get; set; }

        [JsonProperty("OrganizationName")]
        public string OrganizationName { get; set; }

        [JsonProperty("PrimaryEntityId")]
        public string PrimaryEntityId { get; set; }

        [JsonProperty("PrimaryEntityName")]
        public string PrimaryEntityName { get; set; }

        [JsonProperty("RequestId")]
        public string RequestId { get; set; }

        [JsonProperty("SecondaryEntityName")]
        public string SecondaryEntityName { get; set; }
    }




}
