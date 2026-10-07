using System;
using System.Collections.Generic;

namespace ChurchManagementMVC.Models
{
    public class CommunionService
    {
        public int Id { get; set; }

        public DateTime CommunionDate { get; set; }

        public string ServiceType { get; set; } = string.Empty;

        public string? Minister { get; set; }

        public string? Remarks { get; set; }

        public DateTime DateRecorded { get; set; }

        public List<CommunionParticipant> Participants { get; set; }
            = new List<CommunionParticipant>();
    }
}