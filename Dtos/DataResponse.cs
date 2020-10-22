namespace mini_message.Dtos
{
    public class DataResponse
    {
        public bool success;
        public dynamic data;
        public DataResponse(bool success, dynamic data)
        {
            this.success = success;
            this.data = data;
        }
    }
}