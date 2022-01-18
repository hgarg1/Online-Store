namespace Online_Store.pagemodels
{
    public class Login
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
