using API.Interfaces;
using AutoMapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace API.Data
{
    public class UniteOfWork : IUnitOfWork
    {
        private readonly DataContext dataContext;
        private readonly IMapper mapper;

        public UniteOfWork(DataContext dataContext, IMapper mapper)
        {
            this.dataContext = dataContext;
            this.mapper = mapper;
        }

        public IUserRepository UserRepository => new UserRepository(dataContext, mapper);

        public IMessageRepository MessageRepository => new MessageRepository(dataContext, mapper);

        public ILikeRepository LikeRepository => new LikeRepository(dataContext);

        public async Task<bool> Complite()
        {
            return await dataContext.SaveChangesAsync() > 0;
        }

        public bool HasChanges()
        {
            return dataContext.ChangeTracker.HasChanges();
        }
    }
}
