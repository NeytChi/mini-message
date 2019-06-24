using System;
using Common.Routing;
using Common.Logging;
using Common.NDatabase;
using Common.Functional.Pass;
using Common.Functional.Mail;
using Common.NDatabase.UserData;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using MiniMessanger.Models;
using Common.NDatabase.FileData;
using Common.FileSystem;
using Common.Chats;
using MiniMessanger.Models.Chat;

namespace Common.Functional.UserF
{
    /// <summary>
    /// User functional for general movement. This class will be generate functional for user ability.
    /// </summary>
    public class UsersController
    {
        private string domen = "none";
        private DateTime unixed = new DateTime(1970, 1, 1, 0, 0, 0);

        public UsersController(string domen)
        {
            this.domen = domen;
            Router.AddRoute(new Route("POST", "users/Registration", Registration));
            Router.AddRoute(new Route("PUT", "users/Login", Login));
            Router.AddRoute(new Route("PUT", "users/LogOut", LogOut));
            Router.AddRoute(new Route("POST", "users/RecoveryPassword", RecoveryPassword));
            Router.AddRoute(new Route("POST", "users/CheckRecoveryCode", CheckRecoveryCode));
            Router.AddRoute(new Route("POST", "users/ChangePassword", ChangePassword));
            Router.AddRoute(new Route("POST", "users/Delete", Delete));
            Router.AddRoute(new Route("GET", "users/Activate", Activate));
            Router.AddRoute(new Route("POST", "users/UpdateProfile", UpdateProfile)); 
            Router.AddRoute(new Route("PUT", "users/GetUsersList", GetUsersList));
            Router.AddRoute(new Route("PUT", "users/SelectChats", SelectChats));
            Router.AddRoute(new Route("PUT", "users/SelectMessages", SelectMessages));
        }
        /// <summary>
        /// Registration user by specified request.
        /// </summary>
        /// <param name="request">Request.</param>
        public void Registration(ref HttpRequest request)
        {
            string message = string.Empty;
            UserCache user = new UserCache();
            user.user_email = request.RequiredJsonField("user_email", JTokenType.String);
            if (user.user_email == null) return;
            user.user_login = request.RequiredJsonField("user_login", JTokenType.String);
            if (user.user_login == null) return;
            user.user_password = request.RequiredJsonField("user_password", JTokenType.String);
            if (user.user_password == null) return;
            if (Validator.ValidateEmail(ref user.user_email))
            {
                if (Validator.ValidatePassword(ref user.user_password, ref message))
                {
                    if (!Database.user.CheckUserByEmail(user.user_email))
                    {
                        user.user_password = Validator.HashPassword(ref user.user_password);
                        user.user_hash = Validator.GenerateHash(100);
                        user.created_at = (int)(DateTime.Now - unixed).TotalSeconds;
                        user.activate = 0;
                        user.last_login_at = user.created_at;
                        user.user_token = Validator.GenerateHash(40);
                        user.recovery_code = 0;
                        user.user_public_token = Validator.GenerateHash(20);
                        Database.user.AddUser(ref user);
                        ProfileData profile = new ProfileData();
                        profile.user_id = user.user_id;
                        Database.profile.AddProfile(ref profile);
                        user.profile = profile;
                        MailF.SendEmail(user.user_email, "Activate account", "Activate account url: <a href=http://" + Config.IP + ":" + Config.Port + "/v1.0/users/Activate/?hash=" + user.user_hash + ">Activation url!</a>");
                        request.ResponseJsonAnswer(true, "User account was successfully registrate. See your email to activate account by url.");
                        Logger.WriteLog("Registrate new user, user_id=" + user.user_id, LogLevel.Usual);
                        return;
                    }
                    else { message = "Have exists account with this email."; }
                }
                else { message = "Validation password - unsuccessfully. " + message; }
            }
            else { message = "Validating email=" + user.user_email + " false"; }
            Logger.WriteLog(message, LogLevel.Warning);
            request.ResponseJsonAnswer(false, message);
        }
        public void Login(ref HttpRequest request)
        {
            string message = null;
            UserCache user = new UserCache();
            user.user_email = request.RequiredJsonField("user_email", JTokenType.String);
            if (user.user_email == null) return;
            user.user_password = request.RequiredJsonField("user_password", JTokenType.String);
            if (user.user_password == null) return;
            string insert_password = user.user_password;
            if (Database.user.SelectUserByEmail(user.user_email, ref user))
            {
                if (Validator.VerifyHashedPassword(ref user.user_password, ref insert_password))
                {
                    if (user.activate == 1)
                    {
                        user.user_password = null;
                        Database.user.UpdateLastLoginAt(user.user_id, (int)(DateTime.Now - unixed).TotalSeconds);
                        request.ResponseJsonData(user);
                        Logger.WriteLog("Login user, user_id=" + user.user_id, LogLevel.Usual);
                        return;
                    }
                    else { message = "User account is not activate."; }
                }
                else { message = "Wrong password."; }
            }
            else { message = "No user with such email."; }
            request.ResponseJsonAnswer(false, message);
            Logger.WriteLog(message, LogLevel.Warning);
        }
        public void LogOut(ref HttpRequest request)
        {
            string message = null;
            UserCache user = new UserCache();
            string user_token = request.FormField("user_token");
            if (Database.user.SelectUserByToken(user_token, ref user))
            {
                user.user_token = Validator.GenerateHash(40);
                Database.user.UpdateUserToken(user.user_id, user.user_token);
                request.ResponseJsonAnswer(true, "Log out is successfully.");
                Logger.WriteLog("LogOut user, user_id=" + user.user_id, LogLevel.Usual);
                return;
            }
            else
            {
                message = "Server can't get user_token from request.";
            }
            request.ResponseJsonAnswer(false, message);
            Logger.WriteLog(message, LogLevel.Warning);
        }
        public void RecoveryPassword(ref HttpRequest request)
        {
            string user_email = request.FormField("user_email");
            UserCache user = new UserCache();
            if (Database.user.SelectUserByEmail(user_email, ref user))
            {
                user.recovery_code = Validator.random.Next(100000, 999999);
                MailF.SendEmail(user.user_email, "Recovery password", "Recovery code=" + user.recovery_code);
                Database.user.UpdateRecoveryCode(user.user_id, user.recovery_code);
                request.ResponseJsonAnswer(true, "Recovery password. Send message with code to email=" + user.user_email);
                Logger.WriteLog("Recovery password, user_id=" + user.user_id, LogLevel.Usual);
            }
            else
            {
                request.ResponseJsonAnswer(false, "No user with that email.");
                Logger.WriteLog("No user with that email.", LogLevel.Error);
            }
        }
        public void CheckRecoveryCode(ref HttpRequest request)
        {
            string message = null;
            int recovery_code = 0;
            if (Int32.TryParse(request.FormField("recovery_code"), out recovery_code) && recovery_code != 0)
            {
                UserCache user = new UserCache();
                user.user_email = request.FormField("user_email");
                if (Database.user.SelectUserByEmail(user.user_email, ref user))
                {
                    if (user.recovery_code == recovery_code)
                    {
                        user.recovery_token = Validator.GenerateHash(40);
                        Database.user.UpdateRecoveryToken(user.user_id, user.recovery_token);
                        Database.user.UpdateRecoveryCode(user.user_id, 0);
                        request.ResponseJsonData(user.recovery_token);
                        Logger.WriteLog("Check recovery code - successfully", LogLevel.Usual);
                        return;
                    }
                    else { message = "Recovery code doesn't match with server's code."; }
                }
                else { message = "Can't find this user by user_email."; }
            }
            else { message = "Server can't get recovery_code from request."; }
            request.ResponseJsonAnswer(false, message);
            Logger.WriteLog(message, LogLevel.Warning);
        }
        public void ChangePassword(ref HttpRequest request)
        {
            string message = null;
            string recovery_token = request.FormField("recovery_token");
            UserCache user = new UserCache();
            if (Database.user.SelectUserByRecoveryToken(recovery_token, ref user))
            {
                string user_password = request.FormField("user_password");
                string user_confirm_password = request.FormField("user_confirm_password");
                if (Validator.EqualsPasswords(ref user_password, ref user_confirm_password))
                {
                    if (Validator.ValidatePassword(ref user_password, ref message))
                    {
                        user.user_password = Validator.HashPassword(ref user_password);
                        Database.user.UpdateRecoveryToken(user.user_id, "");
                        Database.user.UpdateUserPassword(user.user_id, user.user_password);
                        Logger.WriteLog("Change user password, user_id=" + user.user_id, LogLevel.Usual);
                        request.ResponseJsonAnswer(true, "Change user password, user_id=" + user.user_id);
                    }
                    else { message = "Validation password - unsuccessfully. " + message; }
                }
                else { message = "Password are not match to each other."; }
            }
            else { message = "Can't find user by recovery_token. Try again get request CheckRecoveryCode."; }
            request.ResponseJsonAnswer(false, message);
            Logger.WriteLog(message, LogLevel.Warning);
        }
        public void Delete(ref HttpRequest request)
        {
            string user_token = request.FormField("user_token");
            UserCache user = new UserCache();
            if (Database.user.SelectUserByToken(user_token, ref user))
            {
                Database.user.DeleteUser(user.user_id);
                request.ResponseJsonAnswer(true, "Delete user with user_id=" + user.user_id);
                Logger.WriteLog("Delete user with user_id=" + user.user_id, LogLevel.Usual);
            }
            else
            {
                request.ResponseJsonAnswer(false, "No user with that user_token.");
                Logger.WriteLog("No user with that user_token.", LogLevel.Warning);
            }
        }
        public void Activate(ref HttpRequest request)
        {
            string hash = request.HeadParameter("hash");
            if (string.IsNullOrEmpty(hash))
            {
                request.ResponseJsonAnswer(false, "Request doesn't contains head parameter hash.");
                Logger.WriteLog("Can not get hash from url parameters", LogLevel.Error);
                return;
            }
            if (Database.user.UpdateActivateUser(hash))
            {
                Logger.WriteLog("Active user account.", LogLevel.Usual);
                request.ResponseJsonAnswer(true, "User account is successfully active.");
            }
            else
            {
                Logger.WriteLog("Can't activate account. Unknow hash in request parameters.", LogLevel.Warning);
                request.ResponseJsonAnswer(false, "Can't activate account. Unknow hash in request parameters.");
            }
        }
        public void UpdateProfile(ref HttpRequest request)
        {
            string message = null;
            string user_token = request.FormField("user_token");
            UserCache user = new UserCache();
            if (Database.user.SelectUserByToken(user_token, ref user))
            {
                FileD file = new FileD();
                if (request.GetFileRequest(ref file))
                {
                    Console.WriteLine(file.file_type);
                    if (file.file_type == "image")
                    {
                        ProfileData profile = new ProfileData();
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
            else { message = "No user with that user_token."; }
            request.ResponseJsonAnswer(false, message);
            Logger.WriteLog(message, LogLevel.Warning);
        }
        public void GetUsersList(ref HttpRequest request)
        {
            string message = null;
            string user_token = request.FormField("user_token");
            UserCache user = new UserCache();
            if (Database.user.SelectUserByToken(user_token, ref user))
            {
                int page = 0;
                Int32.TryParse(request.HeadParameter("page"), out page);
                List<UserCache> users = Database.user.SelectUsers(user.user_id, page * 30, 30);
                for(int i = 0; i < users.Count; i++)
                {
                    UserCache cache = users[i];
                    ProfileData profile = new ProfileData();
                    Database.profile.SelectByUserId(cache.user_id, ref profile);
                    cache.profile = profile;
                    users[i] = cache;
                }
                request.ResponseJsonData(users);
                Logger.WriteLog("Get users list.", LogLevel.Usual);
                return;
            }
            else { message = "No user with that user_token."; }
            request.ResponseJsonAnswer(false, message);
            Logger.WriteLog(message, LogLevel.Warning);
        }
        /// <summary>
        /// Select list of chats. Get last message data, user's data of chat and chat data.
        /// </summary>
        /// <param name="request">Request.</param>
        public void SelectChats(ref HttpRequest request)
        {
            string message = null;
            string user_token = request.FormField("user_token");
            UserCache user = new UserCache();
            if (Database.user.SelectUserByToken(user_token, ref user))
            {
                int page = 0;
                Int32.TryParse(request.HeadParameter("page"), out page);
                List<ChatData> chats = new List<ChatData>();
                List<Participant> participants = Database.participant.SelectParticipantByUserId(user.user_id);
                foreach (Participant participant in participants)
                {
                    ChatData data = new ChatData();
                    ChatRoom room = new ChatRoom();
                    Database.chat.SelectChatById(participant.chat_id, ref room);
                    data.chat = room;
                    data.user = Database.user.SelectUserForChat(participant.opposide_id);
                    data.last_message = Database.message.SelectLastMessage(room.chat_id);
                    chats.Add(data);
                }
                request.ResponseJsonData(chats);
                Logger.WriteLog("Get list of chats.", LogLevel.Usual);
                return;
            }
            else { message = "No user with that user_token."; }
            request.ResponseJsonAnswer(false, message);
            Logger.WriteLog(message, LogLevel.Warning);
        }
        public void SelectMessages(ref HttpRequest request)
        {
            string message = null;
            string user_token = request.FormField("user_token");
            UserCache user = new UserCache();
            if (Database.user.SelectUserByToken(user_token, ref user))
            {
                ChatRoom room = new ChatRoom();
                string chat_token = request.FormField("chat_token");
                if (Database.chat.SelectChatByToken(ref chat_token, ref room))
                {
                    int page = 0;
                    Int32.TryParse(request.HeadParameter("page"), out page);
                    List<Message> messages = Database.message.SelectMessageByChatId(room.chat_id, page * 50, page * 50 + 50);
                    request.ResponseJsonData(messages);
                    if(!messages[messages.Count - 1].message_viewed && messages[messages.Count - 1].user_id != user.user_id)
                    {
                        Database.message.UpdateMessages(room.chat_id, true);
                    }
                    Logger.WriteLog("Get list of messages, chat_id->" + room.chat_id, LogLevel.Usual);
                    return;
                }
                else { message = "Server can't define chat by chat_token."; }
            }
            else { message = "No user with that user_token."; }
            request.ResponseJsonAnswer(false, message);
            Logger.WriteLog(message, LogLevel.Warning);
        }
    }
}