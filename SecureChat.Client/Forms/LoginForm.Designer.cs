namespace SecureChat.Client
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
            _labelTitle = new Label();
            _labelServer = new Label();
            _labelUsername = new Label();
            _labelPassword = new Label();
            _textBoxServer = new TextBox();
            _textBoxUsername = new TextBox();
            _textBoxPassword = new TextBox();
            _buttonLogin = new Button();
            SuspendLayout();
            // 
            // _labelTitle
            // 
            _labelTitle.AutoSize = true;
            _labelTitle.Location = new Point(100, 20);
            _labelTitle.Name = "_labelTitle";
            _labelTitle.Size = new Size(87, 20);
            _labelTitle.TabIndex = 0;
            _labelTitle.Text = "Secure Chat";
            // 
            // _labelServer
            // 
            _labelServer.AutoSize = true;
            _labelServer.Location = new Point(30, 75);
            _labelServer.Name = "_labelServer";
            _labelServer.Size = new Size(69, 20);
            _labelServer.TabIndex = 1;
            _labelServer.Text = "Server IP:";
            // 
            // _labelUsername
            // 
            _labelUsername.AutoSize = true;
            _labelUsername.Location = new Point(30, 115);
            _labelUsername.Name = "_labelUsername";
            _labelUsername.Size = new Size(78, 20);
            _labelUsername.TabIndex = 2;
            _labelUsername.Text = "Username:";
            // 
            // _labelPassword
            // 
            _labelPassword.AutoSize = true;
            _labelPassword.Location = new Point(30, 155);
            _labelPassword.Name = "_labelPassword";
            _labelPassword.Size = new Size(73, 20);
            _labelPassword.TabIndex = 3;
            _labelPassword.Text = "Password:";
            // 
            // _textBoxServer
            // 
            _textBoxServer.Location = new Point(120, 72);
            _textBoxServer.Name = "_textBoxServer";
            _textBoxServer.PlaceholderText = "127.0.0.1";
            _textBoxServer.Size = new Size(200, 27);
            _textBoxServer.TabIndex = 4;
            // 
            // _textBoxUsername
            // 
            _textBoxUsername.Location = new Point(120, 112);
            _textBoxUsername.Name = "_textBoxUsername";
            _textBoxUsername.Size = new Size(200, 27);
            _textBoxUsername.TabIndex = 5;
            // 
            // _textBoxPassword
            // 
            _textBoxPassword.Location = new Point(120, 152);
            _textBoxPassword.Name = "_textBoxPassword";
            _textBoxPassword.PasswordChar = '*';
            _textBoxPassword.Size = new Size(200, 27);
            _textBoxPassword.TabIndex = 6;
            _textBoxPassword.UseSystemPasswordChar = true;
            // 
            // _buttonLogin
            // 
            _buttonLogin.Location = new Point(120, 195);
            _buttonLogin.Name = "_buttonLogin";
            _buttonLogin.Size = new Size(94, 29);
            _buttonLogin.TabIndex = 7;
            _buttonLogin.Text = "Login";
            _buttonLogin.UseVisualStyleBackColor = true;
            _buttonLogin.Click += ButtonLoginOnClick;
            // 
            // LoginForm
            // 
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(352, 243);
            Controls.Add(_buttonLogin);
            Controls.Add(_textBoxPassword);
            Controls.Add(_textBoxUsername);
            Controls.Add(_textBoxServer);
            Controls.Add(_labelPassword);
            Controls.Add(_labelUsername);
            Controls.Add(_labelServer);
            Controls.Add(_labelTitle);
            Name = "LoginForm";
            Text = "SecureChat - Login";
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Label _labelTitle;
        private Label _labelServer;
        private Label _labelUsername;
        private Label _labelPassword;
        private TextBox _textBoxServer;
        private TextBox _textBoxUsername;
        private TextBox _textBoxPassword;
        private Button _buttonLogin;
    }
}