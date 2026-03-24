namespace SecureChat.Client
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
            _textBoxInput = new TextBox();
            _textBoxChat = new RichTextBox();
            _sendButton = new Button();
            SuspendLayout();
            // 
            // _textBoxInput
            // 
            _textBoxInput.Location = new Point(12, 505);
            _textBoxInput.Name = "_textBoxInput";
            _textBoxInput.Size = new Size(630, 27);
            _textBoxInput.TabIndex = 0;
            _textBoxInput.KeyDown += OnTextBoxInputKeyDown;
            // 
            // _textBoxChat
            // 
            _textBoxChat.Location = new Point(12, 12);
            _textBoxChat.Name = "_textBoxChat";
            _textBoxChat.ReadOnly = true;
            _textBoxChat.Size = new Size(760, 480);
            _textBoxChat.TabIndex = 1;
            _textBoxChat.Text = "";
            // 
            // _sendButton
            // 
            _sendButton.Location = new Point(685, 503);
            _sendButton.Name = "_sendButton";
            _sendButton.Size = new Size(87, 32);
            _sendButton.TabIndex = 2;
            _sendButton.Text = "Send";
            _sendButton.UseVisualStyleBackColor = true;
            _sendButton.Click += OnSendButtonClick;
            // 
            // ChatForm
            // 
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(788, 549);
            Controls.Add(_sendButton);
            Controls.Add(_textBoxChat);
            Controls.Add(_textBoxInput);
            Name = "ChatForm";
            Text = "ChatForm";
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private TextBox _textBoxInput;
        private RichTextBox _textBoxChat;
        private Button _sendButton;
    }
}