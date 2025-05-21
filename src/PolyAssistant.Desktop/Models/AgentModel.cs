using CommunityToolkit.Mvvm.ComponentModel;
using PolyAssistant.Core.Agents.Interfaces;
using PolyAssistant.Desktop.Components;

namespace PolyAssistant.Desktop.Models;

public partial class AgentModel(IAgent agent) : Model
{
    [ObservableProperty] private bool _isSelected;

    public IAgent Agent { get; } = agent;
}