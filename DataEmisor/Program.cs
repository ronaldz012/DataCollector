using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;

// Configurar la conexión
var factory = new ConnectionFactory 
{ 
    HostName = "localhost", 
    UserName = "emisor_user", 
    Password = "password_emisor" 
};

// Usar 'using' para asegurar que los recursos se liberen correctamente
await using var connection = await factory.CreateConnectionAsync();
await using var channel = await connection.CreateChannelAsync();

// 1. Declarar el Exchange (El enrutador)
await channel.ExchangeDeclareAsync(
    exchange: "ventas.exchange", 
    type: ExchangeType.Direct,
    durable: false,  // Persistir el exchange
    autoDelete: false);

// 2. Declarar la Cola (El buzón)
await channel.QueueDeclareAsync(
    queue: "ventas.pos", 
    durable: true,  // La cola sobrevive a reinicios del servidor
    exclusive: false, 
    autoDelete: false);

// 3. Unir la Cola con el Exchange usando una "Routing Key"
await channel.QueueBindAsync(
    queue: "ventas.pos", 
    exchange: "ventas.exchange", 
    routingKey: "nueva.venta");

// 4. Publicar el mensaje
var mensaje = "Datos de la venta #123";
var body = Encoding.UTF8.GetBytes(mensaje);

await channel.BasicPublishAsync(
    exchange: "ventas.exchange", 
    routingKey: "nueva.venta", 
    body: body,
    mandatory: false);

Console.WriteLine($"Mensaje enviado: {mensaje}");