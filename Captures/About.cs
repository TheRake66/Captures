using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Captures
{
    public partial class About : Form
    {
        //--------------------------------
        private bool ToUp = false;
        //--------------------------------



        //=================================================================
        public About()
        {
            //--------------------------------
            InitializeComponent();
            this.label2.Text = this.label2.Text
                .Replace("{Version}", typeof(Menu).Assembly.GetName().Version.ToString())
                .Replace("{Copyright}", "© BUSTOS Thibault (TheRake66) - 2020")
                .Replace("{Language}", "C#");
            //--------------------------------
        }
        //=================================================================



        //=================================================================
        private void timer1_Tick(object sender, EventArgs e)
        {
            //--------------------------------
            if (ToUp)
            {
                if (this.label2.Location.Y > 395)       this.label2.Location = new Point(this.label2.Location.X, this.label2.Location.Y - 2);
                else if (this.label2.Location.Y > 385)  this.label2.Location = new Point(this.label2.Location.X, this.label2.Location.Y - 1);
                else this.ToUp = false;
            }
            else
            {
                if (this.label2.Location.Y < 405)       this.label2.Location = new Point(this.label2.Location.X, this.label2.Location.Y + 2);
                else if (this.label2.Location.Y < 415)  this.label2.Location = new Point(this.label2.Location.X, this.label2.Location.Y + 1);
                else this.ToUp = true;
            }
            //--------------------------------
        }
        //=================================================================



        //=================================================================
        private void linkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            //--------------------------------
            try { Process.Start("explorer", "https://github.com/TheRake66"); }
            catch { }
            //--------------------------------
        }
        //=================================================================



        //=================================================================
        private void button1_Click(object sender, EventArgs e)
        {
            //--------------------------------
            this.Close();
            //--------------------------------
        }
        //=================================================================
    }
}
