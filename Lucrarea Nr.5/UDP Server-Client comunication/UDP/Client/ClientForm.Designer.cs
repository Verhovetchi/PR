namespace UDP
{
    partial class ClientForm
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
               this.label2 = new System.Windows.Forms.Label();
               this.label3 = new System.Windows.Forms.Label();
               this.txtServerPort = new System.Windows.Forms.TextBox();
               this.txtClientPort = new System.Windows.Forms.TextBox();
               this.txtHostName = new System.Windows.Forms.TextBox();
               this.txtLog = new System.Windows.Forms.RichTextBox();
               this.txtMsg = new System.Windows.Forms.TextBox();
               this.btnSend = new System.Windows.Forms.Button();
               this.btnConnect = new System.Windows.Forms.Button();
               this.label4 = new System.Windows.Forms.Label();
               this.SuspendLayout();
               // 
               // label1
               // 
               this.label1.AutoSize = true;
               this.label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 7.8F);
               this.label1.Location = new System.Drawing.Point(6, 16);
               this.label1.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
               this.label1.Name = "label1";
               this.label1.Size = new System.Drawing.Size(88, 17);
               this.label1.TabIndex = 1;
               this.label1.Text = "Server Port :";
               // 
               // label2
               // 
               this.label2.AutoSize = true;
               this.label2.Font = new System.Drawing.Font("Microsoft Sans Serif", 7.8F);
               this.label2.Location = new System.Drawing.Point(13, 43);
               this.label2.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
               this.label2.Name = "label2";
               this.label2.Size = new System.Drawing.Size(81, 17);
               this.label2.TabIndex = 2;
               this.label2.Text = "Client Port :";
               // 
               // label3
               // 
               this.label3.AutoSize = true;
               this.label3.Font = new System.Drawing.Font("Microsoft Sans Serif", 7.8F);
               this.label3.Location = new System.Drawing.Point(53, 76);
               this.label3.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
               this.label3.Name = "label3";
               this.label3.Size = new System.Drawing.Size(41, 17);
               this.label3.TabIndex = 3;
               this.label3.Text = "Host:";
               // 
               // txtServerPort
               // 
               this.txtServerPort.Location = new System.Drawing.Point(102, 13);
               this.txtServerPort.Margin = new System.Windows.Forms.Padding(4);
               this.txtServerPort.Name = "txtServerPort";
               this.txtServerPort.Size = new System.Drawing.Size(216, 22);
               this.txtServerPort.TabIndex = 4;
               this.txtServerPort.Text = "5678";
               // 
               // txtClientPort
               // 
               this.txtClientPort.Location = new System.Drawing.Point(102, 43);
               this.txtClientPort.Margin = new System.Windows.Forms.Padding(4);
               this.txtClientPort.Name = "txtClientPort";
               this.txtClientPort.Size = new System.Drawing.Size(216, 22);
               this.txtClientPort.TabIndex = 5;
               this.txtClientPort.Text = "1234";
               // 
               // txtHostName
               // 
               this.txtHostName.Location = new System.Drawing.Point(102, 73);
               this.txtHostName.Margin = new System.Windows.Forms.Padding(4);
               this.txtHostName.Name = "txtHostName";
               this.txtHostName.Size = new System.Drawing.Size(216, 22);
               this.txtHostName.TabIndex = 6;
               this.txtHostName.Text = "localhost";
               // 
               // txtLog
               // 
               this.txtLog.BackColor = System.Drawing.SystemColors.Window;
               this.txtLog.Location = new System.Drawing.Point(9, 134);
               this.txtLog.Margin = new System.Windows.Forms.Padding(4);
               this.txtLog.Name = "txtLog";
               this.txtLog.ReadOnly = true;
               this.txtLog.Size = new System.Drawing.Size(386, 256);
               this.txtLog.TabIndex = 8;
               this.txtLog.Text = "";
               // 
               // txtMsg
               // 
               this.txtMsg.Location = new System.Drawing.Point(102, 103);
               this.txtMsg.Margin = new System.Windows.Forms.Padding(4);
               this.txtMsg.Name = "txtMsg";
               this.txtMsg.Size = new System.Drawing.Size(216, 22);
               this.txtMsg.TabIndex = 10;
               // 
               // btnSend
               // 
               this.btnSend.Cursor = System.Windows.Forms.Cursors.Hand;
               this.btnSend.Enabled = false;
               this.btnSend.Font = new System.Drawing.Font("Microsoft Sans Serif", 7.8F);
               this.btnSend.Location = new System.Drawing.Point(326, 104);
               this.btnSend.Margin = new System.Windows.Forms.Padding(4);
               this.btnSend.Name = "btnSend";
               this.btnSend.Size = new System.Drawing.Size(69, 22);
               this.btnSend.TabIndex = 11;
               this.btnSend.Text = "Send";
               this.btnSend.UseVisualStyleBackColor = true;
               this.btnSend.Click += new System.EventHandler(this.btnSend_Click);
               // 
               // btnConnect
               // 
               this.btnConnect.Cursor = System.Windows.Forms.Cursors.Hand;
               this.btnConnect.Font = new System.Drawing.Font("Microsoft Sans Serif", 7.8F);
               this.btnConnect.Location = new System.Drawing.Point(326, 73);
               this.btnConnect.Margin = new System.Windows.Forms.Padding(4);
               this.btnConnect.Name = "btnConnect";
               this.btnConnect.Size = new System.Drawing.Size(69, 23);
               this.btnConnect.TabIndex = 12;
               this.btnConnect.Text = "Connect";
               this.btnConnect.UseVisualStyleBackColor = true;
               this.btnConnect.Click += new System.EventHandler(this.btnConnect_Click);
               // 
               // label4
               // 
               this.label4.AutoSize = true;
               this.label4.Font = new System.Drawing.Font("Microsoft Sans Serif", 7.8F);
               this.label4.Location = new System.Drawing.Point(21, 106);
               this.label4.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
               this.label4.Name = "label4";
               this.label4.Size = new System.Drawing.Size(73, 17);
               this.label4.TabIndex = 7;
               this.label4.Text = "Message :";
               // 
               // ClientForm
               // 
               this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
               this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
               this.BackColor = System.Drawing.SystemColors.Window;
               this.ClientSize = new System.Drawing.Size(409, 400);
               this.Controls.Add(this.btnConnect);
               this.Controls.Add(this.btnSend);
               this.Controls.Add(this.txtMsg);
               this.Controls.Add(this.txtLog);
               this.Controls.Add(this.label4);
               this.Controls.Add(this.txtHostName);
               this.Controls.Add(this.txtClientPort);
               this.Controls.Add(this.txtServerPort);
               this.Controls.Add(this.label3);
               this.Controls.Add(this.label2);
               this.Controls.Add(this.label1);
               this.Margin = new System.Windows.Forms.Padding(4);
               this.Name = "ClientForm";
               this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
               this.Text = "Client";
               this.ResumeLayout(false);
               this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TextBox txtServerPort;
        private System.Windows.Forms.TextBox txtClientPort;
        private System.Windows.Forms.TextBox txtHostName;
        private System.Windows.Forms.RichTextBox txtLog;
        private System.Windows.Forms.TextBox txtMsg;
        private System.Windows.Forms.Button btnSend;
        private System.Windows.Forms.Button btnConnect;
          private System.Windows.Forms.Label label4;
     }
}

