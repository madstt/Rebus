using System;
using System.Collections.Generic;
using Rebus.Tests.Persistence;
using Rebus.Transports.Sql;

namespace Rebus.Tests.Contracts.Transports.Factories
{
    public class SqlServerTransportFactory : ITransportFactory
    {
        const string MessageTableName = "messages2";
        readonly List<IDisposable> disposables = new List<IDisposable>();

        public SqlServerTransportFactory()
        {
            DropMessageTableIfItExists();
        }

        public Tuple<ISendMessages, IReceiveMessages> Create()
        {
            var sender = GetQueue("test.contracts.sender");
            var receiver = GetQueue("test.contracts.receiver");

            return Tuple.Create<ISendMessages, IReceiveMessages>(sender, receiver);
        }

        public void CleanUp()
        {
            disposables.ForEach(d => d.Dispose());

            DropMessageTableIfItExists();
        }

        public IReceiveMessages CreateReceiver(string queueName)
        {
            var receiver = GetQueue(queueName);

            return receiver;
        }

        static void DropMessageTableIfItExists()
        {
            if (!SqlServerFixtureBase.GetTableNames().Contains(MessageTableName)) return;

            SqlServerFixtureBase.ExecuteCommand(string.Format("drop table [{0}]", MessageTableName));
        }

        IDuplexTransport GetQueue(string inputQueueName)
        {
            var queue = new SqlServerMessageQueue(SqlServerFixtureBase.ConnectionString, MessageTableName, inputQueueName)
                .EnsureTableIsCreated()
                .PurgeInputQueue();

            disposables.Add(queue);

            return queue;
        }
    }
}