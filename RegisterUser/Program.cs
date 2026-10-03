using System.Text;
using Newtonsoft.Json;
using RabbitMQ.Client;
using Util;

namespace RegisterUser
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            var exchangeName = "User_Register";
            var queueName = "FinallyOrder";
            var calculateOrderFeeExchangeName = "Users-Order";

            Console.Write("Please Enter Phone:");
            var phone = Console.ReadLine();

            Console.Write("Please Enter Email :");
            var email = Console.ReadLine();


            var factory = new ConnectionFactory()
            {
                HostName = "localhost",
                UserName = "guest",
                Password = "guest",
            };
            var connection = await factory.CreateConnectionAsync();
            var chanel = await connection.CreateChannelAsync();

            await chanel.QueueDeclareAsync(queueName, true, false, false);

            await chanel.ExchangeDeclareAsync(calculateOrderFeeExchangeName, ExchangeType.Direct, true, false);
            await chanel.ExchangeDeclareAsync(exchangeName, ExchangeType.Fanout, true, false);

            await chanel.QueueBindAsync(queueName, calculateOrderFeeExchangeName, queueName);

            var user = new User()
            {
                Email = email!,
                Phone = phone!,
            };

            var userConvert = JsonConvert.SerializeObject(user);
            var body = Encoding.UTF8.GetBytes(userConvert);
            var isSend = true;
            do
            {
                await chanel.BasicPublishAsync(calculateOrderFeeExchangeName, queueName, false, body);
                await chanel.BasicPublishAsync(exchangeName, "", body);

                Console.WriteLine("Send Again ? ");
                var res = Console.ReadLine();
                if (res != "y")
                {
                    isSend = false;
                }
            } while (isSend);

            Console.ReadKey();
        }
    }
}
