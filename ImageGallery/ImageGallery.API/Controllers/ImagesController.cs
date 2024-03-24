using AutoMapper;
using ImageGallery.API.Authorization;
using ImageGallery.API.Entities;
using ImageGallery.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Hosting.Internal;

namespace ImageGallery.API.Controllers
{
    [ApiController]
    [Route("api/images")]
    [Authorize]
    public class ImagesController : ControllerBase
    {
        private readonly IGalleryRepository _galleryRepository;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly IMapper _mapper;
        private readonly ILogger<ImagesController> _logger;

        public ImagesController(IGalleryRepository galleryRepository,
            IWebHostEnvironment webHostEnvironment, 
            IMapper mapper)
        {
            _galleryRepository = galleryRepository ??
                throw new ArgumentNullException(nameof(galleryRepository));
            _webHostEnvironment = webHostEnvironment ??
                throw new ArgumentNullException(nameof(webHostEnvironment));
            _mapper = mapper ??
                throw new ArgumentNullException(nameof(mapper));
        }

        [HttpGet()]
        public async Task<ActionResult<IEnumerable<Image>>> GetImages()
        {
           var ownerId = User.Claims.FirstOrDefault(c => c.Type == "sub")?.Value;

            if (ownerId == null)
            {
                throw new Exception("User identifier is missing from token");
            }
           var imagesFromRepo = await _galleryRepository.GetImagesAsync(ownerId);

            var imagesToReturn = _mapper.Map<IEnumerable<Image>> (imagesFromRepo);

            return Ok(imagesToReturn);
        }

        [HttpGet("{id}", Name = "GetImage")]
        //[Authorize("MustOwnImage")]
        [MustOwnImage]
        public async Task<ActionResult<Image>> GetImage(Guid id)
        {
            var imageFromRepo = await _galleryRepository.GetImageAsync(id);

            if (imageFromRepo == null)
            {
                return NotFound();
            }

            return Ok(imageFromRepo);   
        }
    }
}
