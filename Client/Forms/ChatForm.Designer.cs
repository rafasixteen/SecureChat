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
            groupBox2 = new GroupBox();
            _chatPanel = new FlowLayoutPanel();
            _sendButton = new Button();
            _messageTextBox = new TextBox();
            _authButton = new Button();
            groupBox1.SuspendLayout();
            groupBox2.SuspendLayout();
            SuspendLayout();
            // 
            // groupBox1
            // 
            groupBox1.Controls.Add(_friendsList);
            groupBox1.Location = new Point(12, 35);
            groupBox1.Name = "groupBox1";
            groupBox1.Size = new Size(192, 499);
            groupBox1.TabIndex = 2;
            groupBox1.TabStop = false;
            groupBox1.Text = "Friends";
            // 
            // _friendsList
            // 
            _friendsList.FormattingEnabled = true;
            _friendsList.Location = new Point(6, 26);
            _friendsList.Name = "_friendsList";
            _friendsList.Size = new Size(180, 464);
            _friendsList.TabIndex = 0;
            _friendsList.SelectedIndexChanged += FriendsList_SelectedIndexChanged;
            // 
            // _usernameLabel
            // 
            _usernameLabel.AutoSize = true;
            _usernameLabel.Location = new Point(18, 12);
            _usernameLabel.Name = "_usernameLabel";
            _usernameLabel.RightToLeft = RightToLeft.No;
            _usernameLabel.Size = new Size(102, 20);
            _usernameLabel.TabIndex = 3;
            _usernameLabel.Text = "Not logged in";
            _usernameLabel.TextAlign = ContentAlignment.MiddleRight;
            // 
            // groupBox2
            // 
            groupBox2.Controls.Add(_chatPanel);
            groupBox2.Location = new Point(210, 35);
            groupBox2.Name = "groupBox2";
            groupBox2.Size = new Size(738, 463);
            groupBox2.TabIndex = 4;
            groupBox2.TabStop = false;
            groupBox2.Text = "Chat";
            // 
            // _chatPanel
            // 
            _chatPanel.AutoScroll = true;
            _chatPanel.Dock = DockStyle.Fill;
            _chatPanel.FlowDirection = FlowDirection.TopDown;
            _chatPanel.Location = new Point(3, 23);
            _chatPanel.Name = "_chatPanel";
            _chatPanel.Size = new Size(732, 437);
            _chatPanel.TabIndex = 0;
            _chatPanel.WrapContents = false;
            // 
            // _sendButton
            // 
            _sendButton.Location = new Point(840, 501);
            _sendButton.Name = "_sendButton";
            _sendButton.Size = new Size(105, 29);
            _sendButton.TabIndex = 2;
            _sendButton.Text = "Send";
            _sendButton.UseVisualStyleBackColor = true;
            _sendButton.Click += SendButton_Click;
            // 
            // _messageTextBox
            // 
            _messageTextBox.Location = new Point(210, 503);
            _messageTextBox.Name = "_messageTextBox";
            _messageTextBox.Size = new Size(627, 27);
            _messageTextBox.TabIndex = 1;
            _messageTextBox.KeyPress += MessageTextBox_KeyPress;
            // 
            // _authButton
            // 
            _authButton.Location = new Point(854, 12);
            _authButton.Name = "_authButton";
            _authButton.Size = new Size(94, 29);
            _authButton.TabIndex = 5;
            _authButton.Text = "Login";
            _authButton.UseVisualStyleBackColor = true;
            _authButton.Click += AuthButton_Click;
            // 
            // ChatForm
            // 
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(960, 540);
            Controls.Add(_authButton);
            Controls.Add(_messageTextBox);
            Controls.Add(_usernameLabel);
            Controls.Add(_sendButton);
            Controls.Add(groupBox2);
            Controls.Add(groupBox1);
            FormBorderStyle = FormBorderStyle.FixedSingle;
            Margin = new Padding(3, 4, 3, 4);
            MaximizeBox = false;
            Name = "ChatForm";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "Secure Chat";
            FormClosing += ChatForm_FormClosing;
            Load += ChatForm_Load;
            groupBox1.ResumeLayout(false);
            groupBox2.ResumeLayout(false);
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion
        private GroupBox groupBox1;
        private ListBox _friendsList;
        private Label _usernameLabel;
        private GroupBox groupBox2;
        private Button _sendButton;
        private TextBox _messageTextBox;
        private FlowLayoutPanel _chatPanel;
        private Button _authButton;
    }
}