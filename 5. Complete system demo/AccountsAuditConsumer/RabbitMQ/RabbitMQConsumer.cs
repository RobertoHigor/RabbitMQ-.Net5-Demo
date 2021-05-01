using System;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace AccountsAuditConsumer.RabbitMQ
{
    public class RabbitMQConsumer
    {
        private static ConnectionFactory _factory;
        private static IConnection _connection;        

        private const string ExchangeName = "Topic_Exchange";
        private const string AllQueueName = "AllTopic_Queue";

        public void CreateConnection()
        {
            _factory = new ConnectionFactory { HostName = "localhost", UserName = "guest", Password = "guest" };            
        }

        public void Close()
        {
            _connection.Close();
        }

        public void ProcessMessages()
        {
            using (_connection = _factory.CreateConnection())
            {
                using (var channel = _connection.CreateModel())
                {
                    Console.WriteLine("Listening for Topic <payment.*>");
                    Console.WriteLine("------------------------------");
                    Console.WriteLine();
                    
                    channel.ExchangeDeclare(ExchangeName, "topic");
                    channel.QueueDeclare(AllQueueName, true, false, false, null);
                    channel.QueueBind(AllQueueName, ExchangeName, "payment.*");

                    channel.BasicQos(0, 10, false);          
                    var consumer = new EventingBasicConsumer(channel); 

                    consumer.Received += (model, ea) => // suporta async
                    {
                        var body = ea.Body.ToArray();
                        System.Console.WriteLine("DeSerializando a mensagem");
                        var message = body.DeSerializeText();
                        var routingKey = ea.RoutingKey;
                        Console.WriteLine("Message Received '{0}'", message);
                        channel.BasicAck(ea.DeliveryTag, false);
                    };

                    channel.BasicConsume(AllQueueName, autoAck: false, consumer);
                    Console.ReadLine();                              
                }
            }
        }
    }
}
