using System;
using System.Reactive.Linq;
using Avalonia.ReactiveUI;
using PingUI.ViewModels;
using ReactiveUI;

namespace PingUI.Views;

public partial class TargetTagView : ReactiveUserControl<TargetTagViewModel>
{
	public TargetTagView()
	{
		InitializeComponent();
	}
}
