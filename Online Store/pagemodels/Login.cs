namespace Online_Store.pagemodels
{
    public class Login //same as Index but allows changing of message as needed
    {

        public string message { get; private set; }
        public bool rememberMe { get; set; }

        public Login(string message)
        {
            this.message = message;
        }

        public void ChangeMessage(string message)
        {
            if(message == null || String.Equals(message, ""))
            {
                return;
            }

            this.message = message;
        }
    }
}
