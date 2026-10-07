using System;
using System.Collections.Generic;

namespace ChurchManagementMVC.Models
{
    public class CommunionReportVM
    {
        public DateTime CommunionDate { get; set; }

        public int Year { get; set; }

        public List<CommunionPresentMemberVM> MembersPresent { get; set; }
            = new List<CommunionPresentMemberVM>();

        public List<CommunionMonthlyMemberVM> MonthlyMembers { get; set; }
            = new List<CommunionMonthlyMemberVM>();
    }

    public class CommunionPresentMemberVM
    {
        public int MemberId { get; set; }

        public string Name { get; set; } = string.Empty;

        public string? CardNumber { get; set; }

        public string PhoneNumber { get; set; } = string.Empty;
    }

    public class CommunionMonthlyMemberVM
    {
        public int MemberId { get; set; }

        public string Name { get; set; } = string.Empty;

        public string Group { get; set; } = "No Group";

        public string[] Months { get; set; } = new string[12];
    }
}