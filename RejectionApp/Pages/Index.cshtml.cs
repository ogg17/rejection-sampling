using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using RejectionApp.Services;

namespace RejectionApp.Pages
{
    public class IndexModel : PageModel, IResult
    {
        private readonly ILogger<IndexModel> _logger;
        
        [TempData]
        public int FuncCount { get; set; } = 2;

        public IndexModel(ILogger<IndexModel> logger)
        {
            _logger = logger;
        }

        [BindProperty] public Models.Result MyResult { get; set; } = new();

        public void OnGet(int count)
        {
            if (count < 1) count = 1;
            FuncCount = count;
        }

        public IActionResult OnPostAdd()
        {
            FuncCount = (int)TempData["FuncCount"] + 1;
            FuncCount = FuncCount > 100 ? 100 : FuncCount;
            MyResult.Count = FuncCount;
            return RedirectToRoute($"/index/{FuncCount}");
        }
        
        public IActionResult OnPostRemove()
        {
            FuncCount = (int)TempData["FuncCount"] - 1;
            FuncCount = FuncCount < 1 ? 1 : FuncCount;
            MyResult.Count = FuncCount;
            return RedirectToRoute($"/index/{FuncCount}");
        }
    }
}