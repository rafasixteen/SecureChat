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
            _buttonRegister = new Button();
            lnklbl_go_login = new LinkLabel();
            btn_see_confirm_password = new Button();
            btn_see_password = new Button();
            _textBoxUsername = new TextBox();
            txtbx_firstname = new TextBox();
            txtbx_lastname = new TextBox();
            txtbx_password = new TextBox();
            txtbx_confirm_password = new TextBox();
            lbl_register = new Label();
            _labelUsername = new Label();
            lbl_firstname = new Label();
            lbl_lastname = new Label();
            lbl_password = new Label();
            lbl_confirm_password = new Label();
            SuspendLayout();
            // 
            // _buttonRegister
            // 
            _buttonRegister.Location = new Point(263, 369);
            _buttonRegister.Margin = new Padding(3, 4, 3, 4);
            _buttonRegister.Name = "_buttonRegister";
            _buttonRegister.Size = new Size(86, 31);
            _buttonRegister.TabIndex = 0;
            _buttonRegister.Text = "Registar";
            _buttonRegister.UseVisualStyleBackColor = true;
            _buttonRegister.Click += ButtonRegister_Click;
            // 
            // lnklbl_go_login
            // 
            lnklbl_go_login.AutoSize = true;
            lnklbl_go_login.Location = new Point(14, 375);
            lnklbl_go_login.Name = "lnklbl_go_login";
            lnklbl_go_login.Size = new Size(105, 20);
            lnklbl_go_login.TabIndex = 1;
            lnklbl_go_login.TabStop = true;
            lnklbl_go_login.Text = "Já tenho conta";
            // 
            // btn_see_confirm_password
            // 
            btn_see_confirm_password.Location = new Point(307, 312);
            btn_see_confirm_password.Margin = new Padding(3, 4, 3, 4);
            btn_see_confirm_password.Name = "btn_see_confirm_password";
            btn_see_confirm_password.Size = new Size(41, 31);
            btn_see_confirm_password.TabIndex = 2;
            btn_see_confirm_password.Text = "Ver";
            btn_see_confirm_password.UseVisualStyleBackColor = true;
            // 
            // btn_see_password
            // 
            btn_see_password.Location = new Point(307, 244);
            btn_see_password.Margin = new Padding(3, 4, 3, 4);
            btn_see_password.Name = "btn_see_password";
            btn_see_password.Size = new Size(41, 31);
            btn_see_password.TabIndex = 3;
            btn_see_password.Text = "Ver";
            btn_see_password.UseVisualStyleBackColor = true;
            // 
            // _textBoxUsername
            // 
            _textBoxUsername.Location = new Point(14, 101);
            _textBoxUsername.Margin = new Padding(3, 4, 3, 4);
            _textBoxUsername.Name = "_textBoxUsername";
            _textBoxUsername.Size = new Size(334, 27);
            _textBoxUsername.TabIndex = 4;
            // 
            // txtbx_firstname
            // 
            txtbx_firstname.Location = new Point(14, 171);
            txtbx_firstname.Margin = new Padding(3, 4, 3, 4);
            txtbx_firstname.Name = "txtbx_firstname";
            txtbx_firstname.Size = new Size(148, 27);
            txtbx_firstname.TabIndex = 5;
            // 
            // txtbx_lastname
            // 
            txtbx_lastname.Location = new Point(200, 171);
            txtbx_lastname.Margin = new Padding(3, 4, 3, 4);
            txtbx_lastname.Name = "txtbx_lastname";
            txtbx_lastname.Size = new Size(148, 27);
            txtbx_lastname.TabIndex = 6;
            // 
            // txtbx_password
            // 
            txtbx_password.Location = new Point(14, 245);
            txtbx_password.Margin = new Padding(3, 4, 3, 4);
            txtbx_password.Name = "txtbx_password";
            txtbx_password.Size = new Size(286, 27);
            txtbx_password.TabIndex = 7;
            // 
            // txtbx_confirm_password
            // 
            txtbx_confirm_password.Location = new Point(14, 312);
            txtbx_confirm_password.Margin = new Padding(3, 4, 3, 4);
            txtbx_confirm_password.Name = "txtbx_confirm_password";
            txtbx_confirm_password.Size = new Size(286, 27);
            txtbx_confirm_password.TabIndex = 8;
            // 
            // lbl_register
            // 
            lbl_register.AutoSize = true;
            lbl_register.Font = new Font("Segoe UI", 20F);
            lbl_register.Location = new Point(117, 12);
            lbl_register.Name = "lbl_register";
            lbl_register.Size = new Size(141, 46);
            lbl_register.TabIndex = 9;
            lbl_register.Text = "Register";
            // 
            // _labelUsername
            // 
            _labelUsername.AutoSize = true;
            _labelUsername.Location = new Point(14, 77);
            _labelUsername.Name = "_labelUsername";
            _labelUsername.Size = new Size(75, 20);
            _labelUsername.TabIndex = 10;
            _labelUsername.Text = "Username";
            // 
            // lbl_firstname
            // 
            lbl_firstname.AutoSize = true;
            lbl_firstname.Location = new Point(14, 147);
            lbl_firstname.Name = "lbl_firstname";
            lbl_firstname.Size = new Size(110, 20);
            lbl_firstname.TabIndex = 11;
            lbl_firstname.Text = "Primeiro Nome";
            // 
            // lbl_lastname
            // 
            lbl_lastname.AutoSize = true;
            lbl_lastname.Location = new Point(201, 147);
            lbl_lastname.Name = "lbl_lastname";
            lbl_lastname.Size = new Size(99, 20);
            lbl_lastname.TabIndex = 12;
            lbl_lastname.Text = "Último Nome";
            // 
            // lbl_password
            // 
            lbl_password.AutoSize = true;
            lbl_password.Location = new Point(14, 221);
            lbl_password.Name = "lbl_password";
            lbl_password.Size = new Size(70, 20);
            lbl_password.TabIndex = 13;
            lbl_password.Text = "Password";
            // 
            // lbl_confirm_password
            // 
            lbl_confirm_password.AutoSize = true;
            lbl_confirm_password.Location = new Point(14, 288);
            lbl_confirm_password.Name = "lbl_confirm_password";
            lbl_confirm_password.Size = new Size(140, 20);
            lbl_confirm_password.TabIndex = 14;
            lbl_confirm_password.Text = "Confirmar Password";
            // 
            // RegisterForm
            // 
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(362, 419);
            Controls.Add(lbl_confirm_password);
            Controls.Add(lbl_password);
            Controls.Add(lbl_lastname);
            Controls.Add(lbl_firstname);
            Controls.Add(_labelUsername);
            Controls.Add(lbl_register);
            Controls.Add(txtbx_confirm_password);
            Controls.Add(txtbx_password);
            Controls.Add(txtbx_lastname);
            Controls.Add(txtbx_firstname);
            Controls.Add(_textBoxUsername);
            Controls.Add(btn_see_password);
            Controls.Add(btn_see_confirm_password);
            Controls.Add(lnklbl_go_login);
            Controls.Add(_buttonRegister);
            Margin = new Padding(3, 4, 3, 4);
            Name = "RegisterForm";
            Text = "RegisterForm";
            FormClosed += RegisterForm_FormClosed;
            Load += RegisterForm_Load;
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Button _buttonRegister;
        private LinkLabel lnklbl_go_login;
        private Button btn_see_confirm_password;
        private Button btn_see_password;
        private TextBox _textBoxUsername;
        private TextBox txtbx_firstname;
        private TextBox txtbx_lastname;
        private TextBox txtbx_password;
        private TextBox txtbx_confirm_password;
        private Label lbl_register;
        private Label _labelUsername;
        private Label lbl_firstname;
        private Label lbl_lastname;
        private Label lbl_password;
        private Label lbl_confirm_password;
    }
}