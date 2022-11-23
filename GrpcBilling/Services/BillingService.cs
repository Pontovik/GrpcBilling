using Grpc.Core;
using Billing;
using Billing.Models;
using System.Runtime.Serialization.Formatters.Binary;

namespace Billing.Services
{
    public class BillingService : Billing.BillingBase
    {
        private readonly ILogger<BillingService> _logger;
        public BillingService(ILogger<BillingService> logger)
        {
            _logger = logger;
        }

        public override async Task ListUsers(None request, IServerStreamWriter<UserProfile> responseStream, ServerCallContext context)
        {
            foreach (var user in Data.listUsers)
            {
                await responseStream.WriteAsync(new UserProfile
                {
                    Name = $"Name = {user.Name}, amount = {user.Amount}, rating = {user.Rating}"
                });

            }
        }

        public override Task<Response> MoveCoins(MoveCoinsTransaction trans, ServerCallContext context)
        {
            var src = Data.FindByName(trans.SrcUser);
            if (src.Amount < trans.Amount)
            {
                return Task.FromResult(
                    new Response
                    {
                        Status = Response.Types.Status.Ok,
                        Comment = "Done"
                    }
                   );
            }

            var dst = Data.FindByName(trans.DstUser);
            var srcList = Data.CoinsUser[src];
            var dstList = Data.CoinsUser[dst];

            for (int i = 0; i < trans.Amount; i++)
            {
                var first = srcList[0];
                int history = int.Parse(first.History) + 1;
                dstList.Add(new Coin { Id = first.Id, History = history.ToString() });
                srcList.Remove(first);
            }
            Data.SetUsersAmount();
            return Task.FromResult(
       new Response
       {
           Status = Response.Types.Status.Ok,
           Comment = "Done"
       }
                                  );

        }

        public override Task<Coin> LongestHistoryCoin(None none, ServerCallContext context)
        {
            var max = new Coin { History = "0", Id = 0 };
            foreach (var pair in Data.CoinsUser)
            {
                var tempMax = Data.BiggestHistoryInList(pair.Value);
                if (string.Compare(max.History, tempMax.History) == -1)
                {
                    max = tempMax;
                }
            }
            return Task.FromResult(max);
        }

        public override Task<Response> CoinsEmission(EmissionAmount amount, ServerCallContext context)
        {
            var response = new Response();
            if (amount.Amount < 0)
            {
                response.Status = Response.Types.Status.Unspecified;
                response.Comment = "amount is less than 0";
                return Task.FromResult(response);
            }
            int users = Data.listUsers.Count;
            int sumRating = Data.listUsers.AsParallel().Sum(n => n.Rating);
            if (amount.Amount - users < 0)
            {
                response.Status = Response.Types.Status.Failed;
                response.Comment = "fail";
                return Task.FromResult(response);
            }

            Data.listUsers.AsParallel().OrderBy(n => n.Rating);
            long usedCoins = 0;
            int id = Data.GetLastIdFromCoinsUser();
            double propRating = 0.0;
            long propAmount = 0;
            CancellationTokenSource cancelTokenSource = new CancellationTokenSource();
            cancelTokenSource.Cancel();
            for (int i = users - 1; i >= 0; i--)
            {
                if (i != 0)
                {
                    propRating = (double)Data.listUsers[i].Rating / (double)sumRating;
                    propAmount = (int)(propRating * amount.Amount);
                }
                else
                {
                    propAmount = amount.Amount - usedCoins;
                }
                for (int j = 0; j < propAmount; j++)
                {
                    if (Data.CoinsUser.TryGetValue(Data.listUsers[i], out List<Coin> coins))
                    {
                        coins.Add(new Coin { Id = ++id, History = "1" });
                    }
                    else
                    {
                        Data.CoinsUser.Add(Data.listUsers[i], new List<Coin> { new Coin { Id = ++id, History = "1" } });
                    }
                    usedCoins++;

                }
            }
            Data.SetLastIdOfCoinsUser(id);
            Data.SetUsersAmount();
            return Task.FromResult(
                new Response
                {
                    Status = Response.Types.Status.Ok,
                    Comment = "Done"
                }
                );
        }
    }
}