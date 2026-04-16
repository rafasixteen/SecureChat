namespace Client.Forms
{
    partial class LoginForm
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
            _textBoxUsername = new TextBox();
            _textBoxPassword = new TextBox();
            _loginButton = new Button();
            _createAccountLinkLabel = new LinkLabel();
            lbl_username = new Label();
            lbl_password = new Label();
            SuspendLayout();
            // 
            // _textBoxUsername
            // 
            _textBoxUsername.Location = new Point(9, 27);
            _textBoxUsername.Name = "_textBoxUsername";
            _textBoxUsername.Size = new Size(291, 23);
            _textBoxUsername.TabIndex = 0;
            // 
            // _textBoxPassword
            // 
            _textBoxPassword.Location = new Point(9, 71);
            _textBoxPassword.Name = "_textBoxPassword";
            _textBoxPassword.PasswordChar = '*';
            _textBoxPassword.Size = new Size(291, 23);
            _textBoxPassword.TabIndex = 1;
            _textBoxPassword.UseSystemPasswordChar = true;
            // 
            // _loginButton
            // 
            _loginButton.Location = new Point(190, 100);
            _loginButton.Name = "_loginButton";
            _loginButton.Size = new Size(110, 23);
            _loginButton.TabIndex = 2;
            _loginButton.Text = "Log In";
            _loginButton.UseVisualStyleBackColor = true;
            _loginButton.Click += LoginButton_Click;
            // 
            // _createAccountLinkLabel
            // 
            _createAccountLinkLabel.AutoSize = true;
            _createAccountLinkLabel.Location = new Point(9, 104);
            _createAccountLinkLabel.Name = "_createAccountLinkLabel";
            _createAccountLinkLabel.Size = new Size(65, 15);
            _createAccountLinkLabel.TabIndex = 6;
            _createAccountLinkLabel.TabStop = true;
            _createAccountLinkLabel.Text = "Criar conta";
            _createAccountLinkLabel.LinkClicked += CreateAccountLinkLabel_LinkClicked;
            // 
            // lbl_username
            // 
            lbl_username.AutoSize = true;
            lbl_username.Location = new Point(9, 9);
            lbl_username.Name = "lbl_username";
            lbl_username.Size = new Size(60, 15);
            lbl_username.TabIndex = 7;
            lbl_username.Text = "Username";
            // 
            // lbl_password
            // 
            lbl_password.AutoSize = true;
            lbl_password.Location = new Point(9, 53);
            lbl_password.Name = "lbl_password";
            lbl_password.Size = new Size(57, 15);
            lbl_password.TabIndex = 8;
            lbl_password.Text = "Password";
            // 
            // LoginForm
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(310, 132);
            Controls.Add(lbl_password);
            Controls.Add(lbl_username);
            Controls.Add(_createAccountLinkLabel);
            Controls.Add(_loginButton);
            Controls.Add(_textBoxPassword);
            Controls.Add(_textBoxUsername);
            Name = "LoginForm";
            Text = "Secure Chat - Login";
            Load += LoginForm_Load;
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private TextBox _textBoxUsername;
        private TextBox _textBoxPassword;
        private Button _loginButton;
        private LinkLabel _createAccountLinkLabel;
        private Label lbl_username;
        private Label lbl_password;
    }
}