namespace muzikaaa
{
    partial class form_cestoni
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
            this.lw_cestoni = new System.Windows.Forms.ListView();
            this.btn_remove = new System.Windows.Forms.Button();
            this.btn_add = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // lw_cestoni
            // 
            this.lw_cestoni.Activation = System.Windows.Forms.ItemActivation.OneClick;
            this.lw_cestoni.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.lw_cestoni.FullRowSelect = true;
            this.lw_cestoni.HideSelection = false;
            this.lw_cestoni.HoverSelection = true;
            this.lw_cestoni.Location = new System.Drawing.Point(1, 1);
            this.lw_cestoni.Name = "lw_cestoni";
            this.lw_cestoni.Size = new System.Drawing.Size(850, 290);
            this.lw_cestoni.TabIndex = 1;
            this.lw_cestoni.UseCompatibleStateImageBehavior = false;
            this.lw_cestoni.View = System.Windows.Forms.View.Details;
            this.lw_cestoni.SelectedIndexChanged += new System.EventHandler(this.lw_cestoni_SelectedIndexChanged);
            this.lw_cestoni.DoubleClick += new System.EventHandler(this.lw_cestoni_DoubleClick);
            // 
            // btn_remove
            // 
            this.btn_remove.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btn_remove.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btn_remove.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btn_remove.Location = new System.Drawing.Point(750, 297);
            this.btn_remove.Name = "btn_remove";
            this.btn_remove.Size = new System.Drawing.Size(98, 23);
            this.btn_remove.TabIndex = 2;
            this.btn_remove.Text = "remove";
            this.btn_remove.UseVisualStyleBackColor = true;
            this.btn_remove.Click += new System.EventHandler(this.btn_remove_Click);
            // 
            // btn_add
            // 
            this.btn_add.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btn_add.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btn_add.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btn_add.Location = new System.Drawing.Point(654, 297);
            this.btn_add.Name = "btn_add";
            this.btn_add.Size = new System.Drawing.Size(92, 23);
            this.btn_add.TabIndex = 3;
            this.btn_add.Text = "add";
            this.btn_add.UseVisualStyleBackColor = true;
            this.btn_add.Click += new System.EventHandler(this.btn_add_Click);
            // 
            // form_cestoni
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 14F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.White;
            this.ClientSize = new System.Drawing.Size(853, 324);
            this.Controls.Add(this.btn_add);
            this.Controls.Add(this.btn_remove);
            this.Controls.Add(this.lw_cestoni);
            this.Font = new System.Drawing.Font("Courier New", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.SizableToolWindow;
            this.Name = "form_cestoni";
            this.Text = "cestoni";
            this.Load += new System.EventHandler(this.cestoni_Load);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.ListView lw_cestoni;
        private System.Windows.Forms.Button btn_remove;
        private System.Windows.Forms.Button btn_add;
    }
}