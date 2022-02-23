namespace Pglet
{
    public class Persona : Control
    {
        protected override string ControlName => "persona";

        public string ImageUrl
        {
            get { return GetAttr("imageUrl"); }
            set { SetAttr("imageUrl", value); }
        }

        public string ImageAlt
        {
            get { return GetAttr("imageAlt"); }
            set { SetAttr("imageAlt", value); }
        }

        public PersonaInitialsColor InitialsColor
        {
            get { return GetEnumAttr<PersonaInitialsColor>("initialsColor"); }
            set { SetEnumAttr("initialsColor", value); }
        }

        public PersonaSize Size
        {
            get { return GetEnumAttr<PersonaSize>("size"); }
            set { SetEnumAttr("size", value); }
        }

        public string InitialsTextColor
        {
            get { return GetAttr("initialsTextColor"); }
            set { SetAttr("initialsTextColor", value); }
        }

        public string Text
        {
            get { return GetAttr("text"); }
            set { SetAttr("text", value); }
        }

        public string SecondaryText
        {
            get { return GetAttr("secondaryText"); }
            set { SetAttr("secondaryText", value); }
        }

        public string TertiaryText
        {
            get { return GetAttr("tertiaryText"); }
            set { SetAttr("tertiaryText", value); }
        }

        public string OptionalText
        {
            get { return GetAttr("optionalText"); }
            set { SetAttr("optionalText", value); }
        }

        public PersonaPresense Presence
        {
            get { return GetEnumAttr<PersonaPresense>("presence"); }
            set { SetEnumAttr("presence", value); }
        }

        public bool HideDetails
        {
            get { return GetBoolAttr("hideDetails"); }
            set { SetBoolAttr("hideDetails", value); }
        }
    }
}
