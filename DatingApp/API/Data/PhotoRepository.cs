using System.Linq;
using System.Threading.Tasks;
using API.DTOs;
using API.Entities;
using API.Helpers;
using API.Interfaces;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore;

namespace API.Data
{
    public class PhotoRepository : IPhotoRepository
    {
        private readonly DataContext _context;
        private readonly IMapper _mapper;

        public PhotoRepository(DataContext context,
        IMapper mapper)
        {
            _mapper = mapper;
            _context = context;
        }

        public void ApprovePhoto(Photo photo)
        {
            photo.IsApproved = true;
        }

        public async Task<Photo> GetPhoto(int photoId)
        {
            return await _context.Photos.IgnoreQueryFilters()
            .Where(photo => photo.Id == photoId)
            .SingleOrDefaultAsync();
        }

        public async Task<PagedList<PhotoDto>> GetUnapprovedPhotos(PhotoParams photoParams)
        {
            var query = _context.Photos
            .IgnoreQueryFilters()
            .Where(photo => !photo.IsApproved)
            .ProjectTo<PhotoDto>(_mapper.ConfigurationProvider)
            .AsNoTracking();
            
            return await PagedList<PhotoDto>
            .CreateAsync(query, photoParams.PageNumber, photoParams.PageSize);
        }

        public void DeletePhoto(Photo photo)
        {
            _context.Photos.Remove(photo);
        }
    }
}