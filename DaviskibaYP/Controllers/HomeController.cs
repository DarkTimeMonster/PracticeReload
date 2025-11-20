using DAL;
using Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace DaviskibaYP.Controllers
{
    public class HomeController : Controller
    {
        private readonly ContactService _contactService;

        public HomeController(ContactService contactService, GastroFestDbContext db)
        {
            _contactService = contactService;
        }

        public IActionResult Index() => View();


    }
}
