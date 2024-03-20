using System.Threading;

using GriffinPlus.Lib.DataManager.Viewer;

namespace GriffinPlus.Lib.DataManager;

partial class ExpectedDataValue<T>
{
	/// <summary>
	/// An item storing the sender and the event arguments of an event invocation.
	/// </summary>
	/// <param name="eventName">Name of the invoked event.</param>
	/// <param name="sender">The sender of the event invocation.</param>
	/// <param name="eventArgs">The arguments of the event invocation.</param>
	/// <param name="synchronizationContext">The synchronization context of the thread executing the event handler.</param>
	public sealed class ReceivedViewerUntypedChangedEventItem(
		string                                 eventName,
		object                                 sender,
		UntypedViewerDataValueChangedEventArgs eventArgs,
		SynchronizationContext                 synchronizationContext)
	{
		public readonly string                                 EventName              = eventName;
		public readonly object                                 Sender                 = sender;
		public readonly UntypedViewerDataValueChangedEventArgs EventArgs              = eventArgs;
		public readonly SynchronizationContext                 SynchronizationContext = synchronizationContext;
	}
}
