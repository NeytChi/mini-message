using System.Collections.Generic;
using System.Threading;
using Common.Logging;
using Common.Chats;
using Common.NDatabase;
using MySql.Data.MySqlClient;
using MiniMessanger.Models.Chat;

namespace MiniMessanger.NDatabase.ChatStorage
{
    public class ParticipantStorage : Storage
    {
        public ParticipantStorage(MySqlConnection connection, Semaphore s_locker)
        {
            this.connection = connection;
            this.s_locker = s_locker;
            SetTableName("participants");
            SetTable
            (
                "CREATE TABLE IF NOT EXISTS participants" +
                "(" +
                    "participant_id bigint NOT NULL AUTO_INCREMENT, " +
                    "chat_id int, " +
                    "user_id int, " +
                    "opposide_id int, " +
                    "PRIMARY KEY (participant_id)"  +
                ");"
            );
        }
        public void AddParticipant(ref Participant participant)
        {
            using (MySqlCommand commandSQL = new MySqlCommand("INSERT INTO participants(chat_id, user_id, opposide_id)" +
                "VALUES (@chat_id, @user_id, @opposide_id);", connection))
            {
                commandSQL.Parameters.AddWithValue("@chat_id", participant.chat_id);
                commandSQL.Parameters.AddWithValue("@user_id", participant.user_id);
                commandSQL.Parameters.AddWithValue("@opposide_id", participant.opposide_id);
                s_locker.WaitOne();
                commandSQL.ExecuteNonQuery();
                participant.participant_id = (int)commandSQL.LastInsertedId;
                commandSQL.Dispose();
                s_locker.Release();
            }
            Logger.WriteLog("Add participant.participant_id->" + participant.participant_id + " to database.", LogLevel.Usual);
        }
        public List<Participant> SelectParticipantByChatId(int chat_id)
        {
            List<Participant> participants = new List<Participant>();
            using (MySqlCommand commandSQL = new MySqlCommand("SELECT * FROM participants WHERE chat_id=@chat_id;", connection))
            {
                commandSQL.Parameters.AddWithValue("@chat_id", chat_id);
                s_locker.WaitOne();
                using (MySqlDataReader readerMassive = commandSQL.ExecuteReader())
                {
                    while (readerMassive.Read())
                    {
                        Participant participant = new Participant();
                        participant.participant_id = readerMassive.GetInt32(0);
                        participant.chat_id = readerMassive.GetInt32(1);
                        participant.user_id = readerMassive.GetInt32(2);
                        participant.opposide_id = readerMassive.GetInt32(3);
                        participants.Add(participant);
                    }
                }
                s_locker.Release();
            }
            Logger.WriteLog("Select participants by chat_id->" + chat_id + ".", LogLevel.Usual);
            return participants;
        }
        public List<Participant> SelectParticipantByUserId(int user_id)
        {
            List<Participant> participants = new List<Participant>();
            using (MySqlCommand commandSQL = new MySqlCommand("SELECT * FROM participants WHERE user_id=@user_id;", connection))
            {
                commandSQL.Parameters.AddWithValue("@user_id", user_id);
                s_locker.WaitOne();
                using (MySqlDataReader readerMassive = commandSQL.ExecuteReader())
                {
                    while (readerMassive.Read())
                    {
                        Participant participant = new Participant();
                        participant.participant_id = readerMassive.GetInt32(0);
                        participant.chat_id = readerMassive.GetInt32(1);
                        participant.user_id = readerMassive.GetInt32(2);
                        participant.opposide_id = readerMassive.GetInt32(3);
                        participants.Add(participant);
                    }
                }
                s_locker.Release();
            }
            Logger.WriteLog("Select participants by user_id->" + user_id + ".", LogLevel.Usual);
            return participants;
        }
        public bool SelectByUserOpposideId(int user_id, int opposide_id, ref Participant participant)
        {
            bool success = false;
            using (MySqlCommand commandSQL = new MySqlCommand("SELECT * FROM participants WHERE user_id=@user_id AND opposide_id=@opposide_id;", connection))
            {
                commandSQL.Parameters.AddWithValue("@user_id", user_id);
                commandSQL.Parameters.AddWithValue("@opposide_id", opposide_id);
                s_locker.WaitOne();
                using (MySqlDataReader readerMassive = commandSQL.ExecuteReader())
                {
                    if (readerMassive.Read())
                    {
                        participant.participant_id = readerMassive.GetInt32(0);
                        participant.chat_id = readerMassive.GetInt32(1);
                        participant.user_id = readerMassive.GetInt32(2);
                        participant.opposide_id = readerMassive.GetInt32(3);
                        success = true;
                    }
                }
                s_locker.Release();
            }
            Logger.WriteLog("Select participant by user_id->" + user_id + " and opposide_id->" + opposide_id +". Success->" + success, LogLevel.Usual);
            return success;
        }
    }
}
