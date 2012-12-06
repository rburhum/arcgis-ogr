// Copyright 2010 ESRI
// 
// All rights reserved under the copyright laws of the United States
// and applicable international laws, treaties, and conventions.
// 
// You may freely redistribute and use this sample code, with or
// without modification, provided you include the original copyright
// notice and use restrictions.
// 
// See the use restrictions at http://help.arcgis.com/en/sdk/10.0/usageRestrictions.htm
// 

namespace GDAL.OGRPlugin
{
    partial class OGRAddLayerDialog
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
            this.components = new System.ComponentModel.Container();
            this.btnCancel = new System.Windows.Forms.Button();
            this.btnOK = new System.Windows.Forms.Button();
            this.btnOpenDataSource = new System.Windows.Forms.Button();
            this.groupFromFile = new System.Windows.Forms.GroupBox();
            this.lblDatasets = new System.Windows.Forms.Label();
            this.txtPath = new System.Windows.Forms.TextBox();
            this.lstFeatureClasses = new System.Windows.Forms.ListBox();
            this.toolTip1 = new System.Windows.Forms.ToolTip(this.components);
            this.btnConnect = new System.Windows.Forms.Button();
            this.radioFromFile = new System.Windows.Forms.RadioButton();
            this.radioFromConnstring = new System.Windows.Forms.RadioButton();
            this.groupFromConnString = new System.Windows.Forms.GroupBox();
            this.label2 = new System.Windows.Forms.Label();
            this.txtConnString = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.groupFromFile.SuspendLayout();
            this.groupFromConnString.SuspendLayout();
            this.SuspendLayout();
            // 
            // btnCancel
            // 
            this.btnCancel.Location = new System.Drawing.Point(122, 481);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(75, 23);
            this.btnCancel.TabIndex = 0;
            this.btnCancel.Text = "Cancel";
            this.btnCancel.UseVisualStyleBackColor = true;
            this.btnCancel.Click += new System.EventHandler(this.btnCancel_Click);
            // 
            // btnOK
            // 
            this.btnOK.Location = new System.Drawing.Point(21, 481);
            this.btnOK.Name = "btnOK";
            this.btnOK.Size = new System.Drawing.Size(75, 23);
            this.btnOK.TabIndex = 1;
            this.btnOK.Text = "OK";
            this.btnOK.UseVisualStyleBackColor = true;
            this.btnOK.Click += new System.EventHandler(this.btnOK_Click);
            // 
            // btnOpenDataSource
            // 
            this.btnOpenDataSource.Location = new System.Drawing.Point(454, 28);
            this.btnOpenDataSource.Name = "btnOpenDataSource";
            this.btnOpenDataSource.Size = new System.Drawing.Size(65, 23);
            this.btnOpenDataSource.TabIndex = 2;
            this.btnOpenDataSource.Text = "Browse";
            this.toolTip1.SetToolTip(this.btnOpenDataSource, "Open File");
            this.btnOpenDataSource.UseVisualStyleBackColor = true;
            this.btnOpenDataSource.Click += new System.EventHandler(this.btnOpenDataSource_Click);
            // 
            // groupFromFile
            // 
            this.groupFromFile.Controls.Add(this.lblDatasets);
            this.groupFromFile.Controls.Add(this.txtPath);
            this.groupFromFile.Controls.Add(this.btnOpenDataSource);
            this.groupFromFile.Location = new System.Drawing.Point(21, 51);
            this.groupFromFile.Name = "groupFromFile";
            this.groupFromFile.Size = new System.Drawing.Size(538, 74);
            this.groupFromFile.TabIndex = 3;
            this.groupFromFile.TabStop = false;
            // 
            // lblDatasets
            // 
            this.lblDatasets.AutoSize = true;
            this.lblDatasets.Location = new System.Drawing.Point(3, 272);
            this.lblDatasets.Name = "lblDatasets";
            this.lblDatasets.Size = new System.Drawing.Size(52, 13);
            this.lblDatasets.TabIndex = 5;
            this.lblDatasets.Text = "Datasets:";
            // 
            // txtPath
            // 
            this.txtPath.Location = new System.Drawing.Point(15, 28);
            this.txtPath.Name = "txtPath";
            this.txtPath.ReadOnly = true;
            this.txtPath.Size = new System.Drawing.Size(433, 20);
            this.txtPath.TabIndex = 3;
            // 
            // lstFeatureClasses
            // 
            this.lstFeatureClasses.FormattingEnabled = true;
            this.lstFeatureClasses.Location = new System.Drawing.Point(12, 284);
            this.lstFeatureClasses.Name = "lstFeatureClasses";
            this.lstFeatureClasses.SelectionMode = System.Windows.Forms.SelectionMode.MultiExtended;
            this.lstFeatureClasses.Size = new System.Drawing.Size(547, 173);
            this.lstFeatureClasses.TabIndex = 4;
            this.lstFeatureClasses.DoubleClick += new System.EventHandler(this.lstDeatureClasses_DoubleClick);
            // 
            // btnConnect
            // 
            this.btnConnect.Enabled = false;
            this.btnConnect.Location = new System.Drawing.Point(454, 24);
            this.btnConnect.Name = "btnConnect";
            this.btnConnect.Size = new System.Drawing.Size(65, 27);
            this.btnConnect.TabIndex = 2;
            this.btnConnect.Text = "Connect";
            this.toolTip1.SetToolTip(this.btnConnect, "Open Simple Point data");
            this.btnConnect.UseVisualStyleBackColor = true;
            this.btnConnect.Click += new System.EventHandler(this.btnConnect_Click);
            // 
            // radioFromFile
            // 
            this.radioFromFile.AutoSize = true;
            this.radioFromFile.Checked = true;
            this.radioFromFile.Location = new System.Drawing.Point(12, 28);
            this.radioFromFile.Name = "radioFromFile";
            this.radioFromFile.Size = new System.Drawing.Size(110, 17);
            this.radioFromFile.TabIndex = 6;
            this.radioFromFile.TabStop = true;
            this.radioFromFile.Text = "Add data from file:";
            this.radioFromFile.UseVisualStyleBackColor = true;
            this.radioFromFile.CheckedChanged += new System.EventHandler(this.radioFromFile_CheckedChanged);
            // 
            // radioFromConnstring
            // 
            this.radioFromConnstring.AutoSize = true;
            this.radioFromConnstring.Location = new System.Drawing.Point(12, 158);
            this.radioFromConnstring.Name = "radioFromConnstring";
            this.radioFromConnstring.Size = new System.Drawing.Size(205, 17);
            this.radioFromConnstring.TabIndex = 7;
            this.radioFromConnstring.Text = "Add data from OGR connection string:";
            this.radioFromConnstring.UseVisualStyleBackColor = true;
            this.radioFromConnstring.CheckedChanged += new System.EventHandler(this.radioFromConnstring_CheckedChanged);
            // 
            // groupFromConnString
            // 
            this.groupFromConnString.Controls.Add(this.label2);
            this.groupFromConnString.Controls.Add(this.txtConnString);
            this.groupFromConnString.Controls.Add(this.label1);
            this.groupFromConnString.Controls.Add(this.btnConnect);
            this.groupFromConnString.Location = new System.Drawing.Point(21, 181);
            this.groupFromConnString.Name = "groupFromConnString";
            this.groupFromConnString.Size = new System.Drawing.Size(538, 85);
            this.groupFromConnString.TabIndex = 6;
            this.groupFromConnString.TabStop = false;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Enabled = false;
            this.label2.Location = new System.Drawing.Point(15, 58);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(428, 13);
            this.label2.TabIndex = 7;
            this.label2.Text = "Example:  PG:dbname=mypostgisdb user=postgres password=mypassword host=myserver";
            // 
            // txtConnString
            // 
            this.txtConnString.Location = new System.Drawing.Point(15, 28);
            this.txtConnString.Name = "txtConnString";
            this.txtConnString.Size = new System.Drawing.Size(433, 20);
            this.txtConnString.TabIndex = 6;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(3, 272);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(52, 13);
            this.label1.TabIndex = 5;
            this.label1.Text = "Datasets:";
            // 
            // OGRAddLayerDialog
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(579, 529);
            this.Controls.Add(this.groupFromConnString);
            this.Controls.Add(this.radioFromFile);
            this.Controls.Add(this.radioFromConnstring);
            this.Controls.Add(this.groupFromFile);
            this.Controls.Add(this.btnOK);
            this.Controls.Add(this.btnCancel);
            this.Controls.Add(this.lstFeatureClasses);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            this.Name = "OGRAddLayerDialog";
            this.ShowInTaskbar = false;
            this.Text = "AmigoCloud: Add OGR Layer";
            this.TopMost = true;
            this.groupFromFile.ResumeLayout(false);
            this.groupFromFile.PerformLayout();
            this.groupFromConnString.ResumeLayout(false);
            this.groupFromConnString.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button btnCancel;
        private System.Windows.Forms.Button btnOK;
        private System.Windows.Forms.Button btnOpenDataSource;
        private System.Windows.Forms.GroupBox groupFromFile;
        private System.Windows.Forms.ToolTip toolTip1;
        private System.Windows.Forms.TextBox txtPath;
        private System.Windows.Forms.ListBox lstFeatureClasses;
        private System.Windows.Forms.Label lblDatasets;
        private System.Windows.Forms.RadioButton radioFromFile;
        private System.Windows.Forms.RadioButton radioFromConnstring;
        private System.Windows.Forms.GroupBox groupFromConnString;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button btnConnect;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox txtConnString;
    }
}