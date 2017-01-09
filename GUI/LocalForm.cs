using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace Locomotion
{
    public partial class LocalForm : Form
    {
        public string player1Name { get { return this.player1NameInput.Text; } }
        public string player2Name { get { return this.player2NameInput.Text; } }

        public LocalForm()
        {
            InitializeComponent();
        }

        private void okButton_Click(object sender, EventArgs e)
        {
            DialogResult result = System.Windows.Forms.DialogResult.OK;

            if (this.player1NameInput.Text == "" && this.player2NameInput.Text == "")
            {
                result = MessageBox.Show("If both name fields are left blank, the names will default to 'Player 1' and 'Player 2'. Is this ok?",
                    "Empty Name Fields", MessageBoxButtons.YesNo);
            }
            else if (this.player1NameInput.Text == "")
            {
                result = MessageBox.Show("If Player 1's name is left blank, the name will default to 'Player 1'. Is this ok?",
                    "Empty Name Field", MessageBoxButtons.YesNo);
            }
            else if (this.player2NameInput.Text == "")
            {
                result = MessageBox.Show("If Player 2's name is left blank, the name will default to 'Player 2'. Is this ok?",
                    "Empty Name Field", MessageBoxButtons.YesNo);
            }

            if (result == System.Windows.Forms.DialogResult.OK || result == System.Windows.Forms.DialogResult.Yes)
            {
                if (this.player1NameInput.Text == "")
                    this.player1NameInput.Text = "Player 1";
                if (this.player2NameInput.Text == "")
                    this.player2NameInput.Text = "Player 2";

                this.Close();
            }
        }

        private void LocalForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (this.player1NameInput.Text == "")
                this.player1NameInput.Text = "Player 1";
            if (this.player2NameInput.Text == "")
                this.player2NameInput.Text = "Player 2";
        }

        private void LocalForm_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                okButton_Click(sender, (EventArgs)e);
            }
        }
    }
}
