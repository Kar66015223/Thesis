/// <summary>One self-contained piece of HUD/UI logic that listens to events while bound.</summary>
public interface IUIPresenter
{
    void Bind();
    void Unbind();
}
