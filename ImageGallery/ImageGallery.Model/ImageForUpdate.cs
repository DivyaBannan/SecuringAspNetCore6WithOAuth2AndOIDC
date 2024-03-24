using System.ComponentModel.DataAnnotations;

namespace ImageGallery.Model
{
    public class ImageForUpdate
    {
        [Required]
        [MaxLength(150)]
        public string Title { get; set; } = string.Empty;

        public ImageForUpdate(string title) 
        {
             Title = title;
        }
    }
}
