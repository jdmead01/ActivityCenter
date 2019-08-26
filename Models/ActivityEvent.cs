using System.ComponentModel.DataAnnotations;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
namespace ActivityCenter.Models
{
    public class ActivityEvent
    {
        public User Creator {get;set;}
        public int CreatorId{get;set;}
        
        [Key]
        public int ActivityId{get;set;}

        [Required]
        public DateTime Date {get;set;}

        [Required]
        public string Time {get;set;}
        
        [Required]
        [MinLength(2)]
        public string Title{get;set;}
        
        [Required]
        public int Duration {get;set;}

        public string Units {get;set;}

        [Required]
        [MinLength(10)]
        public string Description {get;set;}

        public List<Guest> Guests {get;set;}
        
        public ActivityEvent(){
            Guests = new List<Guest>();
        }
    }
}