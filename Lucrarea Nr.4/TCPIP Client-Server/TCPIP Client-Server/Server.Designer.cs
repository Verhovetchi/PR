namespace TCPIP_Client_Server
{
     partial class Server
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
               this.txtHost = new System.Windows.Forms.TextBox();
               this.txtPort = new System.Windows.Forms.TextBox();
               this.label1 = new System.Windows.Forms.Label();
               this.label2 = new System.Windows.Forms.Label();
               this.btnStart = new System.Windows.Forms.Button();
               this.btnStop = new System.Windows.Forms.Button();
               this.txtStatus = new System.Windows.Forms.TextBox();
               this.SuspendLayout();
               // 
               // txtHost
               // 
               this.txtHost.Location = new System.Drawing.Point(59, 6);
               this.txtHost.Name = "txtHost";
               this.txtHost.Size = new System.Drawing.Size(100, 22);
               this.txtHost.TabIndex = 0;
               this.txtHost.Text = "127.0.0.1";
               // 
               // txtPort
               // 
               this.txtPort.Location = new System.Drawing.Point(223, 6);
               this.txtPort.Name = "txtPort";
               this.txtPort.Size = new System.Drawing.Size(100, 22);
               this.txtPort.TabIndex = 1;
               this.txtPort.Text = "8910";
               // 
               // label1
               // 
               this.label1.AutoSize = true;
               this.label1.Location = new System.Drawing.Point(12, 9);
               this.label1.Name = "label1";
               this.label1.Size = new System.Drawing.Size(41, 17);
               this.label1.TabIndex = 2;
               this.label1.Text = "Host:";
               // 
               // label2
               // 
               this.label2.AutoSize = true;
               this.label2.Location = new System.Drawing.Point(179, 9);
               this.label2.Name = "label2";
               this.label2.Size = new System.Drawing.Size(38, 17);
               this.label2.TabIndex = 3;
               this.label2.Text = "Port:";
               // 
               // btnStart
               // 
               this.btnStart.Location = new System.Drawing.Point(349, 6);
               this.btnStart.Name = "btnStart";
               this.btnStart.Size = new System.Drawing.Size(75, 23);
               this.btnStart.TabIndex = 4;
               this.btnStart.Text = "Start";
               this.btnStart.UseVisualStyleBackColor = true;
               this.btnStart.Click += new System.EventHandler(this.btnStart_Click);
               // 
               // btnStop
               // 
               this.btnStop.Location = new System.Drawing.Point(430, 6);
               this.btnStop.Name = "btnStop";
               this.btnStop.Size = new System.Drawing.Size(75, 23);
               this.btnStop.TabIndex = 5;
               this.btnStop.Text = "Stop";
               this.btnStop.UseVisualStyleBackColor = true;
               this.btnStop.Click += new System.EventHandler(this.btnStop_Click);
               // 
               // txtStatus
               // 
               this.txtStatus.Location = new System.Drawing.Point(15, 48);
               this.txtStatus.Multiline = true;
               this.txtStatus.Name = "txtStatus";
               this.txtStatus.Size = new System.Drawing.Size(490, 302);
               this.txtStatus.TabIndex = 6;
               // 
               // Server
               // 
               this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
               this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
               this.ClientSize = new System.Drawing.Size(518, 366);
               this.Controls.Add(this.txtStatus);
               this.Controls.Add(this.btnStop);
               this.Controls.Add(this.btnStart);
               this.Controls.Add(this.label2);
               this.Controls.Add(this.label1);
               this.Controls.Add(this.txtPort);
               this.Controls.Add(this.txtHost);
               this.Name = "Server";
               this.Text = "Server";
               this.Load += new System.EventHandler(this.Server_Load);
               this.ResumeLayout(false);
               this.PerformLayout();

          }

        #endregion

        private System.Windows.Forms.TextBox txtHost;
        private System.Windows.Forms.TextBox txtPort;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Button btnStart;
        private System.Windows.Forms.Button btnStop;
        private System.Windows.Forms.TextBox txtStatus;
    }
}

