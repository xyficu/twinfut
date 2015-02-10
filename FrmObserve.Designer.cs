namespace twin_futs
{
    partial class FrmObserve
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
            this.buttonInitFuts = new System.Windows.Forms.Button();
            this.buttonStartObs = new System.Windows.Forms.Button();
            this.listView1 = new twin_futs.ListViewNF();
            this.SuspendLayout();
            // 
            // buttonInitFuts
            // 
            this.buttonInitFuts.Location = new System.Drawing.Point(615, 89);
            this.buttonInitFuts.Name = "buttonInitFuts";
            this.buttonInitFuts.Size = new System.Drawing.Size(75, 23);
            this.buttonInitFuts.TabIndex = 1;
            this.buttonInitFuts.Text = "初始化";
            this.buttonInitFuts.UseVisualStyleBackColor = true;
            this.buttonInitFuts.Click += new System.EventHandler(this.buttonInitFuts_Click);
            // 
            // buttonStartObs
            // 
            this.buttonStartObs.Location = new System.Drawing.Point(615, 196);
            this.buttonStartObs.Name = "buttonStartObs";
            this.buttonStartObs.Size = new System.Drawing.Size(75, 23);
            this.buttonStartObs.TabIndex = 2;
            this.buttonStartObs.Text = "开始观测";
            this.buttonStartObs.UseVisualStyleBackColor = true;
            this.buttonStartObs.Click += new System.EventHandler(this.buttonStartObs_Click);
            // 
            // listView1
            // 
            this.listView1.Location = new System.Drawing.Point(12, 12);
            this.listView1.Name = "listView1";
            this.listView1.Size = new System.Drawing.Size(539, 456);
            this.listView1.TabIndex = 0;
            this.listView1.UseCompatibleStateImageBehavior = false;
            this.listView1.View = System.Windows.Forms.View.Details;
            // 
            // FrmObserve
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1028, 679);
            this.Controls.Add(this.buttonStartObs);
            this.Controls.Add(this.buttonInitFuts);
            this.Controls.Add(this.listView1);
            this.Name = "FrmObserve";
            this.Text = "观测控制面板";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.observe_FormClosing);
            this.ResumeLayout(false);

        }

        #endregion

        private ListViewNF listView1;
        private System.Windows.Forms.Button buttonInitFuts;
        private System.Windows.Forms.Button buttonStartObs;
    }
}