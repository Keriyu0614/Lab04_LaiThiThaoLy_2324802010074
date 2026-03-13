using RabbitMQ.Client;

namespace Messaging.Common.Connection
{
    /// <summary>
    /// [CLASS] Quản lý RabbitMQ IConnection
    /// Tái sử dụng connection thay vì tạo mới mỗi lần (connection rất tốn kém)
    /// </summary>
    public class ConnectionManager
    {
        private readonly ConnectionFactory _factory;
        private IConnection? _connection;

        public ConnectionManager(string hostName, string userName, string password, string vhost)
        {
            _factory = new ConnectionFactory
            {
                HostName = hostName,
                UserName = userName,
                Password = password,
                VirtualHost = vhost,
                DispatchConsumersAsync = true   // BẮT BUỘC cho AsyncEventingBasicConsumer
            };
        }

        /// <summary>Trả về connection hiện tại, tạo mới nếu chưa có hoặc đã đóng</summary>
        public IConnection GetConnection()
        {
            if (_connection == null || !_connection.IsOpen)
                _connection = _factory.CreateConnection();
            return _connection;
        }
    }
}
