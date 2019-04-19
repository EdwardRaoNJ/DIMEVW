﻿using Knowte.Common.Prism;
using Knowte.Common.Services.Note;
using Prism.Events;
using Prism.Mvvm;

namespace Knowte.NotesModule.ViewModels
{
    public class NotesSubMenuViewModel : BindableBase
    {
        private IEventAggregator eventAggregator;
        private INoteService noteService;
        private int allNotesCounter;
        private int todayNotesCounter;
        private int yesterdayNotesCounter;
        private int thisWeekNotesCounter;
        private int flaggedCounter;
       
        public int AllNotesCounter
        {
            get { return this.allNotesCounter; }
            set { SetProperty<int>(ref this.allNotesCounter, value); }
        }

        public int TodayNotesCounter
        {
            get { return this.todayNotesCounter; }
            set { SetProperty<int>(ref this.todayNotesCounter, value); }
        }

        public int YesterdayNotesCounter
        {
            get { return this.yesterdayNotesCounter; }
            set { SetProperty<int>(ref this.yesterdayNotesCounter, value); }
        }

        public int ThisWeekNotesCounter
        {
            get { return this.thisWeekNotesCounter; }
            set { SetProperty<int>(ref this.thisWeekNotesCounter, value); }
        }

        public int FlaggedCounter
        {
            get { return this.flaggedCounter; }
            set { SetProperty<int>(ref this.flaggedCounter, value); }
        }
    
        public NotesSubMenuViewModel(IEventAggregator eventAggregator, INoteService noteService)
        {
            this.eventAggregator = eventAggregator;
            this.noteService = noteService;

            this.eventAggregator.GetEvent<CountNotesEvent>().Subscribe((i) =>
            {
                this.noteService.CountNotes(ref this.allNotesCounter, ref this.todayNotesCounter, ref this.yesterdayNotesCounter, ref this.thisWeekNotesCounter, ref this.flaggedCounter);

                OnPropertyChanged(() => this.AllNotesCounter);
                OnPropertyChanged(() => this.TodayNotesCounter);
                OnPropertyChanged(() => this.YesterdayNotesCounter);
                OnPropertyChanged(() => this.ThisWeekNotesCounter);
                OnPropertyChanged(() => this.FlaggedCounter);
            });
        }
    }
}
