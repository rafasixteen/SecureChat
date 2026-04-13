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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ChatForm));
            _sendButton = new Button();
            _textBoxMessage = new TextBox();
            _chat = new FlowLayoutPanel();
            _usersListView = new ListView();
            toolStrip1 = new ToolStrip();
            toolStripButton1 = new ToolStripButton();
            _loginButton = new ToolStripButton();
            toolStrip1.SuspendLayout();
            SuspendLayout();
            // 
            // _sendButton
            // 
            _sendButton.Location = new Point(643, 416);
            _sendButton.Name = "_sendButton";
            _sendButton.Size = new Size(75, 23);
            _sendButton.TabIndex = 0;
            _sendButton.Text = "Enviar";
            _sendButton.UseVisualStyleBackColor = true;
            _sendButton.Click += SendButton_Click;
            // 
            // _textBoxMessage
            // 
            _textBoxMessage.Location = new Point(129, 416);
            _textBoxMessage.Name = "_textBoxMessage";
            _textBoxMessage.Size = new Size(508, 23);
            _textBoxMessage.TabIndex = 1;
            // 
            // _chat
            // 
            _chat.AutoScroll = true;
            _chat.FlowDirection = FlowDirection.TopDown;
            _chat.Location = new Point(129, 27);
            _chat.Name = "_chat";
            _chat.Size = new Size(589, 383);
            _chat.TabIndex = 2;
            // 
            // _usersListView
            // 
            _usersListView.Location = new Point(2, 27);
            _usersListView.Name = "_usersListView";
            _usersListView.Size = new Size(121, 412);
            _usersListView.TabIndex = 3;
            _usersListView.UseCompatibleStateImageBehavior = false;
            // 
            // toolStrip1
            // 
            toolStrip1.Items.AddRange(new ToolStripItem[] { toolStripButton1, _loginButton });
            toolStrip1.Location = new Point(0, 0);
            toolStrip1.Name = "toolStrip1";
            toolStrip1.Size = new Size(730, 25);
            toolStrip1.TabIndex = 4;
            toolStrip1.Text = "toolStrip1";
            // 
            // toolStripButton1
            // 
            toolStripButton1.DisplayStyle = ToolStripItemDisplayStyle.Text;
            toolStripButton1.Image = (Image)resources.GetObject("toolStripButton1.Image");
            toolStripButton1.ImageTransparentColor = Color.Magenta;
            toolStripButton1.Name = "toolStripButton1";
            toolStripButton1.Size = new Size(69, 22);
            toolStripButton1.Text = "Add Friend";
            // 
            // _loginButton
            // 
            _loginButton.DisplayStyle = ToolStripItemDisplayStyle.Text;
            _loginButton.Image = (Image)resources.GetObject("_loginButton.Image");
            _loginButton.ImageTransparentColor = Color.Magenta;
            _loginButton.Name = "_loginButton";
            _loginButton.Size = new Size(41, 22);
            _loginButton.Text = "Login";
            _loginButton.Click += LoginButton_Click;
            // 
            // ChatForm
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(730, 450);
            Controls.Add(toolStrip1);
            Controls.Add(_usersListView);
            Controls.Add(_chat);
            Controls.Add(_textBoxMessage);
            Controls.Add(_sendButton);
            Name = "ChatForm";
            Text = "Secure Chat";
            FormClosing += ChatForm_FormClosing;
            Load += ChatForm_Load;
            toolStrip1.ResumeLayout(false);
            toolStrip1.PerformLayout();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Button _sendButton;
        private TextBox _textBoxMessage;
        private FlowLayoutPanel _chat;
        private ListView _usersListView;
        private ToolStrip toolStrip1;
        private ToolStripButton toolStripButton1;
        private ToolStripButton _loginButton;
    }
}