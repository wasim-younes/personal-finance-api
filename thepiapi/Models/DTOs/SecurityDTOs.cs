using System.ComponentModel.DataAnnotations;

namespace thepiapi.Models.DTOs
{
    public class UserPasswordUpdateRequest
    {
        [Required]
        public string CurrentPassword { get; set; } = string.Empty;
        [Required]
        public string NewPassword { get; set; } = string.Empty;
    }

    public class SetPinRequest
    {
        [Required]
        [StringLength(6, MinimumLength = 4)]
        public string Pin { get; set; } = string.Empty;
    }
}