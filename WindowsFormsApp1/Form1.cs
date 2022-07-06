using System;
using System.Windows.Forms;
using System.Messaging;

namespace WindowsFormsApp1
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            MessageQueue messageQueue = null;
            if(MessageQueue.Exists(@".\Private$\test_cola") )
            {
                messageQueue = new MessageQueue(@".\Private$\test_cola");
                messageQueue.Label = "Probando Cola";
            }
            else
            {
                MessageQueue.Create(@".\Private$\test_cola");
                messageQueue = new MessageQueue(@".\Private$\test_cola");
                messageQueue.Label = "Nueva Cola Creada";
            }
            messageQueue.Send("Este es una prueba del XX-06-2022 a MSMQ", "Titulo");
        }

    }
}
