using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using Autodesk.Revit.DB;

namespace Solutia.Forms
{
    public partial class CategoryFamilyExportForm : System.Windows.Forms.Form
    {
        private Document _doc;

        public List<BuiltInCategory> SelectedCategories { get; private set; }
        public string SelectedFolderPath { get; private set; }

        public CategoryFamilyExportForm(Document doc)
        {
            InitializeComponent();
            _doc = doc;

            // Preencher a lista de categorias disponíveis
            PopulateCategories();
            clbCategories.MouseClick += CheckedListBoxCategories_MouseClick;
            ButtonSelectAll.Click += ButtonSelectAll_Click;
            ButtonClearSelection.Click += ButtonClearSelection_Click;

        }

        private void PopulateCategories()
        {
            // Obtém todas as categorias presentes no projeto
            List<Category> categories = _doc.Settings.Categories
                .Cast<Category>()
                .Where(c => c.CategoryType == CategoryType.Model && c.AllowsBoundParameters)
                .OrderBy(c => c.Name)
                .ToList();

            foreach (var category in categories)
            {
                clbCategories.Items.Add(category.Name, false);
            }
        }

        private void ButtonSelectFolder_Click(object sender, EventArgs e)
        {
            using (FolderBrowserDialog folderDialog = new FolderBrowserDialog())
            {
                folderDialog.Description = "Selecione a pasta para salvar as famílias";
                folderDialog.ShowNewFolderButton = true;

                if (folderDialog.ShowDialog() == DialogResult.OK)
                {
                    TextBoxFolderPath.Text = folderDialog.SelectedPath;
                }
            }
        }

        private void ButtonOk_Click(object sender, EventArgs e)
        {
            // Validações
            if (string.IsNullOrWhiteSpace(TextBoxFolderPath.Text) || !System.IO.Directory.Exists(TextBoxFolderPath.Text))
            {
                MessageBox.Show("Por favor, selecione um caminho válido para a pasta.", "Erro", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (clbCategories.CheckedItems.Count == 0)
            {
                MessageBox.Show("Por favor, selecione ao menos uma categoria.", "Erro", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            // Obtém os BuiltInCategories selecionados
            SelectedCategories = new List<BuiltInCategory>();
            foreach (string categoryName in clbCategories.CheckedItems)
            {
                Category category = _doc.Settings.Categories
                    .Cast<Category>()
                    .FirstOrDefault(c => c.Name == categoryName);

                if (category != null)
                {
                    SelectedCategories.Add((BuiltInCategory)category.Id.Value);
                }
            }

            SelectedFolderPath = TextBoxFolderPath.Text;
            DialogResult = DialogResult.OK;
            Close();
        }

        private void ButtonCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }

        private void CheckedListBoxCategories_MouseClick(object sender, MouseEventArgs e)
        {
            int index = clbCategories.IndexFromPoint(e.Location);
            if (index != ListBox.NoMatches)
            {
                clbCategories.SetItemChecked(index, !clbCategories.GetItemChecked(index));
            }
        }

        private void ButtonSelectAll_Click(object sender, EventArgs e)
        {
            // Marca todos os itens como selecionados
            for (int i = 0; i < clbCategories.Items.Count; i++)
            {
                clbCategories.SetItemChecked(i, true);
            }
        }

        private void ButtonClearSelection_Click(object sender, EventArgs e)
        {
            // Desmarca todos os itens
            for (int i = 0; i < clbCategories.Items.Count; i++)
            {
                clbCategories.SetItemChecked(i, false);
            }
        }


    }
}
