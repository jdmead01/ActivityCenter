using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using ActivityCenter.Models;


namespace ActivityCenter.Controllers
{
    public class HomeController : Controller
    {
        private MyContext _context;
 
        public HomeController(MyContext context)
        {
            _context = context;
        }
        public IActionResult Index()
        {
            ViewBag.FName = TempData["FirstName"];
            ViewBag.LName = TempData["LastName"];
            ViewBag.Password = TempData["Password"];
            ViewBag.PConfirm = TempData["PasswordConfirmation"];
            ViewBag.Login = TempData["LoginBasic"];
            ViewBag.NoUser = TempData["NoUser"];
            ViewBag.BadInfo = TempData["IncorrectInfo"];
            ViewBag.ExistingUser = TempData["ExistingUser"];
            return View();
        }

[HttpPost]
        public IActionResult RegisterUser(User newuser)
        {
            if (ModelState.IsValid)
            {
                List<User> userlist = _context.Users.Where(u => u.Email == newuser.Email).ToList();
                if (userlist.Count() > 0)
                {
                    TempData["ExistingUser"] = "A user with this email already exists";
                    return RedirectToAction("Index");
                }
                PasswordHasher<User> Hasher = new PasswordHasher<User>();
                newuser.Password = Hasher.HashPassword(newuser, newuser.Password);
                User registeringuser = new User
                {
                    FirstName = newuser.FirstName,
                    LastName = newuser.LastName,
                    Email = newuser.Email, 
                    Password = newuser.Password,
                };
                 _context.Add(registeringuser);
                 _context.SaveChanges();
                HttpContext.Session.SetInt32("UserId", registeringuser.Id);
                return RedirectToAction("Home");
            }
            else
            {
                foreach (var MSkey in ModelState.Keys)
                {
                    var val = ModelState[MSkey];
                    foreach (var error in val.Errors)
                    {
                        var key = MSkey;
                        var EM = error.ErrorMessage;
                        TempData[key] = EM;
                    }
                }

                return RedirectToAction("Index");
            }
        }

[HttpPost]
        public IActionResult LoginUser(string Email, string Password)
        {
            if (Password ==null || Email ==null)
            {
                TempData["LoginBasic"] = "Please Enter Your Email and Password";
                return RedirectToAction("Index");
            }

            List<User> userlist = _context.Users.Where(u => u.Email == Email).Select(u =>u).ToList();
            if (userlist.Count ==0 || Password == null){
                TempData["NoUser"]= "No user with that email exists";
                return RedirectToAction("Index");
            }
            else
            {
                User user = userlist.First();
                var Hasher = new PasswordHasher<User>();
                // Pass the user object, the hashed password, and the PasswordToCheck
    		    var result = Hasher.VerifyHashedPassword(user, user.Password, Password);
                if(result != 0)
                    {
                        HttpContext.Session.SetInt32("UserId", user.Id);
                        return RedirectToAction("Home");
                    }
                else
                {
                    TempData["IncorrectInfo"] = "Information is Incorrect";
                    return RedirectToAction("Index");
                }
            }
        }

[HttpGet("Home")]
        public IActionResult Home()
        {
            ViewBag.Id = HttpContext.Session.GetInt32("UserId");
            int? UserId = HttpContext.Session.GetInt32("UserId");
            if(UserId == null){
                return RedirectToAction("Index");
            }
            List<ActivityEvent> AllActivities =_context.Activites.Include(w=>w.Creator).Include(w=>w.Guests).ThenInclude(g => g.User).ToList();
            List<Guest> AllLinks = _context.Links.Where(u => u.UserId == UserId).ToList();
            ViewBag.AllLinks = AllLinks;
            // foreach (var link in AllLinks){
            //     System.Console.WriteLine("user id");
            //     System.Console.WriteLine(link.UserId);
            //     System.Console.WriteLine("wedding id");
            //     System.Console.WriteLine(link.WeddingId);
            // }
            // foreach (var wedding in AllWeddings)
            // {
            //     foreach (var guest in wedding.Guests){
            //         if (guest.User.Id == (int)UserId){
            //             GuestLoggedIn = true;
            //         }
            //     }
            // }
            User user = _context.Users.Where(u => u.Id == (HttpContext.Session.GetInt32("UserId"))).FirstOrDefault();
            ViewBag.user= user;
    
            List<ActivityEvent> DateBasedAllActivities= _context.Activites.ToList().OrderBy(a =>a.Date).ThenBy(a=> a.Time).ToList();
            List<ActivityEvent> Regular=_context.Activites.ToList().OrderBy(a => a.Date).ToList();

            foreach ( var d in DateBasedAllActivities)
            {

                if (d.Date < DateTime.Today){
                    _context.Activites.Remove(d);
                    _context.SaveChanges();
                }
                else if (d.Date == DateTime.Today)
                {
                    var hourbeforeparse = d.Time.Split(':').First();
                    var hour = Int32.Parse(hourbeforeparse);
                    var nowhour = DateTime.Now.Hour;
                    if (hour < nowhour)
                    {
                        _context.Activites.Remove(d);
                        _context.SaveChanges();
                    }
                    else if (hour == nowhour)
                    {
                        var minutebeforeparse = d.Time.Split(':')[1];
                        var minute = Int32.Parse(minutebeforeparse);
                        var nowminute = DateTime.Now.Minute;
                        if (minute < nowminute)
                        {
                            _context.Activites.Remove(d);
                            _context.SaveChanges();  
                        }
                    }
                }
            }
            ViewBag.AA = DateBasedAllActivities;
            ViewBag.AlreadyRSVP = TempData["AlreadyRSVP"];
            ViewBag.Conflictingtimes= TempData["Conflict"];
            return View();
        }

