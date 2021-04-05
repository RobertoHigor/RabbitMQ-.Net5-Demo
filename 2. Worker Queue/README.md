# Worker Queue

As mensagens da fila são compartilhadas entre um ou mais consumers que realizam um load balancing.

Cada consumer recebe somente uma mensagem. Uma nova mensagem será recebida após o mesmo devolver um ack, depois de processar a imagem. Enquanto isso, um segundo consumer pode pegar a próxima mensagem da fila.

![alt](img\workerqueue.png)
