using System;
using System.Timers;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.ServiceProcess;
using Azure.Messaging.ServiceBus;

namespace ServicioUpdCobranza
{
    public partial class Service1 : ServiceBase
    {
        protected static Timer tmRecibir;
        public Service1()
        {
            InitializeComponent();
        }

        protected override void OnStart(string[] args)
        {
            if (!EventLog.SourceExists("srvServiceBus")) {
                EventLog.CreateEventSource("srvServiceBus", "Application");
            }

            tmRecibir = new Timer();
            tmRecibir.Interval = 4000; // 4 segundos 
            tmRecibir.Elapsed += OnTimedEvent;
            tmRecibir.Enabled = true;
            tmRecibir.Start();
        }

        protected override void OnStop()
        {

        }

        private static void OnTimedEvent(Object source, System.Timers.ElapsedEventArgs e)
        {
            Timer t = (Timer)source;
            t.Stop();
            //RecibirMensaje1();
            RecibirMensaje2();
            t.Start();
        }

        private static async void RecibirMensaje1()
        {
            SqlConnection cnEnsa = new SqlConnection();
            SqlCommand slqCommando = new SqlCommand();
            DataTable dtCola = new DataTable();
            SqlDataAdapter SQLAdapter;
            EventLog eventoDesacopla = new EventLog();
            string CadSelec;
            eventoDesacopla.Source = "srvServiceBus";

            try
            {
                cnEnsa.ConnectionString = @"Data Source=ENSE26LN060\MSSQLSERVER01;Initial Catalog=Colita;Application Name=Colita;Integrated Security=SSPI";
                string connectionString = "Endpoint=sb://sboptimuspruebas.servicebus.windows.net/;SharedAccessKeyName=edwin;SharedAccessKey=yQTRkbTTA7RD2Bb+RqSrQs1lEOS8gv1dzDWwJorze/U=;EntityPath=myqueue";
                string queueName = "myqueue";


                var client = new ServiceBusClient(connectionString);
                ServiceBusSender sender = client.CreateSender(queueName);

                using (cnEnsa)
                {
                    cnEnsa.Open();
                    slqCommando.Connection = cnEnsa;
                    slqCommando.CommandTimeout = 1800;

                    do
                    {
                        try
                        {
                            CadSelec = "DECLARE @conversation uniqueidentifier, @senderMsgType nvarchar(100), @msg varchar(max);";
                            CadSelec += "WAITFOR (RECEIVE top (1) ";
                            CadSelec += "@conversation = conversation_handle, @msg = message_body, @senderMsgType = message_type_name ";
                            CadSelec += "FROM RecipCola); ";
                            CadSelec += "SELECT @msg AS RecievedMessage, @senderMsgType AS SenderMessageType; ";
                            CadSelec += "END CONVERSATION @conversation;";

                            slqCommando.CommandText = CadSelec;
                            slqCommando.CommandType = CommandType.Text;
                            SQLAdapter = new SqlDataAdapter(slqCommando);
                            SQLAdapter.Fill(dtCola);
                        }
                        catch (Exception ex)
                        {
                            eventoDesacopla.WriteEntry("(1) " + ex.Message, EventLogEntryType.Error, 15);
                        }

                        if (dtCola.Rows.Count > 0)
                        {
                            if (dtCola.Rows[0]["SenderMessageType"].ToString() == "ServiceBus")
                            {
                                ServiceBusMessage message = new ServiceBusMessage(dtCola.Rows[0]["RecievedMessage"].ToString());
                                await sender.SendMessageAsync(message);
                                eventoDesacopla.WriteEntry(dtCola.Rows[0]["RecievedMessage"].ToString());
                            }
                        }
                        dtCola.Clear();

                    } while (true);
                }
            }
            catch (ServiceBusException ex)
            {
                eventoDesacopla.WriteEntry("ServiceBus: " + ex.Message, EventLogEntryType.Error, 15);
            }
            catch (Exception ex)
            {
                eventoDesacopla.WriteEntry("(2) " + ex.Message, EventLogEntryType.Error, 15);
            }
        }

        public static async void RecibirMensaje2()
        {
            SqlConnection cnEnsa = new SqlConnection();
            SqlCommand slqCommando = new SqlCommand();
            DataTable dtCola = new DataTable();
            EventLog eventoDesacopla = new EventLog();
            eventoDesacopla.Source = "srvServiceBus";

            try
            {
                cnEnsa.ConnectionString = @"Data Source=ENSE26LN060\MSSQLSERVER01;Initial Catalog=Colita;Application Name=Colita;Integrated Security=SSPI";
                string connectionString = "Endpoint=sb://sboptimusngc.servicebus.windows.net/;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=P47t2l0Kl3NfpfVdUuefaN5VAZZxmX4fqmYHMMoflIA=";
                string queueName = "myqueue";

                var client = new ServiceBusClient(connectionString);
                ServiceBusSender sender = client.CreateSender(queueName);

                using (cnEnsa)
                {
                    cnEnsa.Open();
                    slqCommando.Connection = cnEnsa;
                    slqCommando.CommandTimeout = 3600;

                    slqCommando.CommandType = CommandType.StoredProcedure;
                    slqCommando.CommandText = "RecibirDocumentoCloud_pa";
                    slqCommando.Parameters.Add("@RecievedMessage", SqlDbType.NVarChar, -1);
                    slqCommando.Parameters["@RecievedMessage"].Direction = ParameterDirection.Output;
                    slqCommando.Parameters.Add("@MessageType", SqlDbType.NVarChar, -1);
                    slqCommando.Parameters["@MessageType"].Direction = ParameterDirection.Output;

                    while (true)
                    {
                        try
                        {
                            slqCommando.ExecuteNonQuery();
                            var mensaje = slqCommando.Parameters["@RecievedMessage"].Value as string;
                            var tipo = slqCommando.Parameters["@MessageType"].Value as string;

                            if (mensaje != null)
                            {
                                if (tipo == "ServiceBus")
                                {
                                    ServiceBusMessage message = new ServiceBusMessage(mensaje);
                                    await sender.SendMessageAsync(message);
                                    eventoDesacopla.WriteEntry(mensaje);
                                }
                            }
                        }
                        catch (ServiceBusException ex)
                        {
                            eventoDesacopla.WriteEntry("ServiceBus: " + ex.Message, EventLogEntryType.Error, 15);
                        }
                        catch (Exception ex)
                        {
                            eventoDesacopla.WriteEntry("(1) " + ex.Message, EventLogEntryType.Error, 15);
                        }
                    } 
                }
            }
            catch (Exception ex)
            {
                eventoDesacopla.WriteEntry("(2) " + ex.Message, EventLogEntryType.Error, 15);
            }
        }
    }
}
