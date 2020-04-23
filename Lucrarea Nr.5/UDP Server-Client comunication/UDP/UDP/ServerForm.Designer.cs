namespace UDP
{
    partial class ServerForm
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
               this.label1 = new System.Windows.Forms.Label();
               this.txtServerPort = new System.Windows.Forms.TextBox();
               this.label4 = new System.Windows.Forms.Label();
               this.txtLog = new System.Windows.Forms.RichTextBox();
               this.btnStartServer = new System.Windows.Forms.Button();
               this.btnStop = new System.Windows.Forms.Button();
               this.SuspendLayout();
               // 
               // label1
               // 
               this.label1.AutoSize = true;
               this.label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 7.8F);
               this.label1.Location = new System.Drawing.Point(16, 9);
               this.label1.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
               this.label1.Name = "label1";
               this.label1.Size = new System.Drawing.Size(88, 17);
               this.label1.TabIndex = 1;
               this.label1.Text = "Server Port :";
               // 
               // txtServerPort
               // 
               this.txtServerPort.Location = new System.Drawing.Point(112, 6);
               this.txtServerPort.Margin = new System.Windows.Forms.Padding(4);
               this.txtServerPort.Name = "txtServerPort";
               this.txtServerPort.Size = new System.Drawing.Size(216, 22);
               this.txtServerPort.TabIndex = 4;
               this.txtServerPort.Text = "5678";
               // 
               // label4
               // 
               this.label4.AutoSize = true;
               this.label4.Font = new System.Drawing.Font("Microsoft Sans Serif", 7.8F);
               this.label4.Location = new System.Drawing.Point(16, 46);
               this.label4.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
               this.label4.Name = "label4";
               this.label4.Size = new System.Drawing.Size(73, 17);
               this.label4.TabIndex = 7;
               this.label4.Text = "Message :";
               // 
               // txtLog
               // 
               this.txtLog.BackColor = System.Drawing.SystemColors.Window;
               this.txtLog.Location = new System.Drawing.Point(19, 78);
               this.txtLog.Margin = new System.Windows.Forms.Padding(4);
               this.txtLog.Name = "txtLog";
               this.txtLog.ReadOnly = true;
               this.txtLog.Size = new System.Drawing.Size(309, 266);
               this.txtLog.TabIndex = 8;
               this.txtLog.Text = "";
               // 
               // btnStartServer
               // 
               this.btnStartServer.Cursor = System.Windows.Forms.Cursors.Hand;
               this.btnStartServer.Font = new System.Drawing.Font("Microsoft Sans Serif", 7.8F);
               this.btnStartServer.Location = new System.Drawing.Point(51, 352);
               this.btnStartServer.Margin = new System.Windows.Forms.Padding(4);
               this.btnStartServer.Name = "btnStartServer";
               this.btnStartServer.Size = new System.Drawing.Size(96, 33);
               this.btnStartServer.TabIndex = 12;
               this.btnStartServer.Text = "Start Server";
               this.btnStartServer.UseVisualStyleBackColor = true;
               this.btnStartServer.Click += new System.EventHandler(this.btnStartServer_Click);
               // 
               // btnStop
               // 
               this.btnStop.Cursor = System.Windows.Forms.Cursors.Hand;
               this.btnStop.Font = new System.Drawing.Font("Microsoft Sans Serif", 7.8F);
               this.btnStop.Location = new System.Drawing.Point(194, 352);
               this.btnStop.Margin = new System.Windows.Forms.Padding(4);
               this.btnStop.Name = "btnStop";
               this.btnStop.Size = new System.Drawing.Size(96, 33);
               this.btnStop.TabIndex = 13;
               this.btnStop.Text = "Stop Server";
               this.btnStop.UseVisualStyleBackColor = true;
               this.btnStop.Click += new System.EventHandler(this.btnStop_Click);
               // 
               // ServerForm
               // 
               this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
               this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
               this.BackColor = System.Drawing.SystemColors.Window;
               this.ClientSize = new System.Drawing.Size(350, 403);
               this.Controls.Add(this.btnStop);
               this.Controls.Add(this.btnStartServer);
               this.Controls.Add(this.txtLog);
               this.Controls.Add(this.label4);
               this.Controls.Add(this.txtServerPort);
               this.Controls.Add(this.label1);
               this.Margin = new System.Windows.Forms.Padding(4);
               this.Name = "ServerForm";
               this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
               this.Text = "Server";
               this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.ServerForm_FormClosing);
               this.ResumeLayout(false);
               this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox txtServerPort;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.RichTextBox txtLog;
        private System.Windows.Forms.Button btnStartServer;
        private System.Windows.Forms.Button btnStop;
    }
}

