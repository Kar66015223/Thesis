/// <summary>Anything that reacts to a flashbang. intensity: 0 (edge of the blast) .. 1 (point blank).</summary>
public interface IFlashable
{
    void ApplyFlash(float intensity);
}
