namespace GriffinPlus.Lib.DataManager;

partial class ExpectedDataValue<T>
{
	/// <summary>
	/// An item storing the sender and the event arguments of an event invocation.
	/// </summary>
	/// <param name="eventName">Name of the invoked event.</param>
	/// <param name="sender">The sender of the event invocation.</param>
	/// <param name="eventArgs">The arguments of the event invocation.</param>
	public sealed class ExpectedChangedEventItem(
		string                               eventName,
		object                               sender,
		ExpectedDataValueChangedEventArgs<T> eventArgs)
	{
		public readonly string                               EventName = eventName;
		public readonly object                               Sender    = sender;
		public readonly ExpectedDataValueChangedEventArgs<T> EventArgs = eventArgs;
	}
}
