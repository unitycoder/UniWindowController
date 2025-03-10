﻿using System;
using System.Windows.Forms;
using System.Linq;
using UnityEngine;
using Kirurobo;
using System.Drawing;

namespace TestLibUniWinC
{
    public partial class FormMain : Form
    {
        UniWinCore uniwinc;

        Vector2 relativeWindowPosition = Vector2.zero;
        
        /// <summary>
        /// ウィンドウをドラッグ中なら true
        /// </summary>
        bool isDragging = false;


        /// <summary>
        /// 値を変更中で、GUI操作を反映させたくないときtrueとする
        /// </summary>
        bool isAplying = false;


        public FormMain()
        {
            InitializeComponent();

            InitializeControls();
        }

        /// <summary>
        /// 各コントロールについて追加の初期化処理
        /// </summary>
        private void InitializeControls()
        {
            comboBoxTransparentType.Items.Add(UniWinCore.TransparentType.Alpha);
            comboBoxTransparentType.Items.Add(UniWinCore.TransparentType.ColorKey);
            comboBoxTransparentType.Items.Add(UniWinCore.TransparentType.None);
            comboBoxTransparentType.SelectedIndex = 0;
            comboBoxTransparentType.SelectedIndexChanged += comboBoxTransparentType_SelectedIndexChanged;

            // 文字や背景が黒だと透けてしまうのは防げていない…
            textBoxMessage.BackColor = Color.FromArgb(0xFF, 0x33, 0x33, 0x33);
            //OpaqueAllTextColor(this);

            // グループボックスでもドラッグでウィンドウ移動ができるようにしておく
            groupBoxWindowSettings.MouseDown += FormMain_MouseDown;
            groupBoxWindowSettings.MouseMove += FormMain_MouseMove;
            groupBoxWindowSettings.MouseUp += FormMain_MouseUp;

            groupBoxFileHandling.MouseDown += FormMain_MouseDown;
            groupBoxFileHandling.MouseMove += FormMain_MouseMove;
            groupBoxFileHandling.MouseUp += FormMain_MouseUp;

            groupBoxInformation.MouseDown += FormMain_MouseDown;
            groupBoxInformation.MouseMove += FormMain_MouseMove;
            groupBoxInformation.MouseUp += FormMain_MouseUp;
        }

        //private void OpaqueAllTextColor(Control currentControl)
        //{
        //    foreach (Control control in currentControl.Controls)
        //    {
        //        if (control.HasChildren)
        //        {
        //            OpaqueAllTextColor(control);
        //        }
        //        var color = control.ForeColor;
        //        control.ForeColor = System.Drawing.Color.FromArgb(
        //            0xFF, color.R, color.G, color.B
        //            );
        //    }
        //}

        private void FormMain_Shown(object sender, EventArgs e)
        {
            uniwinc = new UniWinCore();
            uniwinc.AttachMyWindow();

            UpdateMonitorCombobox();    // 初回の一覧取得

            //  モニタ一覧を表示
            PrintMonitorInfo();

            // 定期的にフラグを監視して処理
            timerMainLoop.Start();
        }

        /// <summary>
        /// モニタ一覧を更新
        /// </summary>
        private void UpdateMonitorCombobox()
        {
            int count = uniwinc.GetMonitorCount();
            int index = comboBoxFitMonitor.SelectedIndex;

            comboBoxFitMonitor.Items.Clear();

            for (int i = 0; i < count; i++)
            {
                comboBoxFitMonitor.Items.Add($"Monitor {i}");
            }

            if (index >= count) index = count - 1;
            if (index < 0)
            {
                index = 0;
            } else
            {
                comboBoxFitMonitor.SelectedIndex = index;
            }
        }

