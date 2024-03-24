using ImageGallery.API.DbContexts;
using ImageGallery.API.Entities;
using Microsoft.EntityFrameworkCore;

namespace ImageGallery.API.Services
{
    public class GalleryRepository : IGalleryRepository
    {
        private readonly GalleryContext _context;

        public GalleryRepository(GalleryContext galleryContext) {
                _context = galleryContext ??
                throw new ArgumentNullException(nameof(galleryContext));
        }

        public void AddImage(Image image)
        {
            _context.Images.Add(image);
        }

        public async void DeleteImage(Image image)
        {
             _context.Images.Remove(image);
        }

        public async Task<Image?> GetImageAsync(Guid id)
        {
            return await _context.Images.FirstOrDefaultAsync(x => x.Id == id);
        }

        public async Task<IEnumerable<Image>> GetImagesAsync(string ownerId)
        {
           return await _context.Images.Where(i => i.OwnerId == ownerId).OrderBy(i => i.Title).ToListAsync();
        }

        public async Task<bool> ImageExistsAsync(Guid id)
        {
            return await _context.Images.AnyAsync(i => i.Id == id);
        }

        public async Task<bool> IsImageOwnerAsync(Guid id, string ownerId)
        {
            return await (_context.Images.AnyAsync(i => i.Id == id && i.OwnerId == ownerId));
        }

        public async Task<bool> SaveChangesAsync()
        {
          return (await _context.SaveChangesAsync() >= 0);
        }

        public void UpdateImage(Image image)
        {
            throw new NotImplementedException();
        }
    }
}
