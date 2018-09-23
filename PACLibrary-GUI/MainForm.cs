using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Microsoft.WindowsAPICodePack.Dialogs;
using PACLibrary;
using PACLibrary_GUI.Misc;
using Reloaded_GUI.Styles.Themes;
using Reloaded_GUI.Utilities.Windows;

namespace PACLibrary_GUI
{
    public partial class MainForm : Form
    {
        /*
            ------------------------
            Default Instance Members
            ------------------------
        */
        public PACArchive Archive = new PACArchive();
        private Theme _reloadedDefaultTheme;
        private string _lastOpenFile;

        /*
            ------------------
            Class Construction
            ------------------
        */

        public MainForm(string openFile = "")
        {
            InitializeComponent();

            if (openFile != "")
            {
                try
                {
                    OpenFile(openFile);
                }
                catch { }
            }
        }

        /* Themes the current Windows Form.*/
        private void MainForm_Load(object sender, EventArgs e)
        {
            // Theme this form.
            _reloadedDefaultTheme = new Theme();
            Reloaded_GUI.Bindings.WindowsForms.Add(this);
            _reloadedDefaultTheme.LoadCurrentTheme();

            // Custom render settings.
            MakeRoundedWindow.RoundWindow(this, 30, 30);
            categoryBar_MenuStrip.Renderer = new MyRenderer();
        }

        /*
            ------------------
            Core Functionality
            ------------------
        */

        /// <summary>
        /// Updates all corresponding GUI elements (e.g. file count, file list) to reflect
        /// current state.
        /// </summary>
        private void UpdateGUI()
        {
            titleBar_ItemCount.Text = $"Files: {Archive.Files.Count}";

            // Backup row if possible.
            bool hasSelectedRow = false;
            int lastSelectedRow = 0;
            int firstDisplayedIndex = box_SoundList.FirstDisplayedScrollingRowIndex;
            if (box_SoundList.SelectedCells.Count > 0)
            {
                hasSelectedRow  = true;
                lastSelectedRow = box_SoundList.SelectedCells[0].RowIndex;
            }

            // Fill new rows.
            box_SoundList.Rows.Clear();
            foreach (var file in Archive.Files)
            {
                box_SoundList.Rows.Add($"{file.GetFileName()}", $"{file.Sound.SoundData.Length}");
            }

            // Restore row if possible.
            try
            {
                if (hasSelectedRow)
                {
                    box_SoundList.Rows[lastSelectedRow].Selected = true;
                    box_SoundList.FirstDisplayedScrollingRowIndex = firstDisplayedIndex;
                }
            }
            catch { }
        }

        private void OpenFile(string filePath)
        {
            // Reset archive, load new archive in.
            Archive = new PACArchive(filePath);
            _lastOpenFile = filePath;

            // Update GUI.
            this.titleBar_Title.Text = Path.GetFileName(filePath);
            UpdateGUI();
        }

        /*
            ------------------
            GUI Control Events
            ------------------
        */

        private void Close_Click(object sender, EventArgs e)
        {
            Environment.Exit(0);
        }

        private void NewItem_Click(object sender, EventArgs e)
        {
            Archive = new PACArchive();
            UpdateGUI();
        }

        private void OpenItem_Click(object sender, EventArgs e)
        {
            // Pick PAC file.
            CommonOpenFileDialog fileDialog = new CommonOpenFileDialog
            {
                Title           = "Select the individual .PAC file to open.",
                Multiselect     = false,
                IsFolderPicker  = false,
            };

            fileDialog.Filters.Add(new CommonFileDialogFilter("Sonic Heroes PAC Archive", ".pac"));

            if (fileDialog.ShowDialog() == CommonFileDialogResult.Ok)
            {
                OpenFile(fileDialog.FileName);
            }
        }


        private void SaveItem_Click(object sender, EventArgs e)
        {
            // Pick PAC file.
            CommonSaveFileDialog fileDialog = new CommonSaveFileDialog()
            {
                Title = "Save .PAC File",
                InitialDirectory = Path.GetDirectoryName(_lastOpenFile),
                DefaultFileName = Path.GetFileName(_lastOpenFile)
            };

            fileDialog.Filters.Add(new CommonFileDialogFilter("Sonic Heroes PAC Archive", ".pac"));

            if (fileDialog.ShowDialog() == CommonFileDialogResult.Ok)
            {
                Archive.SaveToFile(fileDialog.FileName);
            }
        }

        private void ExtractAll_Click(object sender, EventArgs e)
        {
            // Pick PAC file.
            CommonOpenFileDialog fileDialog = new CommonOpenFileDialog
            {
                Title           = "Select the folder to extract sounds to.",
                Multiselect     = false,
                IsFolderPicker  = true,
            };

            if (fileDialog.ShowDialog() == CommonFileDialogResult.Ok)
            {
                // Save all files to output folder.
                string outputFolder = fileDialog.FileName;

                foreach (var file in Archive.Files)
                    file.SaveFile(outputFolder);
            }
        }

        private void AddFiles_Click(object sender, EventArgs e)
        {
            // Pick PAC file.
            CommonOpenFileDialog fileDialog = new CommonOpenFileDialog
            {
                Title           = "Select the individual WAV files to add.",
                Multiselect     = true,
                IsFolderPicker  = false,
            };

            fileDialog.Filters.Add(new CommonFileDialogFilter("WAVE Audio File", ".wav"));

            if (fileDialog.ShowDialog() == CommonFileDialogResult.Ok)
            {
                // Save all files to output folder.
                string[] fileNames = fileDialog.FileNames.ToArray();

                foreach (var file in fileNames)
                    Archive.Files.Add(new PACFile(file));

                UpdateGUI();

                if (box_SoundList.RowCount > 0)
                    box_SoundList.FirstDisplayedScrollingRowIndex = box_SoundList.RowCount - 1;
            }
        }

