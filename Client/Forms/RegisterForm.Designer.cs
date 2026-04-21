namespace Client.Forms
{
    partial class RegisterForm
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
            _registerButton = new Button();
            _haveAccountLinkLabel = new LinkLabel();
            _usernameTextBox = new TextBox();
            _passwordTextBox = new TextBox();
            _confirmPasswordTextBox = new TextBox();
            _labelUsername = new Label();
            lbl_password = new Label();
            lbl_confirm_password = new Label();
            SuspendLayout();
            // 
            // _registerButton
            // 
            _registerButton.Location = new Point(263, 192);
            _registerButton.Margin = new Padding(3, 4, 3, 4);
            _registerButton.Name = "_registerButton";
            _registerButton.Size = new Size(86, 31);
            _registerButton.TabIndex = 0;
            _registerButton.Text = "Registar";
            _registerButton.UseVisualStyleBackColor = true;
            _registerButton.Click += ButtonRegister_Click;
            // 
            // _haveAccountLinkLabel
            // 
            _haveAccountLinkLabel.AutoSize = true;
            _haveAccountLinkLabel.Location = new Point(14, 197);
            _haveAccountLinkLabel.Name = "_haveAccountLinkLabel";
            _haveAccountLinkLabel.Size = new Size(105, 20);
            _haveAccountLinkLabel.TabIndex = 1;
            _haveAccountLinkLabel.TabStop = true;
            _haveAccountLinkLabel.Text = "Já tenho conta";
            _haveAccountLinkLabel.LinkClicked += HaveAccountLinkLabel_LinkClicked;
            // 
            // _usernameTextBox
            // 
            _usernameTextBox.Location = new Point(14, 36);
            _usernameTextBox.Margin = new Padding(3, 4, 3, 4);
            _usernameTextBox.Name = "_usernameTextBox";
            _usernameTextBox.Size = new Size(334, 27);
            _usernameTextBox.TabIndex = 4;
            // 
            // _passwordTextBox
            // 
            _passwordTextBox.Location = new Point(14, 95);
            _passwordTextBox.Margin = new Padding(3, 4, 3, 4);
            _passwordTextBox.Name = "_passwordTextBox";
            _passwordTextBox.PasswordChar = '*';
            _passwordTextBox.Size = new Size(334, 27);
            _passwordTextBox.TabIndex = 7;
            _passwordTextBox.UseSystemPasswordChar = true;
            // 
            // _confirmPasswordTextBox
            // 
            _confirmPasswordTextBox.Location = new Point(14, 153);
            _confirmPasswordTextBox.Margin = new Padding(3, 4, 3, 4);
            _confirmPasswordTextBox.Name = "_confirmPasswordTextBox";
            _confirmPasswordTextBox.PasswordChar = '*';
            _confirmPasswordTextBox.Size = new Size(334, 27);
            _confirmPasswordTextBox.TabIndex = 8;
            _confirmPasswordTextBox.UseSystemPasswordChar = true;
            // 
            // _labelUsername
            // 
            _labelUsername.AutoSize = true;
            _labelUsername.Location = new Point(14, 12);
            _labelUsername.Name = "_labelUsername";
            _labelUsername.Size = new Size(75, 20);
            _labelUsername.TabIndex = 10;
            _labelUsername.Text = "Username";
            // 
            // lbl_password
            // 
            lbl_password.AutoSize = true;
            lbl_password.Location = new Point(14, 71);
            lbl_password.Name = "lbl_password";
            lbl_password.Size = new Size(70, 20);
            lbl_password.TabIndex = 13;
            lbl_password.Text = "Password";
            // 
            // lbl_confirm_password
            // 
            lbl_confirm_password.AutoSize = true;
            lbl_confirm_password.Location = new Point(14, 129);
            lbl_confirm_password.Name = "lbl_confirm_password";
            lbl_confirm_password.Size = new Size(140, 20);
            lbl_confirm_password.TabIndex = 14;
            lbl_confirm_password.Text = "Confirmar Password";
            // 
            // RegisterForm
            // 
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(362, 235);
            Controls.Add(lbl_confirm_password);
            Controls.Add(lbl_password);
            Controls.Add(_labelUsername);
            Controls.Add(_confirmPasswordTextBox);
            Controls.Add(_passwordTextBox);
            Controls.Add(_usernameTextBox);
            Controls.Add(_haveAccountLinkLabel);
            Controls.Add(_registerButton);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            Margin = new Padding(3, 4, 3, 4);
            Name = "RegisterForm";
            StartPosition = FormStartPosition.CenterParent;
            Text = "Register";
            FormClosing += RegisterForm_FormClosing;
            Load += RegisterForm_Load;
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Button _registerButton;
        private LinkLabel _haveAccountLinkLabel;
        private TextBox _usernameTextBox;
        private TextBox _passwordTextBox;
        private TextBox _confirmPasswordTextBox;
        private Label _labelUsername;
        private Label lbl_password;
        private Label lbl_confirm_password;
    }
}