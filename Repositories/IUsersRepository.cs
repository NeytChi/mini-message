using System.Collections.Generic;
using mini_message.Contract.Commands;
using mini_message.Contract.Queries;
using mini_message.Models;

namespace mini_message.Repositories
{
    public interface IUsersRepository
    {
        User AddUser(User user);
        void UpdateUser(User user);
        User GetUserByEmail(string email);
        User GetUserByToken(string token);
        User GetUserById(int id);
        User GetUserByPublicToken(string token);
        User GetUserByRecoveryToken(string token);
        User GetUserByHash(string hash);
        ICollection<User> GetNotBlockedUsers(GetNonBlockedUsersCommand command);
        ICollection<BlockedUser> GetBlockedUsers(int id);
        ICollection<Participant> GetParticipantsByUser(int id, ICollection<int> blockedUsers);
        ICollection<Message> GetMessagesByChat(string chatToken);
        void UpdateViewedMessages(ICollection<Message> messages, int userId);
        ChatRoom CreateChatRoom(ChatRoom room);
        Participant CreateParticipant(Participant participant);
        ChatRoom GetChatById(long id);
        ChatRoom GetChatByToken(string token);
        Participant GetParticipantByIds(int userId, int opposideId);
        Message CreateMessage(Message message);
        BlockedUser GetBlockedUser(int userId, int blockedUserId);
        BlockedUser CreateBlockedUser(BlockedUser blockedUser);
        ICollection<BlockedUserDto> GetBlockedUsersDto(int userId);
        void UpdateBlockedUser(BlockedUser blockedUser);
        Message GetMessageById(long id);
        void CreateComplaint(Complaint complaint);
    }
}