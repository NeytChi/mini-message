﻿using System;
namespace Common.Logging
{
    public struct Log
    {
        public long log_id;
        public string log;
        public string user_computer;
        public short seconds;
        public short minutes;
        public short hours;
        public short day;
        public short month;
        public int year;
        public string level;
    }
}
