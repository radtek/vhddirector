﻿namespace VhdMount
{
    partial class Form1
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
            this.VHDTextBox = new System.Windows.Forms.TextBox();
            this.button1 = new System.Windows.Forms.Button();
            this.button2 = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // VHDTextBox
            // 
            this.VHDTextBox.Location = new System.Drawing.Point(13, 13);
            this.VHDTextBox.Name = "VHDTextBox";
            this.VHDTextBox.Size = new System.Drawing.Size(577, 20);
            this.VHDTextBox.TabIndex = 0;
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(434, 39);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(75, 23);
            this.button1.TabIndex = 1;
            this.button1.Text = "Mount";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.MountButton_Click);
            // 
            // button2
            // 
            this.button2.Location = new System.Drawing.Point(515, 39);
            this.button2.Name = "button2";
            this.button2.Size = new System.Drawing.Size(75, 23);
            this.button2.TabIndex = 2;
            this.button2.Text = "Unmount";
            this.button2.UseVisualStyleBackColor = true;
            this.button2.Click += new System.EventHandler(this.UnmountButton_Click);
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(602, 124);
            this.Controls.Add(this.button2);
            this.Controls.Add(this.button1);
            this.Controls.Add(this.VHDTextBox);
            this.Name = "Form1";
            this.Text = "Form1";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox VHDTextBox;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.Button button2;
    }
}