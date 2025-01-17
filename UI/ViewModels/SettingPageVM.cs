﻿using Core.Librarys;
using Core.Models.Config;
using Core.Models.Config.Link;
using Core.Servicers.Instances;
using Core.Servicers.Interfaces;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using UI.Controls;
using UI.Models;
using UI.Servicers;

namespace UI.ViewModels
{
    public class SettingPageVM : SettingPageModel
    {
        private ConfigModel config;
        private readonly IAppConfig appConfig;
        private readonly MainViewModel mainVM;
        private readonly IData data;
        private readonly IWebData _webData;
        private readonly IUIServicer _uiServicer;
        public Command OpenURL { get; set; }
        public Command CheckUpdate { get; set; }
        public Command DelDataCommand { get; set; }
        public Command ExportDataCommand { get; set; }

        public SettingPageVM(IAppConfig appConfig, MainViewModel mainVM, IData data, IWebData webData, IUIServicer uiServicer_)
        {
            this.appConfig = appConfig;
            this.mainVM = mainVM;
            this.data = data;
            _webData = webData;
            _uiServicer = uiServicer_;

            OpenURL = new Command(new Action<object>(OnOpenURL));
            CheckUpdate = new Command(new Action<object>(OnCheckUpdate));
            DelDataCommand = new Command(new Action<object>(OnDelData));
            ExportDataCommand = new Command(new Action<object>(OnExportData));

            Init();
        }



        private void OnCheckUpdate(object obj)
        {
            try
            {
                CheckUpdateBtnVisibility = System.Windows.Visibility.Collapsed;

                string appInstallDirectory = AppDomain.CurrentDomain.BaseDirectory;

                // 更新程序的相对路径
                string updaterExePath = Path.Combine(appInstallDirectory, "Updater.exe");
                string updaterCacheExePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "Tai", "Update", "Updater.exe");
                string updateDirPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "Tai", "Update");

                if (!Directory.Exists(updateDirPath))
                {
                    Directory.CreateDirectory(updateDirPath);
                }

                if (!File.Exists(updaterExePath))
                {
                    mainVM.Toast("升级程序似乎已被删除，请手动前往发布页查看新版本", Controls.Window.ToastType.Error, Controls.Base.IconTypes.None);
                    return;
                }

                File.Copy(updaterExePath, updaterCacheExePath, true);
                File.Copy(Path.Combine(appInstallDirectory, "Newtonsoft.Json.dll"), Path.Combine(updateDirPath, "Newtonsoft.Json.dll"), true);

                // 使用相对路径启动Updater.exe
                ProcessHelper.Run(Path.Combine(updateDirPath, "Updater.exe"), new string[] { Version });
            }
            catch (Exception ex)
            {
                CheckUpdateBtnVisibility = System.Windows.Visibility.Visible;

                Logger.Error(ex.Message);
                mainVM.Toast("无法正确启动检查更新程序", Controls.Window.ToastType.Error, Controls.Base.IconTypes.None);
            }
        }


        private void OnOpenURL(object obj)
        {
            Process.Start(new ProcessStartInfo(obj.ToString()));
        }

        private void Init()
        {
            config = appConfig.GetConfig();

            Data = config.General;

            TabbarData = new System.Collections.ObjectModel.ObservableCollection<string>()
            {
                "常规","关联","行为","数据","关于"
            };

            PropertyChanged += SettingPageVM_PropertyChanged;

            Version = Assembly.GetExecutingAssembly().GetName().Version.ToString();

            DelDataStartMonthDate = DateTime.Now;
            DelDataEndMonthDate = DateTime.Now;

            ExportDataStartMonthDate = DateTime.Now;
            ExportDataEndMonthDate = DateTime.Now;
        }



        private void SettingPageVM_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(Data))
            {
                if (TabbarSelectedIndex == 0)
                {
                    config.General = Data as GeneralModel;
                }
                else if (TabbarSelectedIndex == 1)
                {
                    if (Data != null)
                    {
                        var newData = new List<LinkModel>();
                        foreach (var item in Data as IEnumerable<object>)
                        {
                            newData.Add(item as LinkModel);
                            Debug.WriteLine(item.ToString());
                        }
                        config.Links = newData;
                    }

                }
                else if (TabbarSelectedIndex == 2)
                {
                    config.Behavior = Data as BehaviorModel;
                }

                appConfig.Save();
            }

            if (e.PropertyName == nameof(TabbarSelectedIndex))
            {
                if (TabbarSelectedIndex == 0)
                {
                    //  常规
                    Data = config.General;
                }
                else if (TabbarSelectedIndex == 1)
                {
                    //  关联
                    Data = config.Links;
                }
                else if (TabbarSelectedIndex == 2)
                {
                    //  行为
                    Data = config.Behavior;
                }
            }
        }

        private async void OnDelData(object obj)
        {
            if (DelDataStartMonthDate > DelDataEndMonthDate)
            {
                mainVM.Toast("时间范围选择错误", Controls.Window.ToastType.Error, Controls.Base.IconTypes.IncidentTriangle);
                return;
            }

            bool isConfirm = await _uiServicer.ShowConfirmDialogAsync("删除确认", "是否执行此操作？");
            if (isConfirm)
            {
                data.ClearRange(DelDataStartMonthDate, DelDataEndMonthDate);
                _webData.Clear(DelDataStartMonthDate, DelDataEndMonthDate);
                mainVM.Toast("操作已完成", Controls.Window.ToastType.Success);
            }
        }

        private void OnExportData(object obj)
        {
            try
            {
                FolderBrowserDialog dialog = new FolderBrowserDialog();
                dialog.Description = "请选择导出位置";
                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    data.ExportToExcel(dialog.SelectedPath, ExportDataStartMonthDate, ExportDataEndMonthDate);
                    _webData.Export(dialog.SelectedPath, ExportDataStartMonthDate, ExportDataEndMonthDate);
                    mainVM.Toast("导出数据完成", Controls.Window.ToastType.Success);
                }
            }
            catch (Exception ec)
            {
                Logger.Error(ec.ToString());
                mainVM.Toast("导出数据失败", Controls.Window.ToastType.Error, Controls.Base.IconTypes.IncidentTriangle);
            }
        }
    }
}
