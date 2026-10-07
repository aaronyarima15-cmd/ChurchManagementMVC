using System.ComponentModel.DataAnnotations;

namespace ChurchManagementMVC.ViewModels
{
    public class ChangePasswordViewModel
    {
        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Please enter a valid email address")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Password is required")]
        [StringLength(
            40,
            MinimumLength = 8,
            ErrorMessage = "Password must be between {2} and {1} characters."
        )]
        [DataType(DataType.Password)]
        [Display(Name = "New Password")]
        public string NewPassword { get; set; } = string.Empty;

        [Required(ErrorMessage = "Confirm Password is required")]
        [DataType(DataType.Password)]
        [Compare("NewPassword", ErrorMessage = "Passwords do not match")]
        [Display(Name = "Confirm New Password")]
        public string ConfirmNewPassword { get; set; } = string.Empty;
    }
}