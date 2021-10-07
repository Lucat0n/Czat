using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Czat
{
    public partial class DialogBox : Form
    {
        private Regex regex = new Regex("^[a-zA-Z0-9_]*$");
        public DialogBox()
        {
            InitializeComponent();
            textBox1.KeyDown += textBox_KeyDown;
        }

        public DialogBox(String header)
        {
            InitializeComponent();
            label1.Text = header;
            textBox1.KeyDown += textBox_KeyDown;
        }

        private void textBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                button1.PerformClick();
            }
        }

        private void Button1_Click(object sender, EventArgs e)
        {
            if (!regex.IsMatch(textBox1.Text))
            {
                label1.ForeColor = Color.Red;
                label1.Text = "Nick nie może zawierać znaków specjalnych!";
            }
            else if (textBox1.Text.Length <= 4)
            {
                label1.ForeColor = Color.Red;
                label1.Text = "Nick musi mieć minimum 5 znaków!";
            }
            else
            {
                DialogResult = DialogResult.OK;
            }
        }
    }
}
