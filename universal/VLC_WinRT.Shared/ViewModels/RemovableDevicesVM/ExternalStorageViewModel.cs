﻿/**********************************************************************
 * VLC for WinRT
 **********************************************************************
 * Copyright © 2013-2014 VideoLAN and Authors
 *
 * Licensed under GPLv2+ and MPLv2
 * Refer to COPYING file of the official project for license
 **********************************************************************/

using System;
using System.Collections.ObjectModel;
using System.Linq;
using Windows.Storage;
using VLC_WinRT.Commands.RemovableDevices;
using VLC_WinRT.Common;
using VLC_WinRT.Services.RunTime;
using VLC_WinRT.ViewModels.Others.VlcExplorer;
using System.Threading.Tasks;
using Autofac;
using VLC_WINRT.Common;
#if WINDOWS_APP
using Windows.Devices.Portable;
#endif

namespace VLC_WinRT.ViewModels.RemovableDevicesVM
{
    public class ExternalStorageViewModel : BindableBase, IDisposable
    {
        #region private props
        private ExternalDeviceService _deviceService;
        private FileExplorerViewModel _currentStorageVM;
        #endregion

        #region private fields
        private ObservableCollection<FileExplorerViewModel> _removableStorageVMs = new ObservableCollection<FileExplorerViewModel>();
        #endregion

        #region public props

        public RemovableDeviceClickedCommand RemovableDeviceClicked { get; }=new RemovableDeviceClickedCommand();

        public FileExplorerViewModel CurrentStorageVM
        {
            get
            {
                return _currentStorageVM;
            }
            set { SetProperty(ref _currentStorageVM, value); }
        }
        #endregion

        #region public fields
        public ObservableCollection<FileExplorerViewModel> RemovableStorageVMs
        {
            get { return _removableStorageVMs; }
            set { SetProperty(ref _removableStorageVMs, value); }
        }
        #endregion

        public ExternalStorageViewModel()
        {
            RemovableDeviceClicked = new RemovableDeviceClickedCommand();
#if WINDOWS_APP
            _deviceService = App.Container.Resolve<ExternalDeviceService>();
            _deviceService.ExternalDeviceAdded += DeviceAdded;
            _deviceService.ExternalDeviceRemoved += DeviceRemoved;
#else
            Initialize();           
#endif
        }

        private async void Initialize()
        {
            var devices = KnownFolders.RemovableDevices;
            var cards = await devices.GetFoldersAsync();
            if (cards.Any())
            {
                var external = new FileExplorerViewModel(cards[0]);
                RemovableStorageVMs.Add(external);
                CurrentStorageVM = RemovableStorageVMs[0];
                await CurrentStorageVM.GetFiles();
            }
        }

#if WINDOWS_APP
        private async void DeviceAdded(object sender, string id)
        {
            await AddFolder(id);
        }

        private async Task AddFolder(string newId)
        {
            await DispatchHelper.InvokeAsync(() =>
            {
                if (RemovableStorageVMs.All(vm => vm.Id != newId))
                {
                    var external = new FileExplorerViewModel(StorageDevice.FromId(newId), newId);
                    RemovableStorageVMs.Add(external);
                }
                if (RemovableStorageVMs.Any())
                {
                    CurrentStorageVM = RemovableStorageVMs[0];
                }
            });
        }

        private async void DeviceRemoved(object sender, string id)
        {
            await DispatchHelper.InvokeAsync(() =>
            {
                FileExplorerViewModel removedViewModel = RemovableStorageVMs.FirstOrDefault(vm => vm.Id == id);
                if (removedViewModel != null)
                {
                    if (CurrentStorageVM == removedViewModel)
                    {
                        CurrentStorageVM.StorageItems.Clear();
                        CurrentStorageVM = null;
                    }
                    RemovableStorageVMs.Remove(removedViewModel);
                    GC.Collect();
                }
            });
        }
#endif

        public void Dispose()
        {
#if WINDOWS_APP
            _deviceService.ExternalDeviceAdded -= DeviceAdded;
            _deviceService.ExternalDeviceRemoved -= DeviceRemoved;
            _deviceService = null;
#endif
        }
    }
}
