﻿using Knowte.Common.Prism;
using Prism.Commands;
using Prism.Mvvm;
using Prism.Regions;

namespace Knowte.SettingsModule.ViewModels
{
    public class SettingsViewModel : BindableBase
    {
        private IRegionManager regionManager;
        private int slideInFrom;
        private int previousIndex;
     
        public DelegateCommand<string> NavigateBetweenSettingsCommand;
   
        public int SlideInFrom
        {
            get { return this.slideInFrom; }
            set { SetProperty<int>(ref this.slideInFrom, value); }
        }
     
        public SettingsViewModel(IRegionManager regionManager)
        {
            this.regionManager = regionManager;
            this.NavigateBetweenSettingsCommand = new DelegateCommand<string>((index) => this.NavigateBetweenSettings(index));
            ApplicationCommands.NavigateBetweenSettingsCommand.RegisterCommand(this.NavigateBetweenSettingsCommand);

            this.SlideInFrom = 50;
        }
        
        private void NavigateBetweenSettings(string index)
        {
            if (string.IsNullOrWhiteSpace(index)) return;

            int localIndex = 0;

            int.TryParse(index, out localIndex);

            if (localIndex == 0) return;

            this.SlideInFrom = localIndex <= this.previousIndex ? -10 : 50;

            this.previousIndex = localIndex;

            switch (localIndex)
            {
                case 1:
                    this.regionManager.RequestNavigate(RegionNames.SettingsRegion, typeof(Views.SettingsAppearance).FullName);
                    break;
                case 2:
                    this.regionManager.RequestNavigate(RegionNames.SettingsRegion, typeof(Views.SettingsNotes).FullName);
                    break;
                case 3:
                    this.regionManager.RequestNavigate(RegionNames.SettingsRegion, typeof(Views.SettingsAdvanced).FullName);
                    break;
                default:
                    break;
            }
        }
    }
}