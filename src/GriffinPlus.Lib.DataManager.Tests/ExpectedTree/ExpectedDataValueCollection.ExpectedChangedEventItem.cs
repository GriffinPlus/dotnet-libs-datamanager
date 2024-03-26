﻿namespace GriffinPlus.Lib.DataManager;

partial class ExpectedDataValueCollection
{
	/// <summary>
	/// An item storing the expected sender and the expected event arguments of an event invocation.
	/// </summary>
	/// <param name="eventName">Name of the event.</param>
	/// <param name="sender">The sender of the event invocation.</param>
	/// <param name="eventArgs">The arguments of the event invocation.</param>
	public sealed class ExpectedChangedEventItem(
		string                                      eventName,
		object                                      sender,
		ExpectedDataValueCollectionChangedEventArgs eventArgs)
	{
		public readonly string                                      EventName = eventName;
		public readonly object                                      Sender    = sender;
		public readonly ExpectedDataValueCollectionChangedEventArgs EventArgs = eventArgs;
	}
}
