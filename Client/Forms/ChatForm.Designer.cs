namespace Client.Forms
{
    partial class ChatForm
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
            groupBox1 = new GroupBox();
            _friendsList = new ListBox();
            _usernameLabel = new Label();
            groupBox1.SuspendLayout();
            SuspendLayout();
            // 
            // groupBox1
            // 
            groupBox1.Controls.Add(_friendsList);
            groupBox1.Location = new Point(12, 12);
            groupBox1.Name = "groupBox1";
            groupBox1.Size = new Size(192, 576);
            groupBox1.TabIndex = 2;
            groupBox1.TabStop = false;
            groupBox1.Text = "Friends";
            // 
            // _friendsList
            // 
            _friendsList.FormattingEnabled = true;
            _friendsList.Location = new Point(6, 26);
            _friendsList.Name = "_friendsList";
            _friendsList.Size = new Size(180, 544);
            _friendsList.TabIndex = 0;
            // 
            // _usernameLabel
            // 
            _usernameLabel.AutoSize = true;
            _usernameLabel.Location = new Point(720, 9);
            _usernameLabel.Name = "_usernameLabel";
            _usernameLabel.RightToLeft = RightToLeft.No;
            _usernameLabel.Size = new Size(102, 20);
            _usernameLabel.TabIndex = 3;
            _usernameLabel.Text = "Not logged in";
            _usernameLabel.TextAlign = ContentAlignment.MiddleRight;
            // 
            // ChatForm
            // 
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(834, 600);
            Controls.Add(_usernameLabel);
            Controls.Add(groupBox1);
            Margin = new Padding(3, 4, 3, 4);
            Name = "ChatForm";
            Text = "Secure Chat";
            FormClosing += ChatForm_FormClosing;
            Load += ChatForm_Load;
            groupBox1.ResumeLayout(false);
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion
        private GroupBox groupBox1;
        private ListBox _friendsList;
        private Label _usernameLabel;
    }
}