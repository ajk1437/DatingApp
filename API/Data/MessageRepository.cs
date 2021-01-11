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
    public class MessageRepository : IMessageRepository
    {
        private readonly DataContext dataContext;
        private readonly IMapper mapper;

        public MessageRepository(DataContext dataContext, IMapper mapper)
        {
            this.dataContext = dataContext;
            this.mapper = mapper;
        }

        public void AddMessage(Message message)
        {
            dataContext.Messages.Add(message);
        }

        public void DeleteMessage(Message message)
        {
            dataContext.Messages.Remove(message);
        }

        public async Task<Message> GetMessage(int id)
        {
            return await dataContext.Messages
                                    .Include(u => u.Sender)
                                    .Include(u => u.Recipient)
                                    .SingleOrDefaultAsync(x=>x.Id == id);
        }

        public async Task<PagedList<MessageDto>> GetMessagesForUser(MessageParams messageParams)
        {
            var query = dataContext.Messages.OrderByDescending(m => m.MessageSent).AsQueryable();

            query = messageParams.Container switch
            {
                "Inbox" => query.Where(u => u.Recipient.UserName == messageParams.Username && !u.RecipientDeleted),
                "Outbox" => query.Where(u => u.Sender.UserName == messageParams.Username && !u.SenderDeleted),
                _ => query.Where(u => u.Recipient.UserName == messageParams.Username && !u.RecipientDeleted && u.DateRead == null)
            };

            var messages = query.ProjectTo<MessageDto>(mapper.ConfigurationProvider);

            return await PagedList<MessageDto>.CreateAsync(messages, messageParams.PageNumber, messageParams.PageSize);
        }

        public async Task<IEnumerable<MessageDto>> GetMessagesThread(string currentUsername, string recipientUsername)
        {
            var messages = await dataContext.Messages.Include(u => u.Sender).ThenInclude(p => p.Photos)
                                                     .Include(u => u.Recipient).ThenInclude(p => p.Photos)
                                                     .Where(m => m.Recipient.UserName == currentUsername && !m.RecipientDeleted
                                                                 && m.Sender.UserName == recipientUsername
                                                                 || m.Recipient.UserName == recipientUsername
                                                                 && m.Sender.UserName == currentUsername && !m.SenderDeleted)
                                                     .OrderBy(m => m.MessageSent)
                                                     .ToListAsync();

            var unreadMessages = messages.Where(m => m.DateRead == null && m.Recipient.UserName == currentUsername).ToList();

            if(unreadMessages.Any())
            {
                foreach (var message in unreadMessages)
                {
                    message.DateRead = DateTime.Now;
                }

                await dataContext.SaveChangesAsync();
            }

            return mapper.Map<IEnumerable<MessageDto>>(messages);
        }

        public async Task<bool> SaveAllAsync()
        {
            return await dataContext.SaveChangesAsync() > 0;
        }
    }
}
