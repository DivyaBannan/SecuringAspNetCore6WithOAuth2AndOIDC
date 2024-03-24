using System.ComponentModel.DataAnnotations;

namespace ImageGallery.Client.ViewModels
{
    public class AddImageViewModel
    {
        public List<IFormFile> Files { get; set; } = new List<IFormFile>();

        [Required]
        public string Title { get; set; }

        public AddImageViewModel(List<IFormFile> files, string title)
        {
            Files = files;
            Title = title;
        }
    }
}
