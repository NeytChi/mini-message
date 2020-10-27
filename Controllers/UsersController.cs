using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using mini_message.Common;
using mini_message.Common.Settings;
using mini_message.Common.Tools;
using mini_message.Contract.Commands;
using mini_message.Contract.Queries;
using mini_message.Dtos;
using mini_message.Models;
using mini_message.Repositories;

namespace mini_message.Controllers
{
    [ApiController]
    [Route("v1.0/[controller]/[action]")]
    public class UsersController : ControllerBase
    {
        private readonly ILogger<UsersController> _logger;
        private readonly IUsersRepository _repository;
        private readonly ProfileCondition _profileCondition;
        private readonly IMailer _mailer;
        private readonly HostSettings _hostSettings;
        
        public UsersController(ILogger<UsersController> logger)
        {
            var configuration = ServerConfiguration.Get();
            _logger = logger;
            _profileCondition = new ProfileCondition(logger);
            var context = new Context(false);
            context.Database.EnsureCreated();
            _repository = new UsersRepository(context);
            _mailer = new Mailer(OperateLoggerFactory.Get(),
                    configuration.GetSection("SmtpSettings").Get<SmtpSettings>());
            _hostSettings = configuration.GetSection("HostSettings").Get<HostSettings>();
        }

        [HttpPost]
        public ActionResult Registration(RegistrationCommand command)
        {
            var message = string.Empty;
            if (_profileCondition.ValidateEmail(command.user_email))
            {
                if (_profileCondition.ValidatePassword(command.user_password, ref message))
                {
                    if (_repository.GetUserByEmail(command.user_email) == null)
                    {
                        var user = new User
                        {
                            Email = command.user_email,
                            Login = command.user_login,
                            Password = _profileCondition.HashPassword(command.user_password),
                            Hash = _profileCondition.GenerateHash(100),
                            CreatedAt = DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
                            Activate = 0,
                            LastLoginAt = DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
                            Token = _profileCondition.GenerateHash(40),
                            RecoveryCode = 0,
                            PublicToken = _profileCondition.GenerateHash(20),
                            Profile = new Profile()
                        };
                        user = _repository.AddUser(user); 
                        _mailer.SendEmail(user.Email, "Activate account", 
                            $"Activate account url: <a href=http://{_hostSettings.Ip}:{_hostSettings.PortHttp}" 
                                + $"/v1.0/users/Activate/?hash={user.Hash}>Activation url!</a>");
                        _logger.LogInformation("Create new user, id ->" + user.Id);
                        return Ok(new MessageResponse(true,
                            "User account was successfully registrate. See your email to activate account by url."));
                    }
                    else
                    {
                        message = "This email is already exists.";
                    }
                }
                else
                {
                    message = "Password not valid. " + message;
                }
            }
            else
            {
                message = $"Email -> {command.user_email} not valid.";
            }

            _logger.LogWarning(message);
            var response = new MessageResponse(false, message);
            return StatusCode(500, response);
        }

        [HttpPut]
        public ActionResult Login(LoginCommand command)
        {
            string message; User user;

            if ((user = _repository.GetUserByEmail(command.user_email)) != null)
            {
                if (_profileCondition.VerifyHashedPassword(user.Password, command.user_password))
                {
                    if (user.Activate == 1)
                    {
                        user.LastLoginAt = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
                        _repository.UpdateUser(user);
                        var login = new LoginDto(user);
                        _logger.LogInformation($"Login user, id -> {user.Id}.");
                        return Ok(new DataResponse(true, login));
                    }
                    else
                    {
                        _mailer.SendEmail(user.Email, "Activate account",
                            $"Activate account url: <a href=http://{_hostSettings.Ip}:{_hostSettings.PortHttp}"
                            + "/v1.0/users/Activate/?hash=" + user.Hash + ">Activation url!</a>");
                        message = "User account is not activate.";
                    }
                }
                else
                {
                    message = "Wrong password.";
                }
            }
            else
            {
                message = "No user with such email.";
            }

            _logger.LogWarning(message);
            var response = new MessageResponse(false, message);
            return StatusCode(500, response);
        }

