﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using x360ce.Engine;

namespace x360ce.App.Controls
{
	public partial class GamesControl : UserControl
	{
		public GamesControl()
		{
			InitializeComponent();
			GamesDataGridView.AutoGenerateColumns = false;
			if (DesignMode) return;
			InitDefaultList();
		}

		x360ce.Engine.Data.Game _CurrentGame;
		public x360ce.Engine.Data.Game CurrentGame
		{
			get { return _CurrentGame; }
			set
			{
				DisableEvents();
				_CurrentGame = value;
				var en = (value != null);
				HookMaskGroupBox.Enabled = en;
				InstalledFilesGroupBox.Enabled = en;
				var item = value ?? new x360ce.Engine.Data.Game();
				// Update XINput mask.
				var inputMask = (XInputMask)item.XInputMask;
				Xinput11CheckBox.Checked = inputMask.HasFlag(XInputMask.Xinput11);
				Xinput12CheckBox.Checked = inputMask.HasFlag(XInputMask.Xinput12);
				Xinput13CheckBox.Checked = inputMask.HasFlag(XInputMask.Xinput13);
				Xinput14CheckBox.Checked = inputMask.HasFlag(XInputMask.Xinput14);
				Xinput91CheckBox.Checked = inputMask.HasFlag(XInputMask.Xinput91);
				XInputMaskTextBox.Text = item.XInputMask.ToString("X8");
				// Update hook mask.
				var hookMask = (HookMask)item.HookMask;
				HookCOMCheckBox.Checked = hookMask.HasFlag(HookMask.COM);
				HookDICheckBox.Checked = hookMask.HasFlag(HookMask.DI);
				HookDISABLECheckBox.Checked = hookMask.HasFlag(HookMask.DISABLE);
				HookLLCheckBox.Checked = hookMask.HasFlag(HookMask.LL);
				HookNameCheckBox.Checked = hookMask.HasFlag(HookMask.NAME);
				HookPIDVIDCheckBox.Checked = hookMask.HasFlag(HookMask.PIDVID);
				HookSACheckBox.Checked = hookMask.HasFlag(HookMask.SA);
				HookSTOPCheckBox.Checked = hookMask.HasFlag(HookMask.STOP);
				HookWTCheckBox.Checked = hookMask.HasFlag(HookMask.WT);
				HookMaskTextBox.Text = item.HookMask.ToString("X8");
				// Location
				GameApplicationLocationTextBox.Text = item.FullPath;
				// Enable events.
				EnableEvents();
			}
		}

		void EnableEvents()
		{
			Xinput11CheckBox.CheckedChanged += CheckBox_Changed;
			Xinput12CheckBox.CheckedChanged += CheckBox_Changed;
			Xinput13CheckBox.CheckedChanged += CheckBox_Changed;
			Xinput14CheckBox.CheckedChanged += CheckBox_Changed;
			Xinput91CheckBox.CheckedChanged += CheckBox_Changed;
			HookCOMCheckBox.CheckedChanged += CheckBox_Changed;
			HookDICheckBox.CheckedChanged += CheckBox_Changed;
			HookDISABLECheckBox.CheckedChanged += CheckBox_Changed;
			HookLLCheckBox.CheckedChanged += CheckBox_Changed;
			HookNameCheckBox.CheckedChanged += CheckBox_Changed;
			HookPIDVIDCheckBox.CheckedChanged += CheckBox_Changed;
			HookSACheckBox.CheckedChanged += CheckBox_Changed;
			HookSTOPCheckBox.CheckedChanged += CheckBox_Changed;
			HookWTCheckBox.CheckedChanged += CheckBox_Changed;
		}

