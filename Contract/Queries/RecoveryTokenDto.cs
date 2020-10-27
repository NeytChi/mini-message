namespace mini_message.Contract.Queries
{
    public class RecoveryTokenDto
    {
        public string recovery_token { get; set; }

        public RecoveryTokenDto(string recoveryToken)
        {
            recovery_token = recoveryToken;
        }
    }
}