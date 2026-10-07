using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace ChurchManagementMVC.Models
{
    public class MembershipManagement
    {
        public int Id { get; set; }

        public string FirstName { get; set; } = string.Empty;

        public string LastName { get; set; } = string.Empty;

        public string? OtherNames { get; set; }

        public string? FatherName { get; set; }

        public string? GrandfatherName { get; set; }

        public DateTime DateOfBirth { get; set; }

        public string? Gender { get; set; }

        public string PhoneNumber { get; set; } = string.Empty;

        public string? Email { get; set; }

        public MaritalStatusEnum? MaritalStatus { get; set; }

        [NotMapped]
        public string MaritalStatusDescription => MaritalStatus switch
        {
            MaritalStatusEnum.Single => "Single",
            MaritalStatusEnum.Married => "Married",
            MaritalStatusEnum.Divorced => "Divorced",
            MaritalStatusEnum.Widower => "Widower",
            MaritalStatusEnum.Widow => "Widow",
            _ => "Not specified"
        };

        public bool IsBaptized { get; set; }

        public string? CardNumber { get; set; }

        public string? PhotoPath { get; set; }

        public string? HomeAddress { get; set; }

        public string? PermanentAddress { get; set; }

        public int? StateId { get; set; }

        public State? State { get; set; }

        public int? LgaId { get; set; }

        public Lga? Lga { get; set; }

        public int RelationshipId { get; set; }

        public Relationship? Relationship { get; set; }

        public int StatusId { get; set; }

        public MembershipStatus? Status { get; set; }

        [NotMapped]
        public int Age
        {
            get
            {
                var age =
                    DateTime.Today.Year -
                    DateOfBirth.Year;

                if (DateTime.Today <
                    DateOfBirth.AddYears(age))
                {
                    age--;
                }

                return age;
            }
        }
    }
}