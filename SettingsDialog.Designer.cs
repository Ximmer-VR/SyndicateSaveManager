namespace SyndicateSaveManager
{
    partial class SettingsDialog
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
            label1 = new Label();
            textBoxLogFilePath = new TextBox();
            buttonOk = new Button();
            buttonCancel = new Button();
            buttonBrowse = new Button();
            SuspendLayout();
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new Point(13, 13);
            label1.Name = "label1";
            label1.Size = new Size(120, 15);
            label1.TabIndex = 0;
            label1.Text = "VRChat Log File Path:";
            // 
            // textBoxLogFilePath
            // 
            textBoxLogFilePath.Location = new Point(16, 30);
            textBoxLogFilePath.Name = "textBoxLogFilePath";
            textBoxLogFilePath.Size = new Size(452, 23);
            textBoxLogFilePath.TabIndex = 1;
            // 
            // buttonOk
            // 
            buttonOk.Location = new Point(349, 59);
            buttonOk.Name = "buttonOk";
            buttonOk.Size = new Size(75, 23);
            buttonOk.TabIndex = 2;
            buttonOk.Text = "OK";
            buttonOk.UseVisualStyleBackColor = true;
            buttonOk.Click += buttonOk_Click;
            // 
            // buttonCancel
            // 
            buttonCancel.Location = new Point(430, 59);
            buttonCancel.Name = "buttonCancel";
            buttonCancel.Size = new Size(75, 23);
            buttonCancel.TabIndex = 3;
            buttonCancel.Text = "Cancel";
            buttonCancel.UseVisualStyleBackColor = true;
            buttonCancel.Click += buttonCancel_Click;
            // 
            // buttonBrowse
            // 
            buttonBrowse.Location = new Point(474, 29);
            buttonBrowse.Name = "buttonBrowse";
            buttonBrowse.Size = new Size(31, 23);
            buttonBrowse.TabIndex = 4;
            buttonBrowse.Text = "...";
            buttonBrowse.UseVisualStyleBackColor = true;
            buttonBrowse.Click += buttonBrowse_Click;
            // 
            // SettingsDialog
            // 
            ClientSize = new Size(517, 94);
            Controls.Add(buttonBrowse);
            Controls.Add(buttonCancel);
            Controls.Add(buttonOk);
            Controls.Add(textBoxLogFilePath);
            Controls.Add(label1);
            Name = "SettingsDialog";
            Text = "Settings";
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Label label1;
        private TextBox textBoxLogFilePath;
        private Button buttonOk;
        private Button buttonCancel;
        private Button buttonBrowse;
    }
}