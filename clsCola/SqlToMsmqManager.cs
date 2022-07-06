using System;
using System.Data.SqlTypes;
using System.Messaging;
using Microsoft.SqlServer.Server;

namespace SqlMsmq
{
    public class SqlToMsmqManager
    {
        [SqlProcedure]
        public static void Send(SqlString nombreCola, SqlString mensaje)
        {
            //select* from sys.dm_clr_properties

            if (nombreCola == null || string.IsNullOrEmpty(nombreCola.Value))
                throw new Exception("El nombre de cola no ha sido indicado");

            var queue = nombreCola.Value;
            if (!MessageQueue.Exists(queue))
                MessageQueue.Create(queue);

            try
            {
                using (var messageQueue = new MessageQueue(queue, QueueAccessMode.Send))
                {
                    messageQueue.Formatter = new XmlMessageFormatter(new Type[] { typeof(string) });
                    messageQueue.Send(mensaje.Value, MessageQueueTransactionType.Single);
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
    }
}
