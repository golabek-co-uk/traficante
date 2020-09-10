namespace Traficante.Studio.ViewModels
{
    public class QueryLanguageRadioButtonModel
    {
        public QueryLanguageModel QueryLanguage { get; set; }
        public ISelectableObject Object { get; set; }

        public string Id => this.QueryLanguage.Id;
        public string Name => this.QueryLanguage.Name;
        public string Description => this.QueryLanguage.Description;
        public bool IsSelected
        {
            get => this.QueryLanguage == this.Object.QueryLanguage;
            set => this.Object.QueryLanguage = this.QueryLanguage;
        }

        public QueryLanguageRadioButtonModel(QueryLanguageModel queryLanguageModel, ISelectableObject objectModel)
        {
            this.QueryLanguage = queryLanguageModel;
            this.Object = objectModel;
        }
    }
}
