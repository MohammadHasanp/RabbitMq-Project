using System.Text;
using Newtonsoft.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using Util;

namespace CalculateOrderFee
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            var queueName = "FinallyOrder";

            var factory = new ConnectionFactory()
            {
                HostName = "localhost",
                UserName = "guest",
                Password = "guest",
            };
            var connection = await factory.CreateConnectionAsync();
            var chanel = await connection.CreateChannelAsync();

            await chanel.QueueDeclareAsync(queueName, true, false, false);

            var consumer = new AsyncEventingBasicConsumer(chanel);
            consumer.ReceivedAsync += async (sender, @event) =>
            {
                var result = Encoding.UTF8.GetString(@event.Body.ToArray());
                var user = JsonConvert.DeserializeObject<User>(result);

                Console.WriteLine($"Calculate Order =>{user!.Phone} + {user!.Email}");
                await chanel.BasicAckAsync(@event.DeliveryTag, false);
            };

            await chanel.BasicConsumeAsync(queueName, false, consumer);
            Console.ReadKey();
        }
    }
}
