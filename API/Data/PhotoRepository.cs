using API.DTOs;
using API.Entities;
using API.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace API.Data
{
    public class PhotoRepository : IPhotoRepository
    {
        private readonly DataContext dataContext;

        public PhotoRepository(DataContext dataContext)
        {
            this.dataContext = dataContext;
        }

        public async Task<Photo> GetPhotoById(int id)
        {
            return await dataContext.Photos
                .IgnoreQueryFilters()
                .SingleOrDefaultAsync(x => x.Id == id);
        }

        public async Task<IEnumerable<PhotoForApprovalDto>> GetUnapprovedPhotos()
        {
            return await dataContext.Photos
                .IgnoreQueryFilters()
                .Where(x => x.IsApproved == false)
                .Select(x => new PhotoForApprovalDto
                {
                    Id = x.Id,
                    Username = x.AppUser.UserName,
                    Url = x.Url,
                    IsApproved = x.IsApproved
                })
                .ToListAsync();
        }

        public void RemovePhoto(Photo photo)
        {
            dataContext.Photos.Remove(photo);
        }
    }
}
