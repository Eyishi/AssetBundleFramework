namespace AssetBundleFramework.Core.Resource
{
    internal class Resource : AResource
    {
        public override bool keepWaiting => !done;
    }
}