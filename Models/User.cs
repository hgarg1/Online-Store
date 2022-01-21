using Binders;
namespace Models
{
    public class User //used when we pull data from a table so we can access data like a normal object
    {
        public string firstName { get; set; }
        public string lastName { get; set; }
        public string email { get; set; }
        public string lastLogin { get; set; }
        public string password { get; set; }
        public string address { get; set; }
        public int age { get; set; }
        public int id { get; set; }
        public string emailVerified { get; set; }

        public override bool Equals(object? obj) //able to handle table to table checks and table to binder checks too
        {
            if(obj is Binders.UserLogin)
            {
                return String.Equals(email, ((Binders.UserLogin)obj).username) && String.Equals(password, ((Binders.UserLogin)obj).password);
            }else if(obj is Models.User)
            {
                return String.Equals(email, ((Models.User)obj).email) 
                    && String.Equals(password, ((Models.User)obj).password)
                    && String.Equals(firstName, ((Models.User)obj).firstName)
                    && String.Equals(lastName, ((Models.User)obj).lastName)
                    && String.Equals(address, ((Models.User)obj).address)
                    && String.Equals($"{age}", $"{((Models.User)obj).age}")
                    && String.Equals($"{id}", $"{((Models.User)obj).id}");
            }
            else
            {
                return false; //will never reach here
            }
        }
    }
}