        /// <summary>
        /// 現在接続されているモニタを列挙
        /// </summary>
        private void PrintMonitorInfo()
        {
            int monitors = uniwinc.GetMonitorCount();

            int currentMonitorIndex = uniwinc.GetCurrentMonitor();

            string message = "Current monitor: " + currentMonitorIndex + "\r\n";

            for (int i = 0; i < monitors; i++)
            {
                Vector2 pos, size;
                bool result = uniwinc.GetMonitorRectangle(i, out pos, out size);

                message += String.Format(
                    "Monitor {0}: X:{1}, Y:{2} - W:{3}, H:{4}\r\n",
                    i, pos.x, pos.y, size.x, size.y
                    );
            }
            Console.WriteLine(message);
            textBoxMessage.Text = message;
        }

        private void PrintWindowInfo()
        {
            var pos = uniwinc.GetWindowPosition();
            var size = uniwinc.GetWindowSize();
            //var hwnd = UniWinC.GetWindowHandle();
            //var hdesktop = UniWinC.GetDesktopWindowHandle();
            //var pid = UniWinC.GetMyProcessId();
            var myPid = System.Diagnostics.Process.GetCurrentProcess().Id;
            var clientSize = this.ClientSize;

            string message = String.Format(
                "Pos. {0}, {1}\r\nSize {2}, {3}\r\nClient {4}, {5}\r\nhWnd {6:X}\r\nPID {9}\r\n",
                pos.x, pos.y, size.x, size.y, clientSize.Width, clientSize.Height,
                "", this.Handle.ToInt32(),
                "", myPid
                );


            Console.WriteLine(message);
            textBoxMessage.Text = message;
        }

        private void DumpStringArray(string[] array)
        {
            string text = String.Join(Environment.NewLine, array);

            Console.WriteLine("Count: " + array.Length);
            Console.WriteLine(text);
            textBoxMessage.Text = text;
        }

        private void buttonCheck_Click(object sender, EventArgs e)
        {
            PrintWindowInfo();
        }

        private void buttonOpenFile_Click(object sender, EventArgs e)
        {
            Kirurobo.FilePanel.Settings ds = new Kirurobo.FilePanel.Settings();
            ds.title = "Open files";
            //ds.filter = "Image files (*.png,*.jpg,*.jpeg,*.tiff)|*.png;*.jpg;*.jpeg;*.tiff|All files (*.*)|*.*";
            ds.filters = new FilePanel.Filter[] {
                new FilePanel.Filter("Image files (*.png; *.jpg; *.tiff)", "png", "jpg", "jpeg", "tiff"),
                new FilePanel.Filter("All files (*.*)", "*"),
            };
            ds.initialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyPictures);
            //ds.initialFile = "D:\\tmp\\TEST";
            ds.initialFile = "TEST";
            ds.flags = FilePanel.Flag.AllowMultipleSelection | FilePanel.Flag.FolderMustExist;
            //ds.flags = FilePanel.Flag.PathMustExist;
            Kirurobo.FilePanel.OpenFilePanel(ds, (files)=> { DumpStringArray(files); });
        }

        private void buttonSaveFile_Click(object sender, EventArgs e)
        {
            Kirurobo.FilePanel.Settings ds = new Kirurobo.FilePanel.Settings();
            ds.title = "Save file (Actually not be written)";
            //ds.filters = new FilePanel.Filter[] {
            //    new FilePanel.Filter("Plain text (*.txt)", "txt"),
            //    new FilePanel.Filter("Word documents (*.doc; *.docx; *.docm)", "doc", "docx", "docm"),
            //    new FilePanel.Filter("All files (*.*)", "*"),
            //    //new FilePanel.Filter("All files", "*"),
            //};
            ds.initialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            ds.initialFile = "Test";
            ds.flags = FilePanel.Flag.AllowMultipleSelection | FilePanel.Flag.CreatePrompt | FilePanel.Flag.OverwritePrompt | FilePanel.Flag.FolderMustExist;
            //ds.flags = FilePanel.Flag.PathMustExist;
            Kirurobo.FilePanel.SaveFilePanel(ds, (files) => { DumpStringArray(files); });
        }

