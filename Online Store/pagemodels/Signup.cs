namespace Online_Store.pagemodels
{
    public class Signup
    {
        public Boolean error { get; set; }
        public string Message { get; set; }

        public Signup()
        {
            error = false;
        }
    }
}
