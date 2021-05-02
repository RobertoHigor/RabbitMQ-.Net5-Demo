using System;
using System.Text;
using Payments.Models;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
// https://www.rabbitmq.com/tutorials/tutorial-six-dotnet.html
namespace Payments.RabbitMQ
{
    public class RabbitMQDirectClient
    {
        private IConnection _connection;
        private IModel _channel;
        private string _replyQueueName;
        private EventingBasicConsumer _consumer;

        public void CreateConnection()
        {
            var factory = new ConnectionFactory { HostName = "localhost", UserName = "guest", Password = "guest" };
            _connection = factory.CreateConnection();
            _channel = _connection.CreateModel();
            _replyQueueName = _channel.QueueDeclare("rpc_reply", true, false, false, null);

            // Criando consumer para ouvir fila de resposta
            _consumer = new EventingBasicConsumer(_channel);            
        }

        public void Close()
        {
            _connection.Close();
        }

        public string MakePayment(CardPayment payment)
        {
            var corrId = Guid.NewGuid().ToString();
            var props = _channel.CreateBasicProperties(); // Pegando as propriedades do objeto de volta
            props.ReplyTo = _replyQueueName; // Fila de resposta
            // Colocando uma guid no CorrelationId
            // A aplicação consumer irá colocar o mesmo id na resposta
            // Isso garante que estamos lendo a mensagem enviada
            props.CorrelationId = corrId;

            // Passando as propriedades para a fila (ReplyTo e CorrelationId)
            Console.WriteLine($" Direct Payment Sent {payment.CardNumber}, £{payment.Amount}");
            _channel.BasicPublish("", "rpc_queue", props, payment.Serialize());

            string authCode = "";
            //bool responseReceived = true;// Teste para esperar resposta, não recomendado
            // Alternativa utilizando pull api (não recomendada)    
            //https://www.rabbitmq.com/dotnet-api-guide.html
            
            BasicGetResult result = null;
            while (result == null) // Recomendado TimeOut
            {
                result = _channel.BasicGet(_replyQueueName, autoAck: false);
                 if (result != null)
                {          
                    // Somente processar a resposta da mensagem enviada
                    //http://www.matthiassommer.it/programming/remote-procedure-calls-with-rabbitmq/
                    if (result.BasicProperties.CorrelationId != corrId) continue;   
                    var body = result.Body.ToArray();
                    authCode = Encoding.UTF8.GetString(body);
                    _channel.BasicAck(result.DeliveryTag, false);
                }
            }    

                 

            // Anonymous function event handling
            // Ver como faria para retornar a mensagem ou acionar algum método com ela 
            // _consumer.Received += (model, ea) => // suporta async
            // {
            //     // Checando se o ID é o mesmo da menasgem enviada
            //     if (ea.BasicProperties.CorrelationId == corrId)
            //     {
            //         var body = ea.Body.ToArray();
            //         authCode = Encoding.UTF8.GetString(body);
            //         responseReceived = false;
            //         Console.WriteLine($"Message received with authcode: {authCode}");
            //     }
            // };
            // Registra o consumer para ouvir a fila
            //_channel.BasicConsume(_replyQueueName, true, _consumer);      

            //TODO: Pesquisar forma correta de retornar resposta síncrona
            // while (responseReceived)
            // {
            //     // prender no loop
            // }            

            return authCode;
        }
    }
}

// Código da documentação
/*
consumer.Received += (model, ea) =>
            {
                string response = null;

                var body = ea.Body.ToArray();
                var props = ea.BasicProperties;
                var replyProps = channel.CreateBasicProperties();
                replyProps.CorrelationId = props.CorrelationId;

                try
                {
                    var message = Encoding.UTF8.GetString(body);
                    int n = int.Parse(message);
                    Console.WriteLine(" [.] fib({0})", message);
                    response = fib(n).ToString();
                }
                catch (Exception e)
                {
                    Console.WriteLine(" [.] " + e.Message);
                    response = "";
                }
                finally
                {
                    var responseBytes = Encoding.UTF8.GetBytes(response);
                    channel.BasicPublish(exchange: "", routingKey: props.ReplyTo,
                      basicProperties: replyProps, body: responseBytes);
                    channel.BasicAck(deliveryTag: ea.DeliveryTag,
                      multiple: false);
                }
            };
            */