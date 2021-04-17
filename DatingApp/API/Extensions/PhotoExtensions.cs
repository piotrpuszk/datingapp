using System.Collections.Generic;
using System.Linq;
using API.DTOs;

namespace API.Extensions
{
    public static class PhotoExtensions
    {
        public static ICollection<PhotoDto> ApprovedOnly(this ICollection<PhotoDto> photos)
        {
            return photos.Where(photo => photo.IsApproved).ToList();
        }
    }
}