using System;
using System.Collections.Generic;

namespace ChurchManagementMVC.Models
{
    public class CommunionAttendanceVM
    {
        public DateTime CommunionDate { get; set; }

        public string Minister { get; set; } = string.Empty;

        public string? Remarks { get; set; }

        public List<CommunionAttendanceMemberVM> Members { get; set; }
            = new List<CommunionAttendanceMemberVM>();
    }

    public class CommunionAttendanceMemberVM
    {
        public int MemberId { get; set; }

        public string MemberName { get; set; } = string.Empty;

        public string? CardNumber { get; set; }

        public string PhoneNumber { get; set; } = string.Empty;

        public AttendanceStatus Status { get; set; }

        public string? Remarks { get; set; }
    }
}