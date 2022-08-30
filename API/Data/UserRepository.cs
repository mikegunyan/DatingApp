using System;
using System.Collections.Generic;
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
  public class UserRepository : IUserRepository
  {
    private readonly DataContext _context;
    private readonly IMapper _mapper;
    public UserRepository(DataContext context, IMapper mapper)
    {
      _mapper = mapper;
      _context = context;
    }

    public void ApprovePhoto(Photo photo)
    {
        _context.Photos.Update(photo);
    }

    public void DeletePhoto(Photo photo)
    {
        _context.Photos.Remove(photo);
    }

    public async Task<MemberDto> GetMemberAsync(string username, bool isCurrentUser = false)
    {
        var query = _context.Users
            .Where(x => x.UserName == username)
            .ProjectTo<MemberDto>(_mapper.ConfigurationProvider)
            .AsQueryable();

        if (isCurrentUser) query = query.IgnoreQueryFilters();
        return await query.FirstOrDefaultAsync();
    }

        public async Task<PagedList<MemberDto>> GetMembersAsync(UserParams userParams)
    {
      var query = _context.Users.AsQueryable();

      query = query.Where(u => u.UserName != userParams.CurrentUsername);
      if (userParams.Gender != "both") query = query.Where(u => u.Gender == userParams.Gender);

      var minDob = DateTime.UtcNow.AddYears(- userParams.MaxAge - 1);
      var maxDob = DateTime.UtcNow.AddYears(- userParams.MinAge);
      
      query = query.Where(u => u.DateOfBirth >= minDob && u.DateOfBirth <= maxDob)
        .Where(u => u.UserName != "admin");

      query = userParams.OrderBy switch
      {
        "created" => query.OrderByDescending(u => u.Created),
        _ => query.OrderByDescending(u => u.LastActive)
      };

      return await PagedList<MemberDto>.CreateAsync(query
          .ProjectTo<MemberDto>(_mapper.ConfigurationProvider)
          .AsNoTracking(),
          userParams.PageNumber, userParams.PageSize);
    }

    public async Task<Photo> GetPhoto(int photoId)
    {
        return await _context.Photos
            .IgnoreQueryFilters()
            .Where(p => p.Id == photoId)
            .FirstAsync();
    }

    public async Task<IEnumerable<UnApprovedDto>> GetUnApprovedPhotosAsync()
    {
        return await _context.Photos
            .IgnoreQueryFilters()
            .Where(p => !p.IsApproved)
            .Select(u => new UnApprovedDto
            {
                Id = u.Id,
                Url = u.Url,
                IsMain = u.IsMain,
                IsApproved = u.IsApproved,
                AppUserId = u.AppUserId,
                Username = u.Username
            }).ToListAsync();
    }

    public async Task<AppUser> GetUserByIdAsync(int id)
    {
        return await _context.Users.FindAsync(id);
    }

    public Task<MemberDto> GetUserByIdAsync()
    {
      throw new NotImplementedException();
    }

    public async Task<AppUser> GetUserByUsernameAsync(string username)
    {
        return await _context.Users
            .Include(p => p.Photos)
            .SingleOrDefaultAsync(x => x.UserName == username);
    }

    public async Task<string> GetUserGender(string username)
    {
        return await _context.Users
            .Where(x => x.UserName == username)
            .Select(x => x.Gender)
            .FirstOrDefaultAsync();
    }

    public async Task<IEnumerable<AppUser>> GetUsersAsync()
    {
        return await _context.Users
            .Include(p => p.Photos)
            .ToListAsync();
    }

    public void Update(AppUser user)
    {
      _context.Entry(user).State = EntityState.Modified;
    }
  }
}