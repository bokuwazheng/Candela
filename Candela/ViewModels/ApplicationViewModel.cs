using Prism.Commands;
using System.Collections.Generic;

namespace Candela.ViewModels
{
    public class ApplicationViewModel : BaseViewModel
    {
        public ApplicationViewModel()
        {
            ViewModels.Add(new MainViewModel());
            CurrentViewModel = ViewModels[0];
        }

        private List<BaseViewModel> _viewModels;
        public List<BaseViewModel> ViewModels
        {
            get
            {
                if (_viewModels == null) _viewModels = new List<BaseViewModel>();
                return _viewModels;
            }
        }

        private BaseViewModel _currentViewModel;
        public BaseViewModel CurrentViewModel
        {
            get => _currentViewModel;
            set
            {
                if (_currentViewModel != value)
                {
                    _currentViewModel = value;
                    RaisePropertyChanged("CurrentViewModel");
                }
            }
        }

        public DelegateCommand<string> ChangeViewModelCommand => new DelegateCommand<string>(s =>
        {
            switch (s)
            {
                case (NavigateTo.Main):
                    CurrentViewModel = ViewModels[0];
                    break;
                case (NavigateTo.Settings):
                    CurrentViewModel = ViewModels[1];
                    break;
                case (NavigateTo.About):
                    CurrentViewModel = ViewModels[2];
                    break;
            }
        });

        public static class NavigateTo
        {
            public const string Main = "Main";
            public const string Settings = "Settings";
            public const string About = "About";
        }

        public string ToMain => NavigateTo.Main;
        public string ToSettings => NavigateTo.Settings;
        public string ToAbout => NavigateTo.About;
    }
}
