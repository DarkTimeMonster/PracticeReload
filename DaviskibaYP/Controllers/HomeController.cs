using Domain.ViewModels.Home;
using Microsoft.AspNetCore.Mvc;
using Services;
using System.Threading;
using System.Threading.Tasks;

namespace DaviskibaYP.Controllers
{
    public class HomeController : Controller
    {
        private readonly ContactService _contactService;
        private readonly FestivalService _festivalService;

        public HomeController(ContactService contactService, FestivalService festivalService)
        {
            _contactService = contactService;
            _festivalService = festivalService;
        }

        [HttpGet]
        public async Task<IActionResult> Index(CancellationToken ct)
        {
            var nearest = await _festivalService.GetNearestAsync(ct);

            var vm = new HomeIndexViewModel
            {
                NearestFestival = nearest
            };

            return View(vm);
        }
    }
}