        /// <summary>
        /// Loads details of the new selection into the fields and updates the currently selected file counter.
        /// </summary>
        private void SoundList_SelectionChanged(object sender, EventArgs e)
        {
            if (box_SoundList.SelectedCells.Count > 1)
            {
                int rowIndex = box_SoundList.SelectedCells[0].RowIndex;
                titleBar_CurrentItem.Text = $"Selected File: {rowIndex}/{Archive.Files.Count}";

                var currentFile = Archive.Files[rowIndex];
                borderless_BankIndexValue.Text = $"{currentFile.File.BankIndex}";
                borderless_FileFlagAValue.Text = $"{currentFile.File.FileFlagA}";
                borderless_FileFlagBValue.Text = $"{currentFile.File.FileFlagB}";
                borderless_FileIndexValue.Text = $"{currentFile.File.FileIndex}";
                borderless_FileSizeValue.Text = $"{currentFile.File.SizeOfFile}";
                borderless_SomeCountValue.Text = $"{currentFile.File.SomeCount}";
            }
        }

        private PACFile GetCurrentFile()
        {
            if (box_SoundList.SelectedCells.Count > 0)
            {
                int rowIndex = box_SoundList.SelectedCells[0].RowIndex;
                return Archive.Files[rowIndex];
            }

            return null;
        }

        private void BankIndexValue_TextChanged(object sender, EventArgs e)
        {
            var file = GetCurrentFile();
            if (file != null)
            {
                try   { file.File.BankIndex = Convert.ToByte(borderless_BankIndexValue.Text); }
                catch { }
            }
        }

        private void FileIndexValue_TextChanged(object sender, EventArgs e)
        {
            var file = GetCurrentFile();
            if (file != null)
            {
                try { file.File.FileIndex = Convert.ToByte(borderless_FileIndexValue.Text); }
                catch { }
            }
        }

        private void FileFlagAValue_TextChanged(object sender, EventArgs e)
        {
            var file = GetCurrentFile();
            if (file != null)
            {
                try { file.File.FileFlagA = Convert.ToByte(borderless_FileFlagAValue.Text); }
                catch { }
            }
        }

        private void FileFlagBValue_TextChanged(object sender, EventArgs e)
        {
            var file = GetCurrentFile();
            if (file != null)
            {
                try   { file.File.FileFlagB = Convert.ToByte(borderless_FileFlagBValue.Text); }
                catch { }
            }
        }

        private void SomeCountValue_TextChanged(object sender, EventArgs e)
        {
            var file = GetCurrentFile();
            if (file != null)
            {
                try { file.File.SomeCount = Convert.ToInt32(borderless_SomeCount.Text); }
                catch { }
            }
        }

        private void FileSizeValue_TextChanged(object sender, EventArgs e)
        {
            var file = GetCurrentFile();
            if (file != null)
            {
                try   { file.File.SizeOfFile = Convert.ToInt32(borderless_FileSizeValue.Text); }
                catch { }
            }
        }

        private void BankIndexValue_Leave(object sender, EventArgs e)
        {
            UpdateGUI();
        }

        private void FileIndexValue_Leave(object sender, EventArgs e)
        {
            UpdateGUI();
        }

        private void box_SoundList_CellMouseClick(object sender, DataGridViewCellMouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                // Get the mouse coordinates and show options.
                var coordinates = box_SoundList.PointToClient(Cursor.Position);
                box_MenuStrip.Show(box_SoundList, coordinates.X, coordinates.Y);

                // Select row.
                box_SoundList.Rows[e.RowIndex].Selected = true;
            }
        }

        /*
            ---------------------
            Right Click MenuStrip
            ---------------------
        */

        private void ExtractToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // Pick PAC file.
            CommonOpenFileDialog fileDialog = new CommonOpenFileDialog
            {
                Title = "Select the folder to extract sound to.",
                Multiselect = false,
                IsFolderPicker = true,
            };

            if (fileDialog.ShowDialog() == CommonFileDialogResult.Ok)
            {
                Archive.Files[box_SoundList.SelectedCells[0].RowIndex].SaveFile(fileDialog.FileName);
            }
        }

        private void ReplaceToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // Pick PAC file.
            CommonOpenFileDialog fileDialog = new CommonOpenFileDialog
            {
                Title = "Select the individual WAV files to add.",
                Multiselect = false,
                IsFolderPicker = false,
            };

            fileDialog.Filters.Add(new CommonFileDialogFilter("WAVE Audio File", ".wav"));

            if (fileDialog.ShowDialog() == CommonFileDialogResult.Ok)
            {
                // Save all files to output folder.
                string fileName = fileDialog.FileName;
                Archive.Files[box_SoundList.SelectedCells[0].RowIndex].ReplaceFromFile(fileName);
                UpdateGUI();
            }
        }

        private void DeleteToolStripMenuItem_Click(object sender, EventArgs e)
        {
            int rowIndex = box_SoundList.SelectedCells[0].RowIndex;
            Archive.Files.RemoveAt(rowIndex);
            UpdateGUI();
        }
    }
}
