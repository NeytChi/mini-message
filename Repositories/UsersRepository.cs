using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using mini_message.Contract.Commands;
using mini_message.Contract.Queries;
using mini_message.Models;
using Z.EntityFramework.Plus;

namespace mini_message.Repositories
{
    public class UsersRepository : IUsersRepository
    {
        private Context _context;
        
        public UsersRepository(Context context)
        {
            _context = context;
        }

        public User AddUser(User user)
        {
            _context.Users.Add(user);
            _context.SaveChanges();
            return user;
        }
        public void UpdateUser(User user)
        {
            _context.Users.Update(user);
            _context.SaveChanges();
        }
        public User GetUserByEmail(string email) => _context.Users.FirstOrDefault(u => u.Email == email);
        public User GetUserByToken(string token) => _context.Users.FirstOrDefault(u => u.Token == token);
        public User GetUserById(int id) => _context.Users.FirstOrDefault(u => u.Id == id);
        public User GetUserByPublicToken(string token) => _context.Users.FirstOrDefault(u => u.PublicToken == token);
        public User GetUserByRecoveryToken(string token) => _context.Users.FirstOrDefault(u => u.RecoveryToken == token);
        public User GetUserByHash(string hash) => _context.Users.FirstOrDefault(u => u.Hash == hash);

        public ICollection<User> GetNotBlockedUsers(GetNonBlockedUsersCommand command) => _context.Users
            .Where(i => i.Id != command.Id
            && !command.BlockedUsers.Contains(i.Id) )
            .OrderBy(i => i.Id).Skip(command.Since * command.Count).Take(command.Count).ToList();

        public ICollection<BlockedUser> GetBlockedUsers(int id) => _context.BlockedUsers
            .Where(b => b.UserId == id && !b.Deleted).ToList();

        public ICollection<Participant> GetParticipantsByUser(int id, ICollection<int> blockedUsers)
            => _context.Participants
                .IncludeOptimized(p => p.OpposideUser)
                .IncludeOptimized(p => p.ChatRoom)
                .IncludeOptimized(p => p.ChatRoom.Messages)
                .Where(p => p.UserId == id && !blockedUsers.Contains(p.OpposideId))
                .ToList();

        public ICollection<Message> GetMessagesByChat(string chatToken)
            => _context.ChatRooms
                .IncludeOptimized(c => c.Messages)
                .Where(c => c.Token == chatToken)
                .Select(c => c.Messages)
                .FirstOrDefault();

        public void UpdateViewedMessages(ICollection<Message> messages, int userId)
        {
            if (messages.Count > 0)
            {
                if (!messages.Last().Viewed && messages.Last().UserId != userId)
                {
                    foreach (var message in messages)
                    {
                        message.Viewed = true;
                    }
                    UpdateMessages(messages);
                }   
            }
        }
        public void UpdateMessages(ICollection<Message> messages)
        {
            _context.Messages.UpdateRange(messages);
            _context.SaveChanges();
        }

        public ChatRoom CreateChatRoom(ChatRoom room)
        {
            _context.ChatRooms.Add(room);
            _context.SaveChanges();
            return room;
        }

        public Participant CreateParticipant(Participant participant)
        {
            _context.Participants.Add(participant);
            _context.SaveChanges();
            return participant;
        }
        public ChatRoom GetChatById(long id) => _context.ChatRooms.FirstOrDefault(c => c.Id == id);
        public ChatRoom GetChatByToken(string token) => _context.ChatRooms.FirstOrDefault(c => c.Token == token);

        public Participant GetParticipantByIds(int userId, int opposideId)
            => _context.Participants.FirstOrDefault(p => p.UserId == userId && p.OpposideId == opposideId);

        public Message CreateMessage(Message message)
        {
            _context.Messages.Add(message);
            _context.SaveChanges();
            return message;
        }

        public BlockedUser GetBlockedUser(int userId, int blockedUserId) => _context.BlockedUsers.FirstOrDefault(
            c => c.UserId == userId && c.BlockedUserId == blockedUserId && !c.Deleted);

        public ICollection<BlockedUserDto> GetBlockedUsersDto(int userId) => _context.BlockedUsers
            .IncludeOptimized(b => b.User)
            .IncludeOptimized(b => b.BlockUser)
            .Where(p => p.UserId == userId && !p.Deleted)
            .Select(p => new BlockedUserDto(p, p.BlockUser))
            .ToList();
        public BlockedUser CreateBlockedUser(BlockedUser blockedUser)
        {
            _context.BlockedUsers.Add(blockedUser);
            _context.SaveChanges();
            return blockedUser;
        }

        public void UpdateBlockedUser(BlockedUser blockedUser)
        {
            _context.BlockedUsers.Update(blockedUser);
            _context.SaveChanges();
        }

        public Message GetMessageById(long id) => _context.Messages.FirstOrDefault(m => m.Id == id);

        public void CreateComplaint(Complaint complaint)
        {
            _context.Complaints.Add(complaint);
            _context.SaveChanges();
        }
    }
}