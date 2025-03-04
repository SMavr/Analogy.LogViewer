﻿
namespace DevExpress.Sync
{
    partial class MainForm
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
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
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.txtbSource = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.txtbTarget = new System.Windows.Forms.TextBox();
            this.btnCopy = new System.Windows.Forms.Button();
            this.btnVersionUpgrade = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // txtbSource
            // 
            this.txtbSource.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtbSource.Location = new System.Drawing.Point(91, 18);
            this.txtbSource.Name = "txtbSource";
            this.txtbSource.Size = new System.Drawing.Size(697, 27);
            this.txtbSource.TabIndex = 0;
            this.txtbSource.Text = "c:\\Program Files (x86)\\DevExpress 21.2\\Components\\Bin\\";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(10, 22);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(57, 20);
            this.label1.TabIndex = 1;
            this.label1.Text = "Source:";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(10, 55);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(53, 20);
            this.label2.TabIndex = 3;
            this.label2.Text = "Target:";
            // 
            // txtbTarget
            // 
            this.txtbTarget.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtbTarget.Location = new System.Drawing.Point(91, 51);
            this.txtbTarget.Name = "txtbTarget";
            this.txtbTarget.Size = new System.Drawing.Size(697, 27);
            this.txtbTarget.TabIndex = 2;
            this.txtbTarget.Text = "e:\\files\\Programming\\Analogy.LogViewer\\DevExpress\\";
            // 
            // btnCopy
            // 
            this.btnCopy.Location = new System.Drawing.Point(91, 88);
            this.btnCopy.Name = "btnCopy";
            this.btnCopy.Size = new System.Drawing.Size(130, 51);
            this.btnCopy.TabIndex = 4;
            this.btnCopy.Text = "Copy";
            this.btnCopy.UseVisualStyleBackColor = true;
            this.btnCopy.Click += new System.EventHandler(this.btnCopy_Click);
            // 
            // btnVersionUpgrade
            // 
            this.btnVersionUpgrade.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnVersionUpgrade.Location = new System.Drawing.Point(658, 88);
            this.btnVersionUpgrade.Name = "btnVersionUpgrade";
            this.btnVersionUpgrade.Size = new System.Drawing.Size(130, 51);
            this.btnVersionUpgrade.TabIndex = 5;
            this.btnVersionUpgrade.Text = "Version Upgrade";
            this.btnVersionUpgrade.UseVisualStyleBackColor = true;
            this.btnVersionUpgrade.Click += new System.EventHandler(this.btnVersionUpgrade_Click);
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 20F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(800, 159);
            this.Controls.Add(this.btnVersionUpgrade);
            this.Controls.Add(this.btnCopy);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.txtbTarget);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.txtbSource);
            this.Name = "MainForm";
            this.Text = "DevExpress Syncer";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox txtbSource;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox txtbTarget;
        private System.Windows.Forms.Button btnCopy;
        private System.Windows.Forms.Button btnVersionUpgrade;
    }
}

