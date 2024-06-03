using System.ComponentModel.DataAnnotations;

namespace Api.Dtos
{
    public class UserDto
    {
        [Required]
        [Length(3, 50, ErrorMessage ="Name Must Be Between 3 - 50 Character")]
        public string Name { get; set; }
    }
}
