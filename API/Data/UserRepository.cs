using API.DTOs;
using API.Entities;
using API.Helpers;
using API.Interfaces;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace API.Data
{
    public class UserRepository : IUserRepository
    {
        private readonly DataContext context;
        private readonly IMapper mapper;

        public UserRepository(DataContext context, IMapper mapper)
        {
            this.context = context;
            this.mapper = mapper;
        }

        public async Task<MemberDto> GetMemberAsync(string username, bool isCurrentUser)
        {
            var query = context.Users
                .Where(x => x.UserName == username)
                .ProjectTo<MemberDto>(mapper.ConfigurationProvider)
                .AsQueryable();

            if (isCurrentUser)
                query = query.IgnoreQueryFilters();

            return await query.FirstOrDefaultAsync();
        }

        public async Task<PagedList<MemberDto>> GetMembersAsync(UserParams userParams)
        {
            var query = context.Users.AsQueryable();

            query = query.Where(u => u.UserName != userParams.CurrentUsername);
            query = query.Where(u => u.Gender == userParams.Gender);

            // logika: minAge = 20, maxAge 30;
            // min = 2020-30-1 = 1989, -1 jer mozda nije bio rodjendan 
            // max = 2020-20 = 2000
            // vrati sve rodjenje od 1989 do 2000
            var minDateOfBirth = DateTime.Today.AddYears(-userParams.MaxAge - 1);
            var maxDateOfBirth = DateTime.Today.AddYears(-userParams.MinAge);

            query = query.Where(u => u.DateOfBirth >= minDateOfBirth && u.DateOfBirth <= maxDateOfBirth);

            query = userParams.OrderBy switch
            {
                "created" => query.OrderByDescending(u => u.Created),

                //default
                _ => query.OrderByDescending(u => u.LastActive)
            };

            return await PagedList<MemberDto>.CreateAsync(
                query.ProjectTo<MemberDto>(mapper.ConfigurationProvider)
                     .AsNoTracking(),
                userParams.PageNumber,
                userParams.PageSize);
        }

        public async Task<AppUser> GetUserByIdAsync(int id)
        {
            return await context.Users.FindAsync(id);
        }

        public async Task<AppUser> GetUserByPhotoId(int photoId)
        {
            return await context.Users
                .Include(p => p.Photos)
                .IgnoreQueryFilters()
                .Where(p => p.Photos.Any(p => p.Id == photoId))
                .FirstOrDefaultAsync();
        }

        public async Task<AppUser> GetUserByUsernameAsync(string username)
        {
            return await context.Users
                .Include(p => p.Photos)
                .SingleOrDefaultAsync(x => x.UserName == username);
        }

        public async Task<string> GetUserGender(string username)
        {
            return await context.Users
                .Where(x => x.UserName == username)
                .Select(x => x.Gender)
                .FirstOrDefaultAsync();
        }

        public async Task<IEnumerable<AppUser>> GetUsersAsync()
        {
            return await context.Users
                .Include(p => p.Photos)
                .ToListAsync();
        }

        public void Update(AppUser user)
        {
            context.Entry(user).State = EntityState.Modified;
        }
    }
}