        /// <summary>
        /// 選択されたモニタにウィンドウを移動
        /// </summary>
        private void FitToMonitor(int monitor)
        {
            uniwinc.FitToMonitor(monitor);
        }


        private void checkBoxTransparent_CheckedChanged(object sender, EventArgs e)
        {
            if (isAplying) return;

            isAplying = true;
            uniwinc.EnableTransparent(checkBoxTransparent.Checked);
            isAplying = false;
        }

        private void checkBoxTopmost_CheckedChanged(object sender, EventArgs e)
        {
            if (isAplying) return;

            isAplying = true;
            checkBoxBottommost.Checked = false;
            uniwinc.EnableTopmost(checkBoxTopmost.Checked);
            isAplying = false;
        }

        private void checkBoxBottommost_CheckedChanged(object sender, EventArgs e)
        {
            if (isAplying) return;

            isAplying = true;
            checkBoxTopmost.Checked = false;
            uniwinc.EnableBottommost(checkBoxBottommost.Checked);
            isAplying = false;
        }

        private void checkBoxAllowDrop_CheckedChanged(object sender, EventArgs e)
        {
            if (isAplying) return;

            isAplying = true;
            uniwinc.SetAllowDrop(checkBoxAllowDrop.Checked);
            isAplying = false;
        }

        private void FormMain_Resize(object sender, EventArgs e)
        {
            //Console.WriteLine("Resized!");
        }

        private void buttonShowMonitorInfo_Click(object sender, EventArgs e)
        {
            PrintMonitorInfo();
        }

        private void buttonFitMonitor_Click(object sender, EventArgs e)
        {
            FitToMonitor(comboBoxFitMonitor.SelectedIndex);
            PrintWindowInfo();
        }

        private void PerformEvent()
        {
            string[] files;

            // ウインドウスタイルの変化
            if (uniwinc.ObserveWindowStyleChanged(out var type))
            {
                Console.WriteLine($"Style changed: {type}");
            }

            // ファイルドロップ時、その内容を出力
            if (uniwinc.ObserveDroppedFiles(out files))
            {
                // ファイルがドロップされた後の処理
                string text = String.Join(Environment.NewLine, files);
                Console.WriteLine("Drop");
                Console.WriteLine(text);
                textBoxMessage.Text = text;
            }

            // 解像度変更時、モニター一覧を更新
            if (uniwinc.ObserveMonitorChanged()) {
                // 解像度が変化した後の処理
                UpdateMonitorCombobox();
            }
        }

        // UnityのUpdateの代わりに定期的に実行するメソッド
        private void timerMainLoop_Tick(object sender, EventArgs e)
        {
            uniwinc.Update();
            
            PerformEvent();
        }

        /// <summary>
        /// ウィンドウ透明度を変更
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void trackBarAlpha_Scroll(object sender, EventArgs e)
        {
            float alpha = (float)((TrackBar)sender).Value / 255.0f;
            uniwinc.SetAlphaValue(alpha);
        }

        private void FormMain_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                isDragging = true;
                relativeWindowPosition = uniwinc.GetWindowPosition() - uniwinc.GetCursorPosition();
            }
        }

        private void FormMain_MouseMove(object sender, MouseEventArgs e)
        {
            if (isDragging)
            {
                var windowPos = uniwinc.GetCursorPosition() + relativeWindowPosition;
                uniwinc.SetWindowPosition(windowPos );
            }
        }

        private void FormMain_MouseUp(object sender, MouseEventArgs e)
        {
            if ((e.Button & MouseButtons.Left) == MouseButtons.Left)
            {
                isDragging = false;
            }
        }

        private void comboBoxTransparentType_SelectedIndexChanged(object sender, EventArgs e)
        {
            var item = comboBoxTransparentType.SelectedItem;

            if ((uniwinc != null) && (item is UniWinCore.TransparentType))
            {
                var type = (UniWinCore.TransparentType)item;
                uniwinc.SetTransparentType(type);
            }
        }
    }
}
