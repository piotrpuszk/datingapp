using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using API.DTOs;
using API.Entities;
using API.Extensions;
using API.Helpers;
using API.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace API.Controllers
{
    public class AdminController : BaseApiController
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IPhotoService _photoService;

        public AdminController(UserManager<AppUser> userManager,
        IUnitOfWork unitOfWork,
        IPhotoService photoService)
        {
            _photoService = photoService;
            _unitOfWork = unitOfWork;
            _userManager = userManager;
        }

        [Authorize(Policy = "RequireAdminRole")]
        [HttpGet("users-with-roles")]
        public async Task<ActionResult> GetUsersWithRoles()
        {
            var users = await _userManager
            .Users
            .Include(x => x.UserRoles)
            .ThenInclude(x => x.Role)
            .OrderBy(x => x.UserName)
            .Select(x => new
            {
                x.Id,
                Username = x.UserName,
                Roles = x.UserRoles.Select(r => r.Role.Name).ToList()
            })
            .ToListAsync();
            return Ok(users);
        }

        [Authorize(Policy = "RequireAdminRole")]
        [HttpPost("edit-roles/{username}")]
        public async Task<ActionResult> EditRoles(string username, [FromQuery] string roles)
        {
            var selectedRoles = roles.Split(",").ToArray();
            var user = await _userManager.FindByNameAsync(username);
            if (user == null) return NotFound("Could not find user");
            var userRoles = await _userManager.GetRolesAsync(user);
            var result = await _userManager.AddToRolesAsync(user, selectedRoles.Except(userRoles));
            if (!result.Succeeded) return BadRequest("Failed to add to roles");
            result = await _userManager.RemoveFromRolesAsync(user, userRoles.Except(selectedRoles));
            if (!result.Succeeded) return BadRequest("Failed to remove from roles");
            return Ok(await _userManager.GetRolesAsync(user));
        }

        [Authorize(Policy = "ModeratePhotoRole")]
        [HttpGet("photos-to-moderate")]
        public async Task<ActionResult<IEnumerable<PhotoDto>>> GetPhotosForModeration(
            [FromQuery] PhotoParams photoParams)
        {
            var photos = await _unitOfWork.PhotoRepository.GetUnapprovedPhotos(photoParams);
            Response.AddPaginationHeader(photos.CurrentPage,
            photos.PageSize,
            photos.TotalCount,
            photos.TotalPages);
            return photos;
        }

        [Authorize(Policy = "ModeratePhotoRole")]
        [HttpPut("approve-photo")]
        public async Task<ActionResult> ApprovePhoto(int photoId)
        {
            var photo = await _unitOfWork.PhotoRepository.GetPhoto(photoId);
            if (photo == null) return NotFound("Photo not found");
            if (photo.IsApproved) return BadRequest("You cannot approve already approved photo");
            _unitOfWork.PhotoRepository.ApprovePhoto(photo);
            var user = await _unitOfWork.UserRepository.GetUserByPhotoId(photoId);
            if(user.Photos.FirstOrDefault(photo => photo.IsMain) == null)
            {
                photo.IsMain = true;
            }
            if (await _unitOfWork.Complete()) return Ok();
            return BadRequest("Failed to approve photo");
        }

        [Authorize(Policy = "ModeratePhotoRole")]
        [HttpDelete("delete-photo")]
        public async Task<ActionResult> DeletePhoto(int photoId)
        {
            var photo = await _unitOfWork.PhotoRepository.GetPhoto(photoId);
            if (photo == null) return NotFound("Photo not found");
            if (!string.IsNullOrEmpty(photo.PublicId))
            {
                var result = await _photoService.DeletePhotoAsync(photo.PublicId);
                if(result.Error != null) return BadRequest("Can't delete public photo"); 
            }
            _unitOfWork.PhotoRepository.DeletePhoto(photo);
            if(await _unitOfWork.Complete()) return Ok();
            return BadRequest("Deleting photo failed");
        }
    }
}