        [HttpPut]
        public ActionResult LogOut(LogOutCommand command)
        {
            string message; User user;
            
            if ((user = _repository.GetUserByToken(command.user_token)) != null)
            {
                user.Token = _profileCondition.GenerateHash(40);
                _repository.UpdateUser(user);
                _logger.LogInformation($"Log out user, id -> {user.Id}.");
                return Ok(new MessageResponse(true, "Log out is successfully."));
            }
            else
            {
                message = "Server can't define user by token.";
            }
            _logger.LogWarning(message);
            var response = new MessageResponse(false, message);
            return StatusCode(500, response);
        }

        [HttpPost]
        public ActionResult RecoveryPassword(RecoveryPasswordCommand command)
        {
            string message; User user;
            if ((user = _repository.GetUserByEmail(command.user_email)) != null)
            {
                user.RecoveryCode = _profileCondition.random.Next(100000, 999999);
                _repository.UpdateUser(user);
                _mailer.SendEmail(user.Email, "Recovery password", "Recovery code =" + user.RecoveryCode);
                _logger.LogInformation($"Recovery password, id -> {user.Id}.");
                return Ok(new MessageResponse(true,$"Recovery password. Send message with code to email -> {user.Email}."));
            }
            else
            {
                message = $"User with email -> {command.user_email} doesn't exist.";
            }
            _logger.LogWarning(message);
            var response = new MessageResponse(false, message);
            return StatusCode(500, response);
        }

        [HttpPost]
        public ActionResult CheckRecoveryCode(CheckRecoveryCodeCommand command)
        {
            string message; User user;

            if ((user = _repository.GetUserByEmail(command.user_email)) != null)
            {
                if (user.RecoveryCode == command.recovery_code)
                {
                    user.RecoveryToken = _profileCondition.GenerateHash(40);
                    user.RecoveryCode = 0;
                    _repository.UpdateUser(user);
                    _logger.LogInformation($"Check recovery code, id -> {user.Id}.");
                    return Ok(new RecoveryTokenDto(user.RecoveryToken));
                }
                else
                {
                    message = "Recovery code doesn't match with server's code.";
                }
            }
            else
            {
                message = $"User with email -> {command.user_email} doesn't exist.";
            }

            _logger.LogWarning(message);
            var response = new MessageResponse(false, message);
            return StatusCode(500, response);
        }

