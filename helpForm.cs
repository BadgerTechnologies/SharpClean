using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Diagnostics;

/*
 * Sharp Clean: Program.cs
 * Author: Joey Harrison
 */

namespace sharpclean
{
    public partial class helpForm : Form
    {
        public helpForm()
        {
            InitializeComponent();

        }

        private void linkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            try
            {
                VisitLink();
            }
            catch (Exception ee)
            {
                MessageBox.Show("Unable to open link.");
                Console.WriteLine("Error: " + ee);
            }
        }

        private void VisitLink()
        {
            string link = "https://docs.google.com/document/d/15Aop8_XPgLWvEJ6C8ix3vcBJgelrvwAU0vLe095Q8IU/edit?usp=sharing";
            linkLabel1.LinkVisited = true;
            Process.Start(link);
        }
    }
}
