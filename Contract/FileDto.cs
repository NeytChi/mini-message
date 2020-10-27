namespace mini_message.Dtos
{
    public struct FileDto
    {
        public int file_id { get; set; }
        public string file_path { get; set; }
        public string file_name { get; set; }
        public string file_type { get; set; }
        public string file_extension { get; set; }
        public string file_last_name { get; set; }
        public string file_fullpath { get; set; }
        public byte[] data { get; set; }
    }
}