        [HttpGet("AddActivity")]
        public IActionResult AddActivity()
        {
            if(HttpContext.Session.GetInt32("UserId") == null){
                return RedirectToAction("Index");
            }
            ViewBag.Future = TempData["Future"];
            return View();
        }

    [HttpGet("activity/{actId}")]
    public IActionResult Activity(int actId){
       if(HttpContext.Session.GetInt32("UserId") == null){
                return RedirectToAction("Index");
            }
            ViewBag.Id = HttpContext.Session.GetInt32("UserId");
            ActivityEvent activity= _context.Activites.Where(w => w.ActivityId == actId).FirstOrDefault();
            ViewBag.activity = activity;
            List<ActivityEvent> guests = _context.Activites.Where(w => w.ActivityId == actId).Include( u=> u.Creator).Include(u =>u.Guests).ThenInclude(u=> u.User).ToList();
            ViewBag.Guests = guests;
            return View();
    }


        [HttpPost]
        public IActionResult CreateActivity(ActivityEvent newactivity)
        {
            if(HttpContext.Session.GetInt32("UserId") == null){
                return RedirectToAction("Index");
            }
            var j = newactivity.Time.Split(':').First();
            var za = newactivity.Time.Split(':')[1];
            var i = Int32.Parse(j);
            var m = Int32.Parse(za);
            var nh = DateTime.Now.Hour;
            var nm = DateTime.Now.Minute;
            if (newactivity.Date > DateTime.Today){
                User user = _context.Users.Where(u => u.Id == (HttpContext.Session.GetInt32("UserId"))).FirstOrDefault();
                if (ModelState.IsValid){
                    ActivityEvent creatingactivity = new ActivityEvent(){
                        Creator = user,
                        CreatorId = user.Id,
                        Title = newactivity.Title,
                        Time = newactivity.Time,
                        Date = newactivity.Date,
                        Duration = newactivity.Duration,
                        Units= newactivity.Units,
                        Description = newactivity.Description,
                    };
                    _context.Activites.Add(creatingactivity);
                    _context.SaveChanges();
                    return RedirectToAction("Activity", new {actId = creatingactivity.ActivityId});
                }
                else{
                    return View("AddActivity");
                }
             }
            else if (newactivity.Date == DateTime.Today){
                if (i > nh)
                {
                    User user = _context.Users.Where(u => u.Id == (HttpContext.Session.GetInt32("UserId"))).FirstOrDefault();
                    if (ModelState.IsValid){
                        ActivityEvent creatingactivity = new ActivityEvent(){
                            Creator = user,
                            CreatorId = user.Id,
                            Title =newactivity.Title,
                            Time = newactivity.Time,
                            Date = newactivity.Date,
                            Duration = newactivity.Duration,
                            Units= newactivity.Units,
                            Description = newactivity.Description,
                        };
                        _context.Activites.Add(creatingactivity);
                        _context.SaveChanges();
                        return RedirectToAction("Activity", new {actId = creatingactivity.ActivityId});
                    }
                    else{
                        return View("AddActivity");
                        }
                }
                else if ( i == nh)
                {
                    if (m > nm)
                    {
                        User user = _context.Users.Where(u => u.Id == (HttpContext.Session.GetInt32("UserId"))).FirstOrDefault();
                        if (ModelState.IsValid){
                            ActivityEvent creatingactivity = new ActivityEvent(){
                                Creator = user,
                                CreatorId = user.Id,
                                Title =newactivity.Title,
                                Time = newactivity.Time,
                                Date = newactivity.Date,
                                Duration = newactivity.Duration,
                                Units= newactivity.Units,
                                Description = newactivity.Description,
                            };
                            _context.Activites.Add(creatingactivity);
                            _context.SaveChanges();
                            return RedirectToAction("Activity", new {actId = creatingactivity.ActivityId});
                        }
                        else{
                            return View("AddActivity");
                            }
                    }
                    else
                    {
                        TempData["Future"] = "Must enter a Time and Date in the future";
                        return RedirectToAction("AddActivity");
                    }
                }
                else{
                TempData["Future"] = "Must enter a Time and Date in the future";
                return RedirectToAction("AddActivity");
                }

            }
            else{
                TempData["Future"] = "Must enter a Time and Date in the future";
                return RedirectToAction("AddActivity");
                }
        }

