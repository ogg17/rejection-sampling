using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace RejectionApp.Pages
{
    public class ResultError : PageModel
    {
        [BindProperty(SupportsGet = true)] public string ExMessage { get; set; }

        public IActionResult OnGet()
        {
            return Page();
        }
    }
}