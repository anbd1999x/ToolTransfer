namespace ToolTransfer
{
    partial class Form1
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
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
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            btnTransfer = new Button();
            treeViewDatabase = new TreeView();
            txtConnectString = new TextBox();
            label1 = new Label();
            Checkconnect = new Button();
            groupBox1 = new GroupBox();
            groupBox2 = new GroupBox();
            TestConenctAws3 = new Button();
            label6 = new Label();
            label5 = new Label();
            label3 = new Label();
            label4 = new Label();
            label2 = new Label();
            txtFolder = new TextBox();
            txtBucket = new TextBox();
            TxtAccessKey = new TextBox();
            TxtSecretKey = new TextBox();
            txtAws3Url = new TextBox();
            richTextBox1 = new RichTextBox();
            groupBox1.SuspendLayout();
            groupBox2.SuspendLayout();
            SuspendLayout();
            // 
            // btnTransfer
            // 
            btnTransfer.Location = new Point(374, 328);
            btnTransfer.Name = "btnTransfer";
            btnTransfer.Size = new Size(60, 23);
            btnTransfer.TabIndex = 0;
            btnTransfer.Text = ">>>";
            btnTransfer.UseVisualStyleBackColor = true;
            btnTransfer.Click += TransferData;
            // 
            // treeViewDatabase
            // 
            treeViewDatabase.CheckBoxes = true;
            treeViewDatabase.Location = new Point(24, 163);
            treeViewDatabase.Name = "treeViewDatabase";
            treeViewDatabase.Size = new Size(344, 390);
            treeViewDatabase.TabIndex = 1;
            treeViewDatabase.AfterCheck += treeViewDatabase_AfterCheck;
            treeViewDatabase.AfterSelect += treeViewDatabase_AfterSelect;
            // 
            // txtConnectString
            // 
            txtConnectString.Location = new Point(126, 16);
            txtConnectString.Name = "txtConnectString";
            txtConnectString.PlaceholderText = "ConnectString";
            txtConnectString.Size = new Size(170, 23);
            txtConnectString.TabIndex = 2;
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new Point(6, 19);
            label1.Name = "label1";
            label1.Size = new Size(105, 15);
            label1.TabIndex = 3;
            label1.Text = "ConnectionStrings";
            // 
            // Checkconnect
            // 
            Checkconnect.Location = new Point(126, 49);
            Checkconnect.Name = "Checkconnect";
            Checkconnect.Size = new Size(107, 23);
            Checkconnect.TabIndex = 4;
            Checkconnect.Text = "Test Connect";
            Checkconnect.UseVisualStyleBackColor = true;
            Checkconnect.Click += Checkconnect_Click;
            // 
            // groupBox1
            // 
            groupBox1.Controls.Add(label1);
            groupBox1.Controls.Add(Checkconnect);
            groupBox1.Controls.Add(txtConnectString);
            groupBox1.Location = new Point(33, 22);
            groupBox1.Name = "groupBox1";
            groupBox1.Size = new Size(344, 123);
            groupBox1.TabIndex = 5;
            groupBox1.TabStop = false;
            groupBox1.Text = "Database";
            groupBox1.UseCompatibleTextRendering = true;
            // 
            // groupBox2
            // 
            groupBox2.Controls.Add(TestConenctAws3);
            groupBox2.Controls.Add(label6);
            groupBox2.Controls.Add(label5);
            groupBox2.Controls.Add(label3);
            groupBox2.Controls.Add(label4);
            groupBox2.Controls.Add(label2);
            groupBox2.Controls.Add(txtFolder);
            groupBox2.Controls.Add(txtBucket);
            groupBox2.Controls.Add(TxtAccessKey);
            groupBox2.Controls.Add(TxtSecretKey);
            groupBox2.Controls.Add(txtAws3Url);
            groupBox2.Location = new Point(431, 22);
            groupBox2.Name = "groupBox2";
            groupBox2.Size = new Size(362, 123);
            groupBox2.TabIndex = 6;
            groupBox2.TabStop = false;
            groupBox2.Text = "Aws3";
            // 
            // TestConenctAws3
            // 
            TestConenctAws3.Location = new Point(243, 86);
            TestConenctAws3.Name = "TestConenctAws3";
            TestConenctAws3.Size = new Size(100, 23);
            TestConenctAws3.TabIndex = 2;
            TestConenctAws3.Text = "Test Connenct";
            TestConenctAws3.UseVisualStyleBackColor = true;
            TestConenctAws3.Click += TestConenctAws3_Click;
            // 
            // label6
            // 
            label6.AutoSize = true;
            label6.Location = new Point(180, 57);
            label6.Name = "label6";
            label6.Size = new Size(40, 15);
            label6.TabIndex = 1;
            label6.Text = "Folder";
            // 
            // label5
            // 
            label5.AutoSize = true;
            label5.Location = new Point(6, 86);
            label5.Name = "label5";
            label5.Size = new Size(43, 15);
            label5.TabIndex = 1;
            label5.Text = "Bucket";
            // 
            // label3
            // 
            label3.AutoSize = true;
            label3.Location = new Point(6, 57);
            label3.Name = "label3";
            label3.Size = new Size(62, 15);
            label3.TabIndex = 1;
            label3.Text = "AccessKey";
            // 
            // label4
            // 
            label4.AutoSize = true;
            label4.Location = new Point(6, 22);
            label4.Name = "label4";
            label4.Size = new Size(22, 15);
            label4.TabIndex = 1;
            label4.Text = "Url";
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Location = new Point(180, 19);
            label2.Name = "label2";
            label2.Size = new Size(57, 15);
            label2.TabIndex = 1;
            label2.Text = "secretKey";
            // 
            // txtFolder
            // 
            txtFolder.Location = new Point(243, 54);
            txtFolder.Name = "txtFolder";
            txtFolder.Size = new Size(100, 23);
            txtFolder.TabIndex = 0;
            // 
            // txtBucket
            // 
            txtBucket.Location = new Point(74, 83);
            txtBucket.Name = "txtBucket";
            txtBucket.Size = new Size(100, 23);
            txtBucket.TabIndex = 0;
            // 
            // TxtAccessKey
            // 
            TxtAccessKey.Location = new Point(74, 54);
            TxtAccessKey.Name = "TxtAccessKey";
            TxtAccessKey.Size = new Size(100, 23);
            TxtAccessKey.TabIndex = 0;
            // 
            // TxtSecretKey
            // 
            TxtSecretKey.Location = new Point(243, 16);
            TxtSecretKey.Name = "TxtSecretKey";
            TxtSecretKey.Size = new Size(100, 23);
            TxtSecretKey.TabIndex = 0;
            // 
            // txtAws3Url
            // 
            txtAws3Url.Location = new Point(74, 16);
            txtAws3Url.Name = "txtAws3Url";
            txtAws3Url.Size = new Size(100, 23);
            txtAws3Url.TabIndex = 0;
            // 
            // richTextBox1
            // 
            richTextBox1.Location = new Point(454, 163);
            richTextBox1.Name = "richTextBox1";
            richTextBox1.Size = new Size(342, 390);
            richTextBox1.TabIndex = 7;
            richTextBox1.Text = "";
            // 
            // Form1
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(808, 571);
            Controls.Add(richTextBox1);
            Controls.Add(groupBox2);
            Controls.Add(groupBox1);
            Controls.Add(treeViewDatabase);
            Controls.Add(btnTransfer);
            Name = "Form1";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "Transfer IIS to Aws3";
            groupBox1.ResumeLayout(false);
            groupBox1.PerformLayout();
            groupBox2.ResumeLayout(false);
            groupBox2.PerformLayout();
            ResumeLayout(false);
        }

        #endregion

        private Button btnTransfer;
        private TreeView treeViewDatabase;
        private TextBox txtConnectString;
        private Label label1;
        private Button Checkconnect;
        private GroupBox groupBox1;
        private GroupBox groupBox2;
        private TextBox TxtAccessKey;
        private TextBox txtAws3Url;
        private Button TestConenctAws3;
        private Label label3;
        private Label label4;
        private Label label2;
        private TextBox TxtSecretKey;
        private RichTextBox richTextBox1;
        private Label label6;
        private Label label5;
        private TextBox txtFolder;
        private TextBox txtBucket;
    }
}