		void DisableEvents()
		{
			Xinput11CheckBox.CheckedChanged -= CheckBox_Changed;
			Xinput12CheckBox.CheckedChanged -= CheckBox_Changed;
			Xinput13CheckBox.CheckedChanged -= CheckBox_Changed;
			Xinput14CheckBox.CheckedChanged -= CheckBox_Changed;
			Xinput91CheckBox.CheckedChanged -= CheckBox_Changed;
			HookCOMCheckBox.CheckedChanged -= CheckBox_Changed;
			HookDICheckBox.CheckedChanged -= CheckBox_Changed;
			HookDISABLECheckBox.CheckedChanged -= CheckBox_Changed;
			HookLLCheckBox.CheckedChanged -= CheckBox_Changed;
			HookNameCheckBox.CheckedChanged -= CheckBox_Changed;
			HookPIDVIDCheckBox.CheckedChanged -= CheckBox_Changed;
			HookSACheckBox.CheckedChanged -= CheckBox_Changed;
			HookSTOPCheckBox.CheckedChanged -= CheckBox_Changed;
			HookWTCheckBox.CheckedChanged -= CheckBox_Changed;
		}

		void CheckBox_Changed(object sender, EventArgs e)
		{
			if (CurrentGame == null) return;
			var xm = XInputMask.None;
			if (Xinput11CheckBox.Checked) xm |= XInputMask.Xinput11;
			if (Xinput12CheckBox.Checked) xm |= XInputMask.Xinput12;
			if (Xinput13CheckBox.Checked) xm |= XInputMask.Xinput13;
			if (Xinput14CheckBox.Checked) xm |= XInputMask.Xinput14;
			if (Xinput91CheckBox.Checked) xm |= XInputMask.Xinput91;
			if (CurrentGame.XInputMask != (int)xm)
			{
				CurrentGame.XInputMask = (int)xm;
				XInputMaskTextBox.Text = CurrentGame.XInputMask.ToString("X8");
			}
			var hm = HookMask.NONE;
			if (HookCOMCheckBox.Checked) hm |= HookMask.COM;
			if (HookDICheckBox.Checked) hm |= HookMask.DI;
			if (HookDISABLECheckBox.Checked) hm |= HookMask.DISABLE;
			if (HookLLCheckBox.Checked) hm |= HookMask.LL;
			if (HookNameCheckBox.Checked) hm |= HookMask.NAME;
			if (HookPIDVIDCheckBox.Checked) hm |= HookMask.PIDVID;
			if (HookSACheckBox.Checked) hm |= HookMask.SA;
			if (HookSTOPCheckBox.Checked) hm |= HookMask.STOP;
			if (HookWTCheckBox.Checked) hm |= HookMask.WT;
			if (CurrentGame.HookMask != (int)xm)
			{
				CurrentGame.HookMask = (int)hm;
				HookMaskTextBox.Text = CurrentGame.HookMask.ToString("X8");
			}
			SettingsFile.Current.Save();
		}

		void RefreshAllButton_Click(object sender, EventArgs e)
		{
			MainForm.Current.LoadingCircle = true;
			var ws = new WebServiceClient();
			ws.Url = MainForm.Current.OptionsPanel.InternetDatabaseUrlTextBox.Text;
			bool? enabled = null;
			int? minInstances = null;
			if (IncludeEnabledCheckBox.CheckState == CheckState.Checked) enabled = true;
			if (IncludeEnabledCheckBox.CheckState == CheckState.Unchecked) enabled = false;
			if (MinimumInstanceCountNumericUpDown.Value > 0)
			{
				minInstances = (int)MinimumInstanceCountNumericUpDown.Value;
			}
			ws.GetProgramsCompleted += ws_GetProgramsCompleted;
			ws.GetProgramsAsync(enabled, minInstances);
		}

		void ws_GetProgramsCompleted(object sender, ResultEventArgs e)
		{
			MainForm.Current.LoadingCircle = false;
			if (e.Error != null)
			{
				MainForm.Current.UpdateHelpHeader(e.Error.Message, MessageBoxIcon.Error);
				throw e.Error;
			}
			else if (e.Result == null)
			{
				MainForm.Current.UpdateHelpHeader("", MessageBoxIcon.Error);
			}
			else
			{
				SettingsFile.Current.Games.Clear();
				var result = (List<x360ce.Engine.Data.Program>)e.Result;
				foreach (var item in result) SettingsFile.Current.Programs.Add(item);
				var header = string.Format("{0: yyyy-MM-dd HH:mm:ss}: '{1}' program(s) loaded.", DateTime.Now, result.Count());
				MainForm.Current.UpdateHelpHeader(header, MessageBoxIcon.Information);
			}
		}

