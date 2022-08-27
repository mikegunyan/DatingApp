using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using API.DTOs;
using API.Entities;
using API.Extensions;
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
        public AdminController(UserManager<AppUser> userManager, IUnitOfWork unitOfWork, IPhotoService photoService)
        {
            _photoService = photoService;
            _unitOfWork = unitOfWork;
            _userManager = userManager;
        }

        [Authorize(Policy  = "RequireAdminRole")]
        [HttpGet("users-with-roles")]
        public async Task<ActionResult> GetUsersWithRoles()
        {
            var users = await _userManager.Users
                .Include(r => r.UserRoles)
                .ThenInclude(r => r.Role)
                .OrderBy(u => u.UserName)
                .Select(u => new
                {
                    u.Id,
                    Username = u.UserName,
                    Roles = u.UserRoles.Select(r => r.Role.Name).ToList()
                })
                .ToListAsync();
            return Ok(users);
        }

        [HttpPost("edit-roles/{username}")]
        public async Task<ActionResult> EditRoles(string username, [FromQuery] string roles)
        {
            var selectedRoles = roles.Split(",").ToArray();
            var user = await _userManager.FindByNameAsync(username);
            if (user == null) return NotFound("Unable to find user");
            var userRoles = await _userManager.GetRolesAsync(user);
            var result = await _userManager.AddToRolesAsync(user, selectedRoles.Except(userRoles));
            if (!result.Succeeded) return BadRequest("Failed to add role");
            result = await _userManager.RemoveFromRolesAsync(user, userRoles.Except(selectedRoles));
            if (!result.Succeeded) return BadRequest("Failed to remove role");
            return Ok(await _userManager.GetRolesAsync(user));
        }
        
        [Authorize(Policy  = "ModeratePhotoRole")]
        [HttpGet("photos-to-moderate")]
        public async Task<ActionResult> GetPhotosForModeration()
        {
            var photos = await _unitOfWork.UserRepository.GetUnApprovedPhotosAsync();
            return Ok(photos);
        }
        
        [Authorize(Policy  = "ModeratePhotoRole")]
        [HttpDelete("delete-photo/{photoId}")]
        public async Task<ActionResult> DeletePhoto(int photoId)
        {
            var photo = await _unitOfWork.UserRepository.GetPhoto(photoId);
            // var user = await _unitOfWork.UserRepository.GetUserByIdAsync(photo.AppUserId);
            // var admin = await _unitOfWork.UserRepository.GetUserByUsernameAsync(User.GetUsername());
            // var message = new Message
            // {
            //     Sender = admin,
            //     Recipient = user,
            //     SenderUsername = admin.UserName,
            //     RecipientUsername = user.UserName,
            //     Content = "Your photo was unable to be approved! It was in violation of our code of conduct!",
            // };
            if (photo == null) return NotFound();
            if (photo.PublicId != null)
            {
                var result = await _photoService.DeletePhotoAsync(photo.PublicId);
                if (result.Error != null) return BadRequest(result.Error.Message);
            }
            _unitOfWork.UserRepository.DeletePhoto(photo);
            // _unitOfWork.MessageRepository.AddMessage(message);
            
            // if (await _unitOfWork.Complete()) return Ok();
            // return BadRequest("Message not sent!");
            return Ok();
        }
        
        [Authorize(Policy  = "ModeratePhotoRole")]
        [HttpPut("approve-photo/{photoId}")]
        public async Task<ActionResult> ApprovePhoto(int photoId)
        {
            var photo = await _unitOfWork.UserRepository.GetPhoto(photoId);
            // var user = await _unitOfWork.UserRepository.GetUserByIdAsync(photo.AppUserId);
            // var admin = await _unitOfWork.UserRepository.GetUserByUsernameAsync(User.GetUsername());
            // var message = new Message
            // {
            //     Sender = admin,
            //     Recipient = user,
            //     SenderUsername = admin.UserName,
            //     RecipientUsername = user.UserName,
            //     Content = "Your photo was approved!",
            // };
            if (photo == null) return NotFound();
            photo.IsApproved = true;
            _unitOfWork.UserRepository.ApprovePhoto(photo);
            // _unitOfWork.MessageRepository.AddMessage(message);
            
            // if (await _unitOfWork.Complete()) return Ok();
            // return BadRequest("Message not sent!");
            return Ok();
        }
    }
}