using System.Text;
using Newtonsoft.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using Util;

namespace SendEmailProject
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            var queueName = "SendEmail";
            var exchangeName = "User_Register";

            var factory = new ConnectionFactory()
            {
                HostName = "localhost",
                UserName = "guest",
                Password = "guest",
            };
            var connection = await factory.CreateConnectionAsync();
            var chanel = await connection.CreateChannelAsync();

            await chanel.QueueDeclareAsync(queueName, true, false, false);
            await chanel.ExchangeDeclareAsync(exchangeName, ExchangeType.Fanout, true);

            await chanel.QueueBindAsync(queueName, exchangeName, "", null);

            var consumer = new AsyncEventingBasicConsumer(chanel);
            consumer.ReceivedAsync += async (sender, @event) =>
            {

                var result = Encoding.UTF8.GetString(@event.Body.ToArray());
                var user = JsonConvert.DeserializeObject<User>(result);

                Console.WriteLine($"Send Email =>{user!.Email}");
                await chanel.BasicAckAsync(@event.DeliveryTag, false);
            };

            await chanel.BasicConsumeAsync(queueName, false, consumer);
            Console.ReadKey();

        }
    }
}
