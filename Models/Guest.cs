using System.ComponentModel.DataAnnotations;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
namespace ActivityCenter.Models
{
    public class Guest
    {
        [Key]
        public int GuestId {get;set;}
        public int UserId{get;set;}
        public User User{get;set;}
        public int ActivityId{get;set;}
        public ActivityEvent Activity{get;set;}
        
    }
}