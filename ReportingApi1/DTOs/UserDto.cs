using System.ComponentModel.DataAnnotations;

namespace ReportingApi1.DTOs
{
    public class RegisterUserDto
    {
        [Required]
        [MinLength(3)]
        [MaxLength(50)]
        public string UserName { get; set;  } = string.Empty;

        [Required]
        [MaxLength(100)]
        public string Password { get; set; } = string.Empty;

        [Range(1, int.MaxValue)]
        public int CompanyId { get; set; }
    }

    public class LoginUserDto
    {
        [Required]
        public string UserName { get; set; } = string.Empty;
        [Required]
        public string Password { get; set; } = string.Empty;
    }
}
