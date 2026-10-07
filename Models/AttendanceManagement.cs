using System;

namespace ChurchManagementMVC.Models
{
    public class AttendanceManagement
    {
        public int Id { get; set; }

        public int MemberId { get; set; }

        public MembershipManagement? Member { get; set; }

        public DateTime AttendanceDate { get; set; }

        public string ServiceType { get; set; } = string.Empty;

        public AttendanceStatus Status { get; set; }

        public string? Remarks { get; set; }

        public DateTime DateRecorded { get; set; }
    }

    public enum AttendanceStatus
    {
        Present = 1,
        Absent = 2
    }
}