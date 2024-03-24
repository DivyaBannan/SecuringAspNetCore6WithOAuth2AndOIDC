using ImageGallery.Model;

namespace ImageGallery.Client.ViewModels
{
    public class GalleryIndexViewModel
    {
        public IEnumerable<Image> Images { get; private set; }
            = new List<Image>();

        public GalleryIndexViewModel(IEnumerable<Image> images)
        {
            Images = images;
        }
    }
}
