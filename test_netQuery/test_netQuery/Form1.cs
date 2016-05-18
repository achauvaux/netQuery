using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

using System.Threading;
using System.Collections;
using System.Text.RegularExpressions;

namespace test_netQuery
{
    public partial class Form1 : Form
    {

        netQuery.netQuery nq;

        public Form1()
        {
            InitializeComponent();
            // textBox1.Text = "http://www.google.fr";
            textBox1.Text = @"C:\Users\achau\OneDrive\Documents\test_netQuery.html";
            textBox2.Text = "div div";
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }

        private void button1_Click(object sender, EventArgs e)
        {
            w.Url = new Uri(textBox1.Text);
        }

        private void button3_Click(object sender, EventArgs e)
        {
            if (w.Document != null)
            {
                if (nq == null)
                    nq = new netQuery.netQuery(w.Document);
                else
                    nq.refresh();

                richTextBox2.Text = w.DocumentText;

                ArrayList elements = nq.getElements(textBox2.Text);

                richTextBox1.Clear();
                foreach (HtmlElement element in elements)
                {
                    richTextBox1.AppendText(element.TagName + " #" + element.Id + "\n");
                    if (element.OuterHtml != null)
                        richTextBox1.AppendText(element.OuterHtml + "\n");
                    richTextBox1.AppendText("___________________________________________________________" + "\n");
                }
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            richTextBox2.Text = w.DocumentText;
        }

        private void w_DocumentCompleted(object sender, WebBrowserDocumentCompletedEventArgs e)
        {

        }

        private void button4_Click(object sender, EventArgs e)
        {
        }
    }
}
