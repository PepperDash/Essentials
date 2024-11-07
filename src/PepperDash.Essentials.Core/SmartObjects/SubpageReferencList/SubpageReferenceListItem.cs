namespace PepperDash.Essentials.Core.SmartObjects.SubpageReferencList
{
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
		/// Called by SRL to release all referenced objects
		/// </summary>
		public virtual void Clear()
		{
		}

		public virtual void Refresh() { }
	}
}