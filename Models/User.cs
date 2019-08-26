using System.ComponentModel.DataAnnotations;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
namespace ActivityCenter.Models
{
    public class User
    {
        [Key]
        public int Id {get;set;}

        [Required]
        [RegularExpression(@"^[a-zA-Z''-'\s]{1,40}$", ErrorMessage = "First Name: Numeric and Special Characters are not allowed.")]
        [MinLength(2)]
        public string FirstName {get;set;}

        [Required]
        [RegularExpression(@"^[a-zA-Z''-'\s]{1,40}$", ErrorMessage = "Last Name: Numeric and Special Characters are not allowed.")]
        [MinLength(2)]
        public string LastName {get;set;}
        
        [Required]
        [DataType(DataType.EmailAddress)]
        public string Email {get;set;}

        [Required]
        [DataType(DataType.Password)]
        [RegularExpression(@"^(?=.*[A-Za-z])(?=.*\d)(?=.*[@$!%*#?&])[A-Za-z\d@$!%*#?&]{8,}$", ErrorMessage= "Password Must Contain at least one letter, one number, and one special character")]
        [MinLength(8)]
        public string Password {get;set;}

        [Required]
        [DataType(DataType.Password)]
        [Compare("Password")]
        [NotMapped]
        public string PasswordConfirmation {get;set;}

        public List<Guest> ActivitiesAttending {get;set;}

        public List<ActivityEvent> CreatedActivities{get;set;}

        public User(){
            ActivitiesAttending = new List<Guest>();
            CreatedActivities = new List<ActivityEvent>();

        }

    }
}