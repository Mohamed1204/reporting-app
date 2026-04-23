using System.ComponentModel.DataAnnotations;

namespace ReportingApi1.DTOs
{
    public class RegisterUserDto
    {
        [Required]
        public string UserName { get; set;  } = string.Empty;
        [Required]
        public string Password { get; set; } = string.Empty;
        [Required]
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
