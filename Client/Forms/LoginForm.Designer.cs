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
            txtbx_username = new TextBox();
            txtbx_password = new TextBox();
            btn_login = new Button();
            btn_see_password = new Button();
            lbl_login = new Label();
            lnklbl_go_register = new LinkLabel();
            lbl_username = new Label();
            lbl_password = new Label();
            SuspendLayout();
            // 
            // txtbx_username
            // 
            txtbx_username.Location = new Point(14, 83);
            txtbx_username.Margin = new Padding(3, 4, 3, 4);
            txtbx_username.Name = "txtbx_username";
            txtbx_username.Size = new Size(332, 27);
            txtbx_username.TabIndex = 0;
            // 
            // txtbx_password
            // 
            txtbx_password.Location = new Point(14, 157);
            txtbx_password.Margin = new Padding(3, 4, 3, 4);
            txtbx_password.Name = "txtbx_password";
            txtbx_password.Size = new Size(287, 27);
            txtbx_password.TabIndex = 1;
            txtbx_password.UseSystemPasswordChar = true;
            // 
            // btn_login
            // 
            btn_login.Location = new Point(221, 196);
            btn_login.Margin = new Padding(3, 4, 3, 4);
            btn_login.Name = "btn_login";
            btn_login.Size = new Size(126, 31);
            btn_login.TabIndex = 2;
            btn_login.Text = "Log In";
            btn_login.UseVisualStyleBackColor = true;
            btn_login.Click += btn_login_Click;
            // 
            // btn_see_password
            // 
            btn_see_password.Location = new Point(309, 157);
            btn_see_password.Margin = new Padding(3, 4, 3, 4);
            btn_see_password.Name = "btn_see_password";
            btn_see_password.Size = new Size(38, 31);
            btn_see_password.TabIndex = 4;
            btn_see_password.Text = "Ver";
            btn_see_password.UseVisualStyleBackColor = true;
            // 
            // lbl_login
            // 
            lbl_login.AutoSize = true;
            lbl_login.Font = new Font("Segoe UI", 20F);
            lbl_login.Location = new Point(135, 12);
            lbl_login.Name = "lbl_login";
            lbl_login.Size = new Size(103, 46);
            lbl_login.TabIndex = 5;
            lbl_login.Text = "Login";
            // 
            // lnklbl_go_register
            // 
            lnklbl_go_register.AutoSize = true;
            lnklbl_go_register.Location = new Point(14, 201);
            lnklbl_go_register.Name = "lnklbl_go_register";
            lnklbl_go_register.Size = new Size(120, 20);
            lnklbl_go_register.TabIndex = 6;
            lnklbl_go_register.TabStop = true;
            lnklbl_go_register.Text = "Não tenho conta";
            lnklbl_go_register.LinkClicked += lnklbl_go_register_LinkClicked;
            // 
            // lbl_username
            // 
            lbl_username.AutoSize = true;
            lbl_username.Location = new Point(14, 59);
            lbl_username.Name = "lbl_username";
            lbl_username.Size = new Size(78, 20);
            lbl_username.TabIndex = 7;
            lbl_username.Text = "UserName";
            // 
            // lbl_password
            // 
            lbl_password.AutoSize = true;
            lbl_password.Location = new Point(14, 133);
            lbl_password.Name = "lbl_password";
            lbl_password.Size = new Size(70, 20);
            lbl_password.TabIndex = 8;
            lbl_password.Text = "Password";
            // 
            // LoginForm
            // 
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(360, 243);
            Controls.Add(lbl_password);
            Controls.Add(lbl_username);
            Controls.Add(lnklbl_go_register);
            Controls.Add(lbl_login);
            Controls.Add(btn_see_password);
            Controls.Add(btn_login);
            Controls.Add(txtbx_password);
            Controls.Add(txtbx_username);
            Margin = new Padding(3, 4, 3, 4);
            Name = "LoginForm";
            Text = "LoginForm";
            Load += LoginForm_Load;
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private TextBox txtbx_username;
        private TextBox txtbx_password;
        private Button btn_login;
        private Button btn_see_password;
        private Label lbl_login;
        private LinkLabel lnklbl_go_register;
        private Label lbl_username;
        private Label lbl_password;
    }
}