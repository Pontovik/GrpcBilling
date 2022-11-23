using Billing.Models;

namespace Billing.Services
{
    public class Data
    {
        public static readonly List<User> listUsers = new List<User>
        {
            new User ("boris", 5000),
            new User ("maria", 1000),
            new User ("oleg",800)
        };

        private static int lastId = 0;
        public static Dictionary<User, List<Coin>> CoinsUser = new Dictionary<User, List<Coin>>();
        public static int GetLastIdFromCoinsUser()
        {
            return lastId;
        }

        public static void SetLastIdOfCoinsUser(int id)
        {
            lastId = id;
        }

        public static void SetUsersAmount()
        {
            for (int i = 0; i < listUsers.Count; i++)
            {
                if (CoinsUser.TryGetValue(listUsers[i], out var list))
                {
                    listUsers[i].Amount = list.Count;
                }
            }
        }

        public static Coin BiggestHistoryInList(List<Coin> coins)
        {
            coins.OrderBy(n => n.History);
            return coins[^1];
        }
        public static User? FindByName(string name)
        {
            for (int i = 0; i < listUsers.Count; i++)
            {
                if (listUsers[i].Name == name)
                {
                    return listUsers[i];
                }
            }
            return null;
        }
    }
}
