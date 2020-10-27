namespace mini_message.Dtos
{
    public class DataResponse
    {
        public bool success { get; set; }
        public dynamic data { get; set; }
        public DataResponse(bool success, dynamic data)
        {
            this.success = success;
            this.data = data;
        }
    }
}