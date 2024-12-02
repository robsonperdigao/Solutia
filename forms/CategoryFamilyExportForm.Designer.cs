namespace Solutia.Forms
{
    partial class CategoryFamilyExportForm
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
            this.clbCategories = new System.Windows.Forms.CheckedListBox();
            this.btnOk = new System.Windows.Forms.Button();
            this.btnCancel = new System.Windows.Forms.Button();
            this.folderBrowserDialog1 = new System.Windows.Forms.FolderBrowserDialog();
            this.TextBoxFolderPath = new System.Windows.Forms.TextBox();
            this.button1 = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.ButtonSelectAll = new System.Windows.Forms.Button();
            this.ButtonClearSelection = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // clbCategories
            // 
            this.clbCategories.FormattingEnabled = true;
            this.clbCategories.Location = new System.Drawing.Point(62, 146);
            this.clbCategories.Margin = new System.Windows.Forms.Padding(2);
            this.clbCategories.Name = "clbCategories";
            this.clbCategories.Size = new System.Drawing.Size(260, 229);
            this.clbCategories.TabIndex = 0;
            // 
            // btnOk
            // 
            this.btnOk.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.btnOk.Location = new System.Drawing.Point(89, 418);
            this.btnOk.Name = "btnOk";
            this.btnOk.Size = new System.Drawing.Size(75, 23);
            this.btnOk.TabIndex = 1;
            this.btnOk.Text = "Exportar";
            this.btnOk.UseVisualStyleBackColor = true;
            // 
            // btnCancel
            // 
            this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btnCancel.Location = new System.Drawing.Point(226, 418);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(75, 23);
            this.btnCancel.TabIndex = 2;
            this.btnCancel.Text = "Cancelar";
            this.btnCancel.UseVisualStyleBackColor = true;
            // 
            // folderBrowserDialog1
            // 
            this.folderBrowserDialog1.Description = "Selecione a pasta";
            this.folderBrowserDialog1.RootFolder = System.Environment.SpecialFolder.MyComputer;
            // 
            // TextBoxFolderPath
            // 
            this.TextBoxFolderPath.Location = new System.Drawing.Point(62, 65);
            this.TextBoxFolderPath.Name = "TextBoxFolderPath";
            this.TextBoxFolderPath.Size = new System.Drawing.Size(260, 20);
            this.TextBoxFolderPath.TabIndex = 3;
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(328, 65);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(25, 20);
            this.button1.TabIndex = 4;
            this.button1.Text = "...";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.ButtonSelectFolder_Click);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(62, 46);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(213, 13);
            this.label1.TabIndex = 5;
            this.label1.Text = "Selecione a pasta para exportar as famílias:";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(62, 122);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(189, 13);
            this.label2.TabIndex = 6;
            this.label2.Text = "Selecione a(s) categoria(s) de famílias:";
            // 
            // ButtonSelectAll
            // 
            this.ButtonSelectAll.Location = new System.Drawing.Point(89, 389);
            this.ButtonSelectAll.Name = "ButtonSelectAll";
            this.ButtonSelectAll.Size = new System.Drawing.Size(103, 23);
            this.ButtonSelectAll.TabIndex = 7;
            this.ButtonSelectAll.Text = "Selecionar todos";
            this.ButtonSelectAll.UseVisualStyleBackColor = true;
            // 
            // ButtonClearSelection
            // 
            this.ButtonClearSelection.Location = new System.Drawing.Point(198, 389);
            this.ButtonClearSelection.Name = "ButtonClearSelection";
            this.ButtonClearSelection.Size = new System.Drawing.Size(103, 23);
            this.ButtonClearSelection.TabIndex = 8;
            this.ButtonClearSelection.Text = "Limpar seleção";
            this.ButtonClearSelection.UseVisualStyleBackColor = true;
            // 
            // CategoryFamilyExportForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(397, 465);
            this.Controls.Add(this.ButtonClearSelection);
            this.Controls.Add(this.ButtonSelectAll);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.button1);
            this.Controls.Add(this.TextBoxFolderPath);
            this.Controls.Add(this.btnCancel);
            this.Controls.Add(this.btnOk);
            this.Controls.Add(this.clbCategories);
            this.Margin = new System.Windows.Forms.Padding(2);
            this.Name = "CategoryFamilyExportForm";
            this.Text = "CategoryFamilyExportForm";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.CheckedListBox clbCategories;
        private System.Windows.Forms.Button btnOk;
        private System.Windows.Forms.Button btnCancel;
        private System.Windows.Forms.FolderBrowserDialog folderBrowserDialog1;
        private System.Windows.Forms.TextBox TextBoxFolderPath;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Button ButtonSelectAll;
        private System.Windows.Forms.Button ButtonClearSelection;
    }
}