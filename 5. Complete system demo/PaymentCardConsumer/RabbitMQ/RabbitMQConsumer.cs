using System;
using RabbitMQ.Client;

namespace PaymentCardConsumer.RabbitMQ
{
    public class RabbitMQConsumer
    {
        private static ConnectionFactory _factory;
        private static IConnection _connection;
        
        private const string ExchangeName = "Topic_Exchange";
        private const string CardPaymentQueueName = "CardPaymentTopic_Queue";

        public void CreateConnection()
        {
            _factory = new ConnectionFactory {
                HostName = "localhost",
                UserName = "guest", Password = "guest" };            
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
                    Console.WriteLine("Listening for Topic <payment.cardpayment>");
                    Console.WriteLine("-----------------------------------------");
                    Console.WriteLine();

                    channel.ExchangeDeclare(ExchangeName, "topic");
                    channel.QueueDeclare(CardPaymentQueueName, 
                        true, false, false, null);

                    channel.QueueBind(CardPaymentQueueName, ExchangeName, 
                        "payment.cardpayment");

                    channel.BasicQos(0, 10, false);
                   
                  
                    // Alternativa a Subscribe que é depreciado.              
                    // Criando override de DefaultBasicConsumer
                   DefaultBasicConsumer subscription = new MessageConsumer(channel);             
                    channel.BasicConsume(CardPaymentQueueName, autoAck: false, subscription); 
                    Console.ReadLine();                                              
                }
            }
        }
    }
}