		void ProgramsDataGridView_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
		{
			var grid = (DataGridView)sender;
			var setting = ((x360ce.Engine.Data.Game)grid.Rows[e.RowIndex].DataBoundItem);
			var isCurrent = CurrentGame != null && setting.GameId == CurrentGame.GameId;
			//if (e.ColumnIndex == grid.Columns[ProgramIdColumn.Name].Index)
			//{
			//	UpdateCellStyle(grid, e, SettingSelection == null ? null : (Guid?)SettingSelection.PadSettingChecksum);
			//}
			//else
			if (e.ColumnIndex == grid.Columns[MyIconColumn.Name].Index)
			{
				e.Value = isCurrent ? SaveButton.Image : Properties.Resources.empty_16x16;
			}
			else
			{
				//var row = grid.Rows[e.RowIndex].Cells[MyIconColumn.Name];
				//e.CellStyle.BackColor = isCurrent ? currentColor : grid.DefaultCellStyle.BackColor;
			}
		}

		void ProgramsDataGridView_SelectionChanged(object sender, EventArgs e)
		{
			// List can't be empty, so return.
			// Issue: When Datasource is set then DataGridView fires the selectionChanged 3 times & it selects the first row. 
			if (GamesDataGridView.SelectedRows.Count == 0) return;
			var row = GamesDataGridView.SelectedRows.Cast<DataGridViewRow>().FirstOrDefault();
			var item = (x360ce.Engine.Data.Game)row.DataBoundItem;
			CurrentGame = item;
		}

		void InitDefaultList()
		{
			SettingsFile.Current.Load();
			GamesDataGridView.DataSource = SettingsFile.Current.Games;
		}

		void ProgramsDataGridView_DataSourceChanged(object sender, EventArgs e)
		{
		}

		//void UpdateCellStyle(DataGridView grid, DataGridViewCellFormattingEventArgs e, Guid? checksum)
		//{
		//	var v = e.Value.ToString().Substring(0, 8).ToUpper();
		//	var s = checksum.HasValue ? checksum.Value.ToString().Substring(0, 8).ToUpper() : null;
		//	var match = v == s;
		//	e.Value = e.Value.ToString().Substring(0, 8).ToUpper();
		//	e.CellStyle.BackColor = match ? System.Drawing.Color.FromArgb(255, 222, 225, 231) : e.CellStyle.BackColor = grid.DefaultCellStyle.BackColor;
		//}


		//	InstallFilesX360ceCheckBox.Checked = System.IO.File.Exists(SettingManager.IniFileName);
		//InstallFilesXinput13CheckBox.Checked = System.IO.File.Exists(dllFile3);
		//InstallFilesX360ceCheckBox.Enabled = IsFileSame(SettingManager.IniFileName);
		//InstallFilesXinput910CheckBox.SuspendLayout();
		//InstallFilesXinput11CheckBox.SuspendLayout();
		//InstallFilesXinput12CheckBox.SuspendLayout();
		//InstallFilesXinput910CheckBox.Checked = System.IO.File.Exists(dllFile0);
		//InstallFilesXinput11CheckBox.Checked = System.IO.File.Exists(dllFile1);
		//InstallFilesXinput12CheckBox.Checked = System.IO.File.Exists(dllFile2);
		//InstallFilesXinput910CheckBox.ResumeLayout(false);
		//InstallFilesXinput11CheckBox.ResumeLayout(false);
		//InstallFilesXinput12CheckBox.ResumeLayout(false);
		////InstallFilesXinput910CheckBox.Enabled = IsFileSame(dllFile0);
		////InstallFilesXinput11CheckBox.Enabled = IsFileSame(dllFile1);
		////InstallFilesXinput12CheckBox.Enabled = IsFileSame(dllFile2);
		//InstallFilesXinput13CheckBox.Enabled = IsFileSame(dllFile3);

