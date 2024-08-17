using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MyLibrary
{
    public partial class Form1 : Form
    {
        public string NewName { get; private set; }
        public Form1()
        {
            InitializeComponent();
            NewName = string.Empty;
        }

        private void button_ok_Click(object sender, EventArgs e)
        {
            // Проверяем, что текстбокс не пустой
            if (!string.IsNullOrWhiteSpace(textBox1.Text))
            {
                NewName = textBox1.Text; // Сохраняем новое имя
                this.DialogResult = DialogResult.OK; // Устанавливаем результат диалога как OK
                this.Close(); // Закрываем форму
            }
            else
            {
                MessageBox.Show("Имя не может быть пустым.", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void button_cancel_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel; // Устанавливаем результат диалога как Cancel
            this.Close(); // Закрываем форму
        }
    }
}
