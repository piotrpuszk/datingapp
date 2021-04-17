using System.Threading.Tasks;
using API.DTOs;
using API.Entities;
using API.Helpers;

namespace API.Interfaces
{
    public interface IPhotoRepository
    {
        void ApprovePhoto(Photo photo);
        Task<PagedList<PhotoDto>> GetUnapprovedPhotos(PhotoParams photoParams);
        Task<Photo> GetPhoto(int photoId);
        void DeletePhoto(Photo photo);
    }
}