		private void Xinput91CheckBox_CheckedChanged(object sender, EventArgs e)
		{
			Helper.CreateDllFile(Xinput91CheckBox.Checked, Helper.dllFile0);
		}

		private void Xinput11CheckBox_CheckedChanged(object sender, EventArgs e)
		{
			Helper.CreateDllFile(Xinput11CheckBox.Checked, Helper.dllFile1);
		}

		private void Xinput12CheckBox_CheckedChanged(object sender, EventArgs e)
		{
			Helper.CreateDllFile(Xinput12CheckBox.Checked, Helper.dllFile2);
		}

		private void Xinput13CheckBox_CheckedChanged(object sender, EventArgs e)
		{
			Helper.CreateDllFile(Xinput13CheckBox.Checked, Helper.dllFile3);
		}

		private void Xinput14CheckBox_CheckedChanged(object sender, EventArgs e)
		{
			Helper.CreateDllFile(Xinput14CheckBox.Checked, Helper.dllFile4);
		}

		private void RefreshButton_Click(object sender, EventArgs e)
		{
			//var ws = new WebServiceClient();
			//ws.Url = MainForm.Current.OptionsPanel.InternetDatabaseUrlTextBox.Text;
			//ws.LoadSettingCompleted += ws_LoadSettingCompleted;
			//ws.LoadSettingAsync(new Guid[] { new Guid("45dec622-d819-2fdc-50a1-34bdf63647fb") }, null);

		}

		void ws_LoadSettingCompleted(object sender, ResultEventArgs e)
		{
			//var x = e;
		}

		private void ExportButton_Click(object sender, EventArgs e)
		{

		}

		private void BrowseButton_Click(object sender, EventArgs e)
		{
			var path = "";
			GameApplicationOpenFileDialog.DefaultExt = ".exe";
			if (!string.IsNullOrEmpty(GameApplicationLocationTextBox.Text))
			{
				var fi = new System.IO.FileInfo(GameApplicationLocationTextBox.Text);
				if (string.IsNullOrEmpty(path)) path = fi.Directory.FullName;
				GameApplicationOpenFileDialog.FileName = fi.Name;
			}
			GameApplicationOpenFileDialog.Filter = Helper.GetFileDescription(".exe") + " (*.exe)|*.exe|All files (*.*)|*.*";
			GameApplicationOpenFileDialog.FilterIndex = 1;
			GameApplicationOpenFileDialog.RestoreDirectory = true;
			if (string.IsNullOrEmpty(path)) path = System.IO.Path.GetPathRoot(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles));
			GameApplicationOpenFileDialog.InitialDirectory = path;
			GameApplicationOpenFileDialog.Title = "Browse for Executable";
			var result = GameApplicationOpenFileDialog.ShowDialog();
			if (result == System.Windows.Forms.DialogResult.OK)
			{
				GameApplicationLocationTextBox.Text = GameApplicationOpenFileDialog.FileName;
				ProcessExecutable(GameApplicationOpenFileDialog.FileName);
			}
		}

		private void ProgramOpenFileDialog_FileOk(object sender, CancelEventArgs e)
		{

		}

		void ProcessExecutable(string filePath)
		{
			var fi = new System.IO.FileInfo(filePath);
			if (!fi.Exists) return;
			// Check if item already exists.
			var game = SettingsFile.Current.Games.FirstOrDefault(x => x.FileName == fi.Name);
			if (game == null)
			{
				game = x360ce.Engine.Data.Game.FromDisk(fi.FullName);
				// Load default settings.
				var program = SettingsFile.Current.Programs.FirstOrDefault(x => x.FileName == game.FileName);
				game.LoadDefault(program);
				SettingsFile.Current.Games.Add(game);
			}
			else
			{
				game.FullPath = fi.FullName;
			}
			SettingsFile.Current.Save();
		}

	}
}