        [HttpPost]
        public ActionResult ChangePassword(ChangePasswordCommand command)
        {
            var message = ""; User user;

            if ((user = _repository.GetUserByRecoveryToken(command.recovery_token)) != null)
            {
                if (_profileCondition.EqualsPasswords(command.user_password, command.user_confirm_password))
                {
                    if (_profileCondition.ValidatePassword(command.user_password, ref message))
                    {
                        user.Password = _profileCondition.HashPassword(command.user_password);
                        user.RecoveryToken = "";
                        _repository.UpdateUser(user);
                        _logger.LogInformation($"Change user password, id -> {user.Id}.");
                        return Ok(new MessageResponse(true, $"Change user password, id -> {user.Id}."));
                    }
                    else
                    {
                        message = "Validation password - unsuccessfully. " + message;
                    }
                }
                else
                {
                    message = "Password are not match to each other.";
                }
            }
            else
            {
                message = "Can't find user by recovery token.";
            }
            
            _logger.LogWarning(message);
            var response = new MessageResponse(false, message);
            return StatusCode(500, response);
        }
        [HttpPost]
        public ActionResult RegistrationEmail(RegistrationEmailCommand command)
        {
            string message; User user;
            
            if ((user = _repository.GetUserByEmail(command.user_email)) != null)
            {
                _mailer.SendEmail(user.Email, "Activate account", 
            $"Activate account url: <a href=http://{_hostSettings.Ip}:{_hostSettings.PortHttp}/v1.0/users/Activate/" 
                + $"?hash={user.Hash}>Activation url!</a>");
                _logger.LogInformation($"Send registration email to user, id -> {user.Id}");
                return Ok(new MessageResponse(true, 
                    "Send registration email to user. See your email to activate account by url."));
            }
            else
            {
                message = "Can't define user by email.";
            }
            
            _logger.LogWarning(message);
            var response = new MessageResponse(false, message);
            return StatusCode(500, response);
        }
        [HttpPost]
        public ActionResult Delete(DeleteCommand command)
        {
            string message; User user;
            
            if ((user = _repository.GetUserByToken(command.user_token)) != null)
            {
                user.Token = "";
                _repository.UpdateUser(user);
                _logger.LogInformation($"Delete user, id -> {user.Id}.");
                return Ok(new MessageResponse(true, $"Delete user, id -> {user.Id}."));
            }
            else
            {
                message = "Server can't define user by token.";
            }
            _logger.LogWarning(message);
            var response = new MessageResponse(false, message);
                return StatusCode(500, response);
        }
        [HttpGet]
        public ActionResult Activate([FromQuery] string hash)
        {
            string message; User user;
            
            if ((user  = _repository.GetUserByHash(hash)) != null)
            {
                user.Activate = 1;
                _repository.UpdateUser(user);
                _logger.LogInformation($"Active user, id -> {user.Id}.");
                return Ok(new MessageResponse(true, "User account active."));
            }
            else
            {
                message = "Server can't define user by hash.";
            }
            
            _logger.LogWarning(message);
            var response = new MessageResponse(false, message);
            return StatusCode(500, response);
        }
        /*[HttpPost]
        public ActionResult UpdateProfile(UpdateProfileCommand command)
        {
            string message; User user;

            if ((user = _repository.GetUserByToken(command.user_token)) != null)
            {
                var file = new UploadFile();
                if (request.GetFileRequest(ref file))
                {
                    if (file.file_type == "image")
                    {
                        var profile = new Profile();
                        Database.profile.SelectByUserId(user.user_id, ref profile);
                        LoaderFile.SaveFile(ref file, "/ProfilePhoto/");
                        profile.url_photo = "http://" + domen + file.file_path + file.file_name;
                        Database.profile.UpdateUrlPhoto(user.user_id, profile.url_photo);
                        request.ResponseJsonData(profile);
                        Logger.WriteLog("Update profile photo", LogLevel.Usual);
                    }
                    else { message = "File type is not correct."; }
                }
                else { message = "Can't get file from request."; }
            }
            else
            {
                message = "Server can't define user by token.";
            }
            _logger.LogWarning(message);
            var response = new MessageResponse(false, message);
            return StatusCode(500, response);
        }*/
        [HttpPut]
        public ActionResult GetUsersList(GetUsersListCommand command)
        {
            string message; User user;

            if ((user = _repository.GetUserByToken(command.user_token)) != null)
            {
                var dto = new List<UserDto>();
                var users = _repository.GetNotBlockedUsers(new GetNonBlockedUsersCommand
                {
                    BlockedUsers = _repository.GetBlockedUsers(user.Id).Select(b => b.BlockedUserId).ToList(),
                    Count = 100000,
                    Since = 0,
                    Id = user.Id
                });
                foreach(var u in users)
                    dto.Add(new UserDto(u));
                
                _logger.LogInformation($"Get users list by user, id -> {user.Id}.");
                return Ok(new DataResponse(true, dto));
            }
            else
            {
                message = "Server can't define user by token.";
            }
            _logger.LogWarning(message);
            var response = new MessageResponse(false, message);
            return StatusCode(500, response);
        }
        [HttpPut]
        public ActionResult SelectChats(SelectChatsCommand command)
        {
            string message; User user;

            if ((user = _repository.GetUserByToken(command.user_token)) != null)
            {
                var chats = new List<ChatDto>();
                var blockedUsers = _repository.GetBlockedUsers(user.Id).Select(b => b.BlockedUserId).ToList();
                var participants = _repository.GetParticipantsByUser(user.Id, blockedUsers);
                foreach (var participant in participants)
                {
                    chats.Add(new ChatDto
                    {
                        chat = new ChatRoomDto(participant.ChatRoom),
                        user = new UserDto(participant.OpposideUser),
                        last_message = participant.ChatRoom.Messages.LastOrDefault() != null ?
                            new MessageDto(participant.ChatRoom.Messages.LastOrDefault()) : null
                    });
                }
                _logger.LogInformation($"Get chat list by user, id -> {user.Id}.");
                return Ok(new DataResponse(true, chats));
            }
            else
            {
                message = "Server can't define user by token.";
            }
            _logger.LogWarning(message);
            var response = new MessageResponse(false, message);
            return StatusCode(500, response);
        }
        [HttpPut]
        public ActionResult SelectMessages(SelectMessagesCommand command)
        {
            string message; User user;

            if ((user = _repository.GetUserByToken(command.user_token)) != null)
            {
                var messages = _repository.GetMessagesByChat(command.chat_token);
                if (messages != null)
                {
                    var messagesDto = new List<MessageDto>();
                    foreach (var m in messages)
                    {
                        messagesDto.Add(new MessageDto(m));
                    }
                    var dataResponse = new DataResponse(true, messagesDto);
                    _repository.UpdateViewedMessages(messages, user.Id);
                    _logger.LogInformation($"Get messages by chat & user, id -> {user.Id}.");
                    return Ok(dataResponse);
                }
                else
                {
                    message = "Server can't define chat by token.";
                }
            }
            else
            {
                message = "Server can't define user by token.";
            }
            _logger.LogWarning(message);
            var response = new MessageResponse(false, message);
            return StatusCode(500, response);
        }
        [HttpPost]
        public ActionResult CreateChat(CreateChatCommand command)
        {
            string message; User user; ChatRoom room;

            if ((user = _repository.GetUserByToken(command.user_token)) != null)
            {
                var opposideUser = _repository.GetUserByPublicToken(command.opposide_public_token);
                if (opposideUser != null)
                {
                    var participant = _repository.GetParticipantByIds(user.Id, opposideUser.Id);
                    if (participant == null)
                    {
                        room = new ChatRoom
                        {
                            Token = _profileCondition.GenerateHash(20),
                            CreatedAt = DateTimeOffset.UtcNow
                        };
                        room = _repository.CreateChatRoom(room);
                        var userParticipant = new Participant
                        {
                            ChatId = room.Id,
                            UserId = user.Id,
                            OpposideId = opposideUser.Id
                        };
                        var opposideParticipant = new Participant
                        {
                            ChatId = room.Id,
                            UserId = opposideUser.Id,
                            OpposideId = user.Id
                        };
                        _repository.CreateParticipant(userParticipant);
                        _repository.CreateParticipant(opposideParticipant);
                        _logger.LogInformation($"Create chat for user, id -> {user.Id} & opposide, id -> {opposideUser.Id}.");
                    }
                    else
                    {
                        room = _repository.GetChatById(participant.ChatId);
                        _logger.LogInformation($"Select exist chat for user, id -> {user.Id} & opposide, id -> {opposideUser.Id}.");
                    }
                    return Ok(new ChatRoomDto(room));
                }
                else
                {
                    message = "Server can't define interlocutor by token.";
                }
            }
            else
            {
                message = "Server can't define user by token.";
            }
            _logger.LogWarning(message);
            var response = new MessageResponse(false, message);
            return StatusCode(500, response);
        }
        [HttpPost]
        public ActionResult SendMessage(SendMessageCommand command)
        {
            string error;
            User user;

            if ((user = _repository.GetUserByToken(command.user_token)) != null)
            {
                if (command.message_text != String.Empty)
                {
                    var room = _repository.GetChatByToken(command.chat_token);
                    if (room != null)
                    {
                        var message = new Message
                        {
                            ChatId = room.Id,
                            UserId = user.Id,
                            Text = command.message_text,
                            Viewed = false,
                            CreatedAt = DateTimeOffset.UtcNow
                        };
                        _repository.CreateMessage(message);
                        _logger.LogInformation($"Create new message, chat id -> {room.Id}.");
                        return Ok(new DataResponse(true, new MessageDto(message)));
                    }
                    else
                    {
                        error = "Server can't define chat by token.";
                    }
                }
                else
                {
                    error = "Message is empty. Server wouldn't upload this message.";
                }
            }
            else
            {
                error = "Server can't define user by token.";
            }

            _logger.LogWarning(error);
            var response = new MessageResponse(false, error);
            return StatusCode(500, response);
        }
        [HttpPost]
        public ActionResult BlockUser(BlockUserCommand command)
        {
            string error;
            User user;

            if ((user = _repository.GetUserByToken(command.user_token)) != null)
            {
                var interlocutor = _repository.GetUserByPublicToken(command.opposide_public_token);
                if (interlocutor != null)
                {
                    if (command.blocked_reason.Length < 100)
                    {
                        var blockedUser = _repository.GetBlockedUser(user.Id, interlocutor.Id);
                        if (blockedUser == null)
                        {
                            blockedUser = new BlockedUser
                            {
                                UserId = user.Id,
                                BlockedUserId = interlocutor.Id,
                                Reason = command.blocked_reason,
                                Deleted = false
                            };
                            _repository.CreateBlockedUser(blockedUser);
                            _logger.LogInformation($"Block user by user, id -> {user.Id}.");
                            return Ok(new MessageResponse(true, "User was blocked."));
                        }
                        else
                        {
                            error = "User blocked current user.";
                        }
                    }
                    else
                    {
                        error = "Reason message can't be longer than 100 characters.";
                    }
                }
                else
                {
                    error = "No user with that opposide_public_token.";
                }
            }
            else
            {
                error = "Server can't define user by token.";
            }

            _logger.LogWarning(error);
            var response = new MessageResponse(false, error);
            return StatusCode(500, response);
        }
        [HttpPut]
        public ActionResult GetBlockedUsers(GetBlockedUsersCommand command)
        {
            string error;
            User user;

            if ((user = _repository.GetUserByToken(command.user_token)) != null)
            {
                var blockedUsers = _repository.GetBlockedUsersDto(user.Id);
                _logger.LogInformation($"Get blocked users by user, id -> {user.Id}.");
                return Ok(new DataResponse(true, blockedUsers));
            }
            else
            {
                error = "Server can't define user by token.";
            }

            _logger.LogWarning(error);
            var response = new MessageResponse(false, error);
                return StatusCode(500, response);
        }
        [HttpPost]
        public ActionResult UnblockUser(UnblockUserCommand command)
        {
            string error;
            User user;

            if ((user = _repository.GetUserByToken(command.user_token)) != null)
            {
                var interlocutor = _repository.GetUserByPublicToken(command.opposide_public_token);
                if (interlocutor != null)
                {
                    var blockedUser = _repository.GetBlockedUser(user.Id, interlocutor.Id);
                    if (blockedUser != null)
                    {
                        blockedUser.Deleted = true;
                        _repository.UpdateBlockedUser(blockedUser);
                        _logger.LogInformation($"Delete blocked user, id -> {user.Id}.");
                        return Ok(new MessageResponse(true, $"Unblock user by user, id -> {user.Id}."));
                    }
                    else
                    {
                        error = $"User didn't block current user, id -> {user.Id}.";
                    }
                }
                else
                {
                    error = "Server can't define user by public token.";
                }
            }
            else
            {
                error = "Server can't define user by token.";
            }

            _logger.LogWarning(error);
            var response = new MessageResponse(false, error);
            return StatusCode(500, response);
        }
        [HttpPost]
        public ActionResult ComplaintContent(ComplaintContentCommand command)
        {
            string error;
            User user;

            if ((user = _repository.GetUserByToken(command.user_token)) != null)
            {
                var message = _repository.GetMessageById(command.message_id);
                if (message != null)
                {
                    if (command.complaint.Length < 100)
                    {
                        if (message.UserId != user.Id)
                        {
                            var interlocutor = _repository.GetUserById(message.UserId);
                            if (interlocutor != null)
                            {
                                var blockedUser = _repository.GetBlockedUser(user.Id, interlocutor.Id);
                                if (blockedUser == null)
                                {
                                    blockedUser = new BlockedUser
                                    {
                                        UserId = user.Id,
                                        BlockedUserId = interlocutor.Id,
                                        Reason = command.complaint,
                                        Deleted = false
                                    };
                                    blockedUser = _repository.CreateBlockedUser(blockedUser);
                                    var complaintUser = new Complaint
                                    {
                                        UserId = user.Id,
                                        BlockedId = blockedUser.Id,
                                        MessageId = message.Id,
                                        ComplaintMessage = command.complaint,
                                        CreatedAt = DateTimeOffset.UtcNow
                                    };
                                    _repository.CreateComplaint(complaintUser);
                                    _logger.LogInformation($"Create complaint, id -> {user.Id}.");
                                    return Ok(new MessageResponse(true, $"Create complaint, id -> {user.Id}."));
                                }
                                else
                                {
                                    error = "User blocked current user.";
                                }
                            }
                            else
                            {
                                error = "Server can't define user.";
                            }
                        }
                        else
                        {
                            error = "User can't complain on himself.";
                        }
                    }
                    else
                    {
                        error = "Complaint message can't be longer than 100 characters.";
                    }
                }
                else
                {
                    error = "Unknow message_id. Server can't define message.";
                }
            }
            else
            {
                error = "Server can't define user by token.";
            }

            _logger.LogWarning(error);
            var response = new MessageResponse(false, error);
            return StatusCode(500, response);
        }
    }
}
