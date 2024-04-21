using System;
using System.Threading;
using System.Threading.Tasks;

namespace LedCube.PluginBase;

public interface IFrameGenerator : IDisposable
{
    static abstract FrameGeneratorInfo Info { get; }

    TimeSpan? FrameTime { get; }
    
    /// <summary>
    /// Initializes the Animation. Allows the Animation to do some preparation/initialization work to be done in advance and asynchronously.
    /// </summary>
    /// <param name="token">Cancellation Token if the Loading of the Animation has been interrupted.</param>
    /// <returns>Task with the asynchronous action.</returns>
    Task InitializeAsync(CancellationToken token) => Task.CompletedTask;

    /// <summary>
    /// Called every cycle to draw a new Frame.
    /// </summary>
    /// <param name="frameContext">Context of the Current Frame, with timing and drawing informations.</param>
    void DrawFrame(FrameContext frameContext);

    /// <summary>
    /// Called when the Animation has been started.
    /// </summary>
    /// <param name="animationContext">Current AnimationContext</param>
    void Start(AnimationContext animationContext) {}
    
    /// <summary>
    /// Called when the Animation has been stopped.
    /// </summary>
    /// <param name="animationContext">Current AnimationContext</param>
    void End(AnimationContext animationContext) {}
    
    /// <summary>
    /// Called when the Animation has been paused.
    /// </summary>
    /// <param name="animationContext">Current AnimationContext</param>
    void Pause(AnimationContext animationContext) {}
    
    /// <summary>
    /// Called when the Animation starts again, after it has been paused.
    /// </summary>
    /// <param name="animationContext">Current AnimationContext</param>
    void Continue(AnimationContext animationContext) {}
    
    /// <summary>
    /// Called when the time has been changed because the user scrubbed through the animation progression 
    /// </summary>
    /// <param name="animationContext">AnimationContext with the updated elapsed time parameter.</param>
    void ChangeTime(AnimationContext animationContext) {}

    // void OnKeyPressed();
}