        [Route("logout")]
        public IActionResult Logout(){
            HttpContext.Session.Clear();
            return RedirectToAction("Index");
        }

public IActionResult Delete (int actId){
            ActivityEvent wedding = _context.Activites.Where(w =>w.ActivityId == actId).SingleOrDefault();
            _context.Activites.Remove(wedding);
            _context.SaveChanges();
            return RedirectToAction("Home");
        }
[HttpPost("Join")]
        public IActionResult Join(int actId)
        {
            User user = _context.Users.Where(u => u.Id == (HttpContext.Session.GetInt32("UserId"))).FirstOrDefault();
            ActivityEvent activity =_context.Activites.Where(w => w.ActivityId == actId).FirstOrDefault();
            Guest existinglink = _context.Links.Where(u => u.UserId == user.Id && u.ActivityId == actId).FirstOrDefault();
            List<Guest> otherlinks = _context.Links.Where( u => u.UserId == user.Id).Include(l => l.Activity).ToList(); 
            foreach (var link in otherlinks){
                if (link.Activity.Date == activity.Date && link.Activity.Time == activity.Time)
                {
                    TempData["Conflict"] = "Can't have activities at same time";
                    return RedirectToAction("Home");
                }
                if (link.Activity.Date == activity.Date)
                {
                    var existinghour1 = link.Activity.Time.Split(':').First();
                    var existingminute1 = link.Activity.Time.Split(':')[1];
                    var existinghour = Int32.Parse(existinghour1);
                    var existingminute = Int32.Parse(existingminute1);

                    var tryhour1 = activity.Time.Split(':').First();
                    var tryminute1 = activity.Time.Split(':')[1];
                    var tryhour = Int32.Parse(tryhour1);
                    var tryminute = Int32.Parse(tryminute1);
                    if (existinghour > tryhour && existinghour <= tryhour + activity.Duration && activity.Units =="Hours")
                    {
                        TempData["Conflict"] = "Can't have activities at same time";
                        return RedirectToAction("Home");   
                    }
                    else if (existinghour == tryhour){
                        if (existinghour + link.Activity.Duration < tryhour + activity.Duration && link.Activity.Units == "Hours" && activity.Units =="Hours"){
                            TempData["Conflict"] = "Can't have activities at same time";
                            return RedirectToAction("Home"); 
                            }
                        else {
                            TempData["Conflict"] = "Can't have activities at same time";
                            return RedirectToAction("Home"); 
                        }
                    }
                }
            }
            if (existinglink != null){
                TempData["AlreadyRSVP"] = "You have already RSVPd to this event";
                return RedirectToAction("Home");
            }
            else{
            Guest newguest = new Guest(){
                User = user,
                Activity = activity,
                UserId = user.Id,
                ActivityId = activity.ActivityId,
            };
            System.Console.WriteLine("activity id");
            System.Console.WriteLine(activity.ActivityId);
            System.Console.WriteLine("link activity id");
            System.Console.WriteLine(newguest.ActivityId);
            user.ActivitiesAttending.Add(newguest);
            _context.Links.Add(newguest);
            _context.SaveChanges();
            return RedirectToAction("Home");
             }
        }
[HttpPost("Leave")]
        public IActionResult Leave (int actId){
            Guest guest = _context.Links.Where( l => l.ActivityId == actId && l.UserId == (HttpContext.Session.GetInt32("UserId"))).SingleOrDefault();
            _context.Links.Remove(guest);
            _context.SaveChanges();
            return RedirectToAction("Home");
        }

        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = System.Diagnostics.Activity.Current?.Id ?? HttpContext.TraceIdentifier});
        }

    }
}
