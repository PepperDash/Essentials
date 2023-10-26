namespace PepperDash.Essentials.Core.Fusion
{
    public class FusionCustomProperty
    {
        public FusionCustomProperty()
        {
        }

        public FusionCustomProperty(string id)
        {
            ID = id;
        }

        public string ID { get; set; }
        public string CustomFieldName { get; set; }
        public string CustomFieldType { get; set; }
        public string CustomFieldValue { get; set; }
    }
}