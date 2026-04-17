using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace MeteoApp
{
	// Base class for all ViewModels — provides INotifyPropertyChanged support
	public abstract class BaseViewModel : INotifyPropertyChanged
	{
		public event PropertyChangedEventHandler PropertyChanged;

		protected BaseViewModel()
		{
		}

		// CallerMemberName automatically injects the calling property's name
		protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
		}
	}
}