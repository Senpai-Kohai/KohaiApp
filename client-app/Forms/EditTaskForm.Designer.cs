namespace client_app
{
    partial class EditTaskForm
    {
        private System.ComponentModel.IContainer components = null;
        private System.Windows.Forms.TextBox taskDescriptionTextBox;
        private System.Windows.Forms.Button okButton;
        private System.Windows.Forms.Button cancelButton;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            taskDescriptionTextBox = new TextBox();
            okButton = new Button();
            cancelButton = new Button();
            completedCheckbox = new CheckBox();
            SuspendLayout();
            // 
            // taskDescriptionTextBox
            // 
            taskDescriptionTextBox.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            taskDescriptionTextBox.Location = new Point(10, 11);
            taskDescriptionTextBox.Multiline = true;
            taskDescriptionTextBox.Name = "taskDescriptionTextBox";
            taskDescriptionTextBox.Size = new Size(316, 71);
            taskDescriptionTextBox.TabIndex = 0;
            // 
            // okButton
            // 
            okButton.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            okButton.Location = new Point(188, 113);
            okButton.Name = "okButton";
            okButton.Size = new Size(66, 22);
            okButton.TabIndex = 1;
            okButton.Text = "OK";
            okButton.UseVisualStyleBackColor = true;
            okButton.Click += TaskWindow_okButton_Click;
            // 
            // cancelButton
            // 
            cancelButton.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            cancelButton.Location = new Point(260, 113);
            cancelButton.Name = "cancelButton";
            cancelButton.Size = new Size(66, 22);
            cancelButton.TabIndex = 2;
            cancelButton.Text = "Cancel";
            cancelButton.UseVisualStyleBackColor = true;
            cancelButton.Click += TaskWindow_cancelButton_Click;
            // 
            // completedCheckbox
            // 
            completedCheckbox.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            completedCheckbox.AutoSize = true;
            completedCheckbox.Location = new Point(239, 88);
            completedCheckbox.Name = "completedCheckbox";
            completedCheckbox.Size = new Size(85, 19);
            completedCheckbox.TabIndex = 3;
            completedCheckbox.Text = "Completed";
            completedCheckbox.UseVisualStyleBackColor = true;
            completedCheckbox.CheckedChanged += TaskWindow_completedCheckbox_Updated;
            // 
            // EditTaskForm
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(336, 149);
            Controls.Add(completedCheckbox);
            Controls.Add(cancelButton);
            Controls.Add(okButton);
            Controls.Add(taskDescriptionTextBox);
            Name = "EditTaskForm";
            Text = "Edit Task";
            ResumeLayout(false);
            PerformLayout();
        }

        private CheckBox completedCheckbox;
    }
}
