using System;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace PurchaseOrderConsumer.RabbitMQ
{
    public class RabbitMQConsumer
    {
        private static ConnectionFactory _factory;
        private static IConnection _connection;

        private const string ExchangeName = "Topic_Exchange";
        private const string PurchaseOrderQueueName = "PurchaseOrderTopic_Queue";

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
                    Console.WriteLine("Listening for Topic <payment.purchaseorder>");
                    Console.WriteLine("------------------------------------------");
                    Console.WriteLine();

                    channel.ExchangeDeclare(ExchangeName, "topic");
                    channel.QueueDeclare(PurchaseOrderQueueName, true, false, false, null);
                    channel.QueueBind(PurchaseOrderQueueName, ExchangeName, "payment.purchaseorder");

                    channel.BasicQos(0, 10, false);

                    // Outra forma seria com DefaultBasicConsumer
                    //var subscription = new EventingBasicConsumer(channel);
                    var consumer = new EventingBasicConsumer(channel);
           
                    consumer.Received += (model, ea) => // suporta async
                    {
                        var body = ea.Body.ToArray();
                        System.Console.WriteLine("DeSerializando a mensagem");
                        var message = (PurchaseOrder)body.DeSerialize(typeof(PurchaseOrder));
                        var routingKey = ea.RoutingKey;
                        Console.WriteLine($"----- Payment - Routing Key<{routingKey}> :{message.AmountToPay} : {message.PoNumber} : {message.CompanyName} : {message.PaymentDayTerms}");
                        channel.BasicAck(ea.DeliveryTag, false);
                    };

                    channel.BasicConsume(PurchaseOrderQueueName, autoAck: false, consumer);
                    Console.ReadLine();
                }
            }
        }
    }
}
