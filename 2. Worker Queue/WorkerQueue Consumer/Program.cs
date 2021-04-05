using System;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace RabbitMQ.Examples
{
    public class Program
    {
        private static ConnectionFactory _factory;
        private static IConnection _connection;
        
        private const string QueueName = "WorkerQueue_Queue";

        static void Main()
        {
            Receive();
            Thread.
            Console.ReadLine();
        }

        public static void Receive()
        {
            try
            {
                // Criando conexão
                _factory = new ConnectionFactory { HostName = "localhost", UserName = "guest", Password = "guest" };
                _connection = _factory.CreateConnection();                
                var channel = _connection.CreateModel();  

                channel.QueueDeclare(queue: QueueName, 
                                    durable:true,
                                    exclusive: false,
                                    autoDelete: false,
                                    arguments: null);

                // prefetchCount 1 diz para o rabbitMq não dar mais deu ma mensagem
                // até que o consumer termine o processamento e devolva um ack          
                // Nesse caso, o RabbitMQ irá entregar a próxima mensagem para um outro worker
                // que não esteja ocupado        
                channel.BasicQos(prefetchSize: 0, prefetchCount: 1, global: false);

                var consumer = new EventingBasicConsumer(channel);                              
                consumer.Received += (model, ea) =>
                {              
                    var body =  ea.Body.ToArray();                 
                    var message = (Payment)body.DeSerialize(typeof(Payment));                           
                    Console.WriteLine($"----- Payment Processed {message.CardNumber} : {message.AmountToPay}");
                    Thread.Sleep(1000); // Pausa de 1 segundo para testar load balancing       
                    Thread.             
                    channel.BasicAck(deliveryTag: ea.DeliveryTag, multiple: false); // Enviando ack  
                };                                                                           
            
                // Registrando consumer para ouvir a fila
                // AutoAck como false pois queremos devolver um ack manualmente, após o processamento com sucesso.                  
                channel.BasicConsume(QueueName, autoAck: false, consumer);                              
            }
            catch (System.Exception)
            {                
                throw;
            }
            
        }
    }
}
