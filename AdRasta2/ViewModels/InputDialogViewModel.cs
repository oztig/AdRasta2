using System;
using System.Reactive;
using System.Threading.Tasks;
using AdRasta2.ViewModels;
using ReactiveUI;

public class InputDialogViewModel : ViewModelBase
{
    private string _userPrompt;

    public string UserPrompt
    {
        get => _userPrompt;
        set => this.RaiseAndSetIfChanged(ref _userPrompt, value);
    }
    
    private string _userInput;
    public string UserInput
    {
        get => _userInput;
        set => this.RaiseAndSetIfChanged(ref _userInput, value);
    }

    private string _inputWatermark;

    public string InputWatermark
    {
        get => _inputWatermark;
        set => this.RaiseAndSetIfChanged(ref _inputWatermark, value);
    }
    
    private string _title;

    public string Title
    {
        get => _title;
        set => this.RaiseAndSetIfChanged(ref _title, value);
    }

    public ReactiveCommand<Unit, Unit> SubmitCommand { get; }
    public ReactiveCommand<Unit, Unit> CancelCommand { get; }


    public Action<bool?> CloseAction { get; set; }

    public InputDialogViewModel()
    {
        SubmitCommand = ReactiveCommand.Create(() =>  CloseAction?.Invoke(true));
        CancelCommand = ReactiveCommand.Create(() => CloseAction?.Invoke(false));
    }

}