using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Windows.Input;

namespace Locomotion
{
    public partial class NetworkForm : Form
    {
        public string username {get {return this.nameInput.Text;}}

        public NetworkForm()
        {
            InitializeComponent();
        }

        public NetworkForm(string name)
        {
            InitializeComponent();
            nameInput.Text = name;
        }

        private void okButton_Click(object sender, EventArgs e)
        {
            DialogResult result = System.Windows.Forms.DialogResult.OK;

            if (this.nameInput.Text == "")
            {
                result = MessageBox.Show("If the name field is left blank, the name will default to 'Player' with a random number. Is this ok?",
                    "Empty Name Field", MessageBoxButtons.YesNo);
            }

            if (result == System.Windows.Forms.DialogResult.OK || result == System.Windows.Forms.DialogResult.Yes)
            {
                if (this.nameInput.Text == "")
                    this.nameInput.Text = "Player";

                this.Close();
            }
        }

        private void NetworkForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (this.nameInput.Text == "")
                this.nameInput.Text = "Player";
        }

        private void nameInput_KeyDown(object sender, System.Windows.Forms.KeyEventArgs e)
        {
            if (e.KeyData == Keys.Enter)
            {
                okButton_Click(sender, e);
            }
        }
    }
}
