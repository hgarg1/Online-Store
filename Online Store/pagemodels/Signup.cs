namespace Online_Store.pagemodels
{
    public class Signup //same as index, but defaults the error
    {
        public Boolean error { get; set; }
        public string Message { get; set; }

        public Signup()
        {
            error = false;
            Message = "";
        }
    }
}
