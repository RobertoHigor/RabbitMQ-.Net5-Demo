
# Sobre os projetos

## Topic Exchange

![Topic Exchange](img\TopicExchange.jpg)

Os projetos `PaymentCardConsumer`, `PurchaseOrderConsumer` e `AccountsAuditConsumer` são consumidores de uma fila por tópicos.
AccountsAuditConsumer recebe mensagem de todas as filas filhas de payment, por utilizar a wild tag `*`

A API `Payment API` possui rotas para adicionar nas filas payment.card e payment.purchaseorder. 

## Remote Procedure Call

![Remote Procedure Call](img\RemoteProcedure.jpg)

O controller `DirectCardPaymentController` envia uma mensagem síncrona via remote procedure call, enviando uma mensagem para `DirectPaymentCardConsumer` e aguardando sua resposta.

A imagem é enviada com um correlationId. Ao receber a resposta, é verificado o correlationId para confirmar que se trata da mesma mensagem. Isso ajuda ajuda a eliminar mensagens duplicadas e evitar reprocessamento.
