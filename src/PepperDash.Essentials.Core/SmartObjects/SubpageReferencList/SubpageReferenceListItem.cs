namespace PepperDash.Essentials.Core.SmartObjects.SubpageReferencList
{
 /// <summary>
 /// Represents a SubpageReferenceListItem
 /// </summary>
	public class SubpageReferenceListItem
	{
		/// <summary>
		/// The list that this lives in
		/// </summary>
		protected SubpageReferenceList Owner;
		protected uint Index;

		public SubpageReferenceListItem(uint index, SubpageReferenceList owner)
		{
			Index = index;
			Owner = owner;
		}

  /// <summary>
  /// Clear method
  /// </summary>
  /// <inheritdoc />
		public virtual void Clear()
		{
		}

  /// <summary>
  /// Refresh method
  /// </summary>
		public virtual void Refresh() { }
	}
}