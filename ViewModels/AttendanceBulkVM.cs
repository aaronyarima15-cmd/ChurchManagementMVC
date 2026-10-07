using System;
using System.Collections.Generic;

namespace ChurchManagementMVC.Models
{
    public class AttendanceBulkVM
    {
        public DateTime AttendanceDate { get; set; } = DateTime.Today;

        public string ServiceType { get; set; } = string.Empty;

        public string? Remarks { get; set; }

        public List<AttendanceMemberVM> Members { get; set; }
            = new List<AttendanceMemberVM>();
    }

    public class AttendanceMemberVM
    {
        public int MemberId { get; set; }

        public string MemberName { get; set; } = string.Empty;

        public string? CardNumber { get; set; }

        public string PhoneNumber { get; set; } = string.Empty;

        public AttendanceStatus Status { get; set; } = AttendanceStatus.Absent;
    }
}