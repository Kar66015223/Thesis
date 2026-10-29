using System;

public abstract class Mission
{
    protected Mission(string name, string desc, MissionType type, string sceneName)
    {
        Name = name;
        Description = desc;
        Type = type;
        SceneName = sceneName;
    }
    
    public string Name { get; }
    public string Description { get; }
    public MissionType Type { get; }
    public MissionStatus Status { get; private set; } = MissionStatus.Pending;
    public string SceneName { get; }

    public bool IsCompleted => Status == MissionStatus.Completed || Status == MissionStatus.Submitted;
    public event Action<Mission> StatusChanged;

    public void Accept()
    {
        if (Status != MissionStatus.Pending)
            return;

        SetStatus(MissionStatus.OnProgress);
        OnAccepted();
    }

    public void Submit()
    {
        if (Status != MissionStatus.Completed)
            return;

        SetStatus(MissionStatus.Submitted);
        OnSubmitted();
    }

    protected void Complete()
    {
        if (Status != MissionStatus.OnProgress)
            return;

        SetStatus(MissionStatus.Completed);
        OnCompleted();
    }

    public void RestoreStatus(MissionStatus status)
    {
        if (status == Status)
            return;

        SetStatus(status);
        OnStatusRestored();
    }

    protected virtual void OnAccepted() {}
    protected virtual void OnCompleted() {}
    protected virtual void OnSubmitted() {}
    protected virtual void OnStatusRestored() {}
    private void SetStatus(MissionStatus status)
    {
        Status = status;
        StatusChanged?.Invoke(this);
    }
}