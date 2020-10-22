namespace Common
{
    public class JsonData
    {
        public bool success;
        public dynamic data;
        public JsonData(bool success, dynamic data)
        {
            this.success = success;
            this.data = data;
        }
    }
}