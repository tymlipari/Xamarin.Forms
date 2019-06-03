namespace Xamarin.Forms
{
	public interface IResourcesProvider
	{
		bool IsResourcesCreated { get; }
		ResourceDictionary Resources { get; set; }
	}
}