﻿namespace Philips.Analogy
{
    partial class XtraFormLogGrid
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(XtraFormLogGrid));
            this.ucLogs1 = new Philips.Analogy.UCLogs();
            this.SuspendLayout();
            // 
            // ucLogs1
            // 
            this.ucLogs1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.ucLogs1.Location = new System.Drawing.Point(0, 0);
            this.ucLogs1.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.ucLogs1.Name = "ucLogs1";
            this.ucLogs1.OnlineMode = false;
            this.ucLogs1.Size = new System.Drawing.Size(1029, 578);
            this.ucLogs1.TabIndex = 0;
            // 
            // XtraFormLogGrid
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1029, 578);
            this.Controls.Add(this.ucLogs1);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "XtraFormLogGrid";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Analogy";
            this.Load += new System.EventHandler(this.XtraFormLogGrid_Load);
            this.ResumeLayout(false);

        }

        #endregion

        private UCLogs ucLogs1;
    }
}