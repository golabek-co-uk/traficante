using Traficante.Studio.Models;

namespace Traficante.Studio.ViewModels
{
    public interface ISelectableObject
    {
        IQueryableObjectModel Object { get; }
        public string Title { get; }
        public object Icon { get; }
        QueryLanguageModel QueryLanguage { get; set; }
        QueryLanguageRadioButtonModel[] QueryLanguages { get; }
    }
}
