using System.ComponentModel.DataAnnotations;

namespace khaosat_api.DTOs
{
    public class ChangeAccessTypeDto
    {
        [Required]
        public int AccessType { get; set; } // 1: Internal, 2: Public, 3: Invitation
    }

    public class ChangeAnonymousModeDto
    {
        [Required]
        public bool AnonymousMode { get; set; }
    }
}
