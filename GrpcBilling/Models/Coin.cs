namespace Billing.Models
{
    public class Coin
    {
        public string History { get; set; }
        public long Id { get; set; }

        public override int GetHashCode()
        {
            return Id.GetHashCode();
        }
    }
}
