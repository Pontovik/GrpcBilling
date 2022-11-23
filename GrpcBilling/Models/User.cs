namespace Billing.Models
{
    public class User
    {
        public string Name { get; set; }
        public int Rating { get; set; }

        public long Amount { get; set; }

        public User(string name, int rating, long amount = 0)
        {
            Name = name;
            Rating = rating;
            Amount = amount;
        }

        
    }
}
