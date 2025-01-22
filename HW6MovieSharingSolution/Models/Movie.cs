using System;
using System.ComponentModel.DataAnnotations;

namespace HW6MovieSharingSolution.Models
{
    public class Movie
    {
        [Required]
        public long Id { get; set; }

        [Required]
        [MaxLength(1024)]
        public string Title { get; set; }

        [Required]
        [MaxLength(256)]
        public string Category { get; set; }

        [Required]
        public string Owner { get; set; }

        [Required]
        [MaxLength(256)]
        [Display(Name = "Owner Email")]
        public string OwnerEmail { get; set; }

        [MaxLength(256)]
        [Display(Name = "Borrower")]
        public string SharedWithName { get; set; }

        [MaxLength(256)]
        [Display(Name = "Borrower Email")]
        public string SharedWithEmailAddress { get; set; }

        [Display(Name = "Shared Date")]
        public DateTime SharedDate { get; set; }

        public string UserRealmId { get; set; }

        public string SharedUserRealmId { get; set; }

        [Display(Name = "Can Share")]
        public bool CanBeShared { get; set; }

        [MaxLength(256)]
        [Display(Name = "Approval Status")]
        public string AprovalStatus { get; set; }

        public bool isApproved { get; set; }

    }
}
