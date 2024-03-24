using ImageGallery.Client.Models;
using ImageGallery.Client.ViewModels;
using ImageGallery.Model;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using System.Diagnostics;
using System.Text;
using System.Text.Json;

namespace ImageGallery.Client.Controllers
{
    [Authorize]
    public class GalleryController : Controller
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILogger<GalleryController> _logger;

        public GalleryController(IHttpClientFactory httpClientFactory, ILogger<GalleryController> logger)
        {
            _httpClientFactory = httpClientFactory ?? throw new ArgumentNullException(nameof(httpClientFactory));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<IActionResult> Index()
        {
            var httpClient = _httpClientFactory.CreateClient("APIClient");

            var request = new HttpRequestMessage(HttpMethod.Get, "/api/images");
            var response = await httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead).ConfigureAwait(false);
            response.EnsureSuccessStatusCode();
            using (var responseStream = await response.Content.ReadAsStreamAsync())
            {
                var images = await JsonSerializer.DeserializeAsync<List<Image>>(responseStream);
                return View(new GalleryIndexViewModel(images ?? new List<Image>()));
            }              
        }

        public async Task<IActionResult> EditImage(Guid id)
        {
            var httpClient =  _httpClientFactory.CreateClient("APIClient");

            var request = new HttpRequestMessage(HttpMethod.Get, $"/api/images/{id}");

            var response = await httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead).ConfigureAwait(false);

            response.EnsureSuccessStatusCode();

            using (var responseStream = await response.Content.ReadAsStreamAsync())
            {
                var deserializedImage = await JsonSerializer.DeserializeAsync<Image>(responseStream);

                if (deserializedImage == null) {
                    throw new Exception("Deserialized image must not be null.");
                }

                var editImageViewModel = new EditImageViewModel() {
                    Id = id,
                    Title = deserializedImage.Title,
                };

                return View(editImageViewModel);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditImage(EditImageViewModel editImageViewModel)
        {
            if (!ModelState.IsValid)
            {
                return View();
            }

            var imageForUpdate = new ImageForUpdate(editImageViewModel.Title);
            var serializeImageForUpdate = JsonSerializer.Serialize(imageForUpdate);
            var httpClient = _httpClientFactory.CreateClient("APIClient");

            var request = new HttpRequestMessage(HttpMethod.Post, $"/api/images/{editImageViewModel.Id}")
            {
                Content = new StringContent(serializeImageForUpdate, System.Text.Encoding.Unicode, "application/json")
            };

            var response = await httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead);

            response.EnsureSuccessStatusCode();

            return RedirectToAction("Index");
        }

        public async Task<IActionResult> DeleteImage(Guid id)
        {
            var httpClient = _httpClientFactory.CreateClient("APIClient");

            var request = new HttpRequestMessage(HttpMethod.Delete,
              $"/api/images/{id}");

            var response = await httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead);
            
            response.EnsureSuccessStatusCode();
            return RedirectToAction("Index");              
           
        }


        //[Authorize(Roles = "PayingUser")]
        [Authorize(Policy = "UserCanAddImage")]
        public IActionResult AddImage()
        {
            return View();
        }


        [Authorize(Policy = "UserCanAddImage")]
        //[Authorize(Roles ="PayingUser")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddImage(AddImageViewModel addImageViewModel)
        {
            if (!ModelState.IsValid)
            {
                return View();
            }

            ImageForCreation? imageForCreation = null;

            var imageFile = addImageViewModel.Files.FirstOrDefault();

            if (imageFile.Length > 0)
            {
                using( var fileStream = imageFile.OpenReadStream())
                {
                    using (var ms = new MemoryStream())
                    {
                        fileStream.CopyTo(ms);
                        imageForCreation = new ImageForCreation(addImageViewModel.Title, ms.ToArray());
                    }
                }
            }

            var serializeImageForCreation =  JsonSerializer.Serialize(imageForCreation);

            var httpClient = _httpClientFactory.CreateClient("APIClient");

            var request = new HttpRequestMessage(HttpMethod.Post, "/api/images") {

                Content = new StringContent(serializeImageForCreation, System.Text.Encoding.Unicode, "application/json")
            };

            var response = await httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead).ConfigureAwait(false);

            response.EnsureSuccessStatusCode();

            return RedirectToAction("Index");

        }

        public async Task LogIdentityInformation()
        {
            //get the saved Identity token

            var identityToken = await HttpContext.GetTokenAsync(OpenIdConnectParameterNames.IdToken);

            //get the saved access token

            var accessToken = await HttpContext.GetTokenAsync(OpenIdConnectParameterNames.AccessToken);

            //get the saved refresh token

            var refreshToken = await HttpContext.GetTokenAsync(OpenIdConnectParameterNames.RefreshToken);

            var userClainsStringBuilder = new StringBuilder();

            foreach (var claim in User.Claims)
            {
                userClainsStringBuilder.AppendLine($"ClaimType: {claim.Type} -Calim Value: { claim.Value}");
            }

            _logger.LogInformation($" Identity token and user claims:"
                + $"\n {identityToken} \n {userClainsStringBuilder}");

            _logger.LogInformation($" Access token:"
               + $"\n {accessToken}");

            _logger.LogInformation($" Refresh token:"
             + $"\n {refreshToken}");
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
