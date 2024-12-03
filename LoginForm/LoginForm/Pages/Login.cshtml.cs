using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace LoginForm.Pages
{
    public class LoginModel : PageModel
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public LoginModel(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        [BindProperty]
        [Required(ErrorMessage = "Email is required.")]
       /* [EmailAddress(ErrorMessage = "Invalid email address.")]*/
        public string Email { get; set; }

        [BindProperty]
        [Required(ErrorMessage = "Password is required.")]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        public string ApiResponseMessage { get; set; }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            var client = _httpClientFactory.CreateClient();
            var loginRequest = new
            {
                email = Email,
                password = Password
            };

            string apiUrl = "https://healthtrack-hydtdue4ede8b5fp.southeastasia-01.azurewebsites.net/api/Users/login";

            var requestContent = new StringContent(
                JsonSerializer.Serialize(loginRequest),
                Encoding.UTF8,
                "application/json");

            var response = await client.PostAsync(apiUrl, requestContent);

            if (response.IsSuccessStatusCode)
            {
                ApiResponseMessage = "Login successful!";
                return RedirectToPage("/Index");
            }
            else
            {
                var errorResponse = await response.Content.ReadAsStringAsync();
                ApiResponseMessage = ParseApiResponse(errorResponse);
                ModelState.AddModelError(string.Empty, ApiResponseMessage);
                return Page();
            }
        }

        private string ParseApiResponse(string response)
        {
            try
            {
                var errorData = JsonSerializer.Deserialize<ApiErrorResponse>(response);
                if (errorData?.Errors != null && errorData.Errors.Count > 0)
                {
                    // Combine all error messages into a single string
                    return string.Join(" ", errorData.Errors.Values.SelectMany(e => e));
                }
            }
            catch
            {
                // Default fallback message
            }
            return "An unexpected error occurred. Please try again.";
        }

        public class ApiErrorResponse
        {
            public string Title { get; set; }
            public Dictionary<string, string[]> Errors { get; set; }
        }
    